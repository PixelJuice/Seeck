using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace dev.jonasjohansson.PathFinding
{
    public interface HeuristicCostCalculator
    {
        int CalculateHCost(int2 p_aPostiontion, int2 p_bPosition);
        int CalculateFCost(int p_gCost, int p_hCost);
    }

    public struct GridBasedManhattanHeuristicCalculator : HeuristicCostCalculator
    {
        static readonly int STRAIGHT_MOVE_COST = 10;
        static readonly int DIAGONAL_MOVE_COST = 14;

        public int CalculateHCost(int2 p_aPostiontion, int2 p_bPosition)
        {
            int xDistance = math.abs(p_aPostiontion.x - p_bPosition.x);
            int yDistance = math.abs(p_aPostiontion.y - p_bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            return DIAGONAL_MOVE_COST * math.min(xDistance, yDistance) + STRAIGHT_MOVE_COST * remaining;
        }

        public int CalculateFCost(int p_gCost, int p_hCost)
        {
            return p_gCost + p_hCost;
        }
    }

    public class PathFindingSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            return new Search<GridBasedManhattanHeuristicCalculator>()
            {
                waypointsBuffers = GetBufferFromEntity<Waypoints>(),
                Neighbours = GetBufferFromEntity<Neighbours>(true),
                GridPositions = GetComponentDataFromEntity<GridPosition>(true),
                NodeCosts = GetComponentDataFromEntity<NodeCost>(true),
            }.Schedule(this, inputDeps);
        }

        [BurstCompile]
        [RequireComponentTag(typeof(Waypoints))]
        struct Search<HeuristicCalculator> : IJobForEachWithEntity<PathRequest, PathInfo> where HeuristicCalculator : struct, HeuristicCostCalculator
        {
            [NativeDisableParallelForRestriction]
            [WriteOnly]
            public BufferFromEntity<Waypoints> waypointsBuffers;
            [ReadOnly]
            public BufferFromEntity<Neighbours> Neighbours;
            [ReadOnly]
            public ComponentDataFromEntity<GridPosition> GridPositions;
            [ReadOnly]
            public ComponentDataFromEntity<NodeCost> NodeCosts;

            HeuristicCalculator calculator;

            public void Execute(Entity p_entity, int p_index, ref PathRequest p_pathRequest, ref PathInfo p_pathInfo)
            {
                if(p_pathRequest.Processed)
                {
                    return;
                }
                if (p_pathRequest.StartPosition == p_pathRequest.EndPosition)
                {
                    p_pathInfo.waypoint = 0;
                    p_pathRequest.Processed = true;
                    return;
                }
                HeuristicCalculator calculator = new HeuristicCalculator();
                Entity startEntity = p_pathRequest.StartPosition;
                Entity targetEntity = p_pathRequest.EndPosition;
                MinHeapPathNode startNode = new MinHeapPathNode
                {
                    Entity = startEntity,
                    gCost = 0,
                    hCost = calculator.CalculateHCost(GridPositions[startEntity].value, GridPositions[targetEntity].value),
                    Parent = Entity.Null,
                    Next = -1,
                };
                startNode.fCost = calculator.CalculateFCost(startNode.gCost, startNode.hCost);
                startNode.Priority = startNode.fCost;

                NativeMinHeapForPathNodes openSet = new NativeMinHeapForPathNodes(10000, Allocator.Temp);
                NativeList<Entity> closedSet = new NativeList<Entity>(Allocator.Temp);
                NativeHashMap<Entity, MinHeapPathNode> nodes = new NativeHashMap<Entity, MinHeapPathNode>(10000, Allocator.Temp);
                nodes.TryAdd(startNode.Entity, startNode);
                openSet.Push(startNode);
                bool pathSuccess = false;
                while (openSet.HasNext())
                {
                    int currentIndex = openSet.Pop();
                    var currentNode = openSet[currentIndex];
                    if (currentNode.Entity == targetEntity)
                    {
                        pathSuccess = true;
                        break;
                    }
                    DynamicBuffer<Neighbours> neighbours = Neighbours[currentNode.Entity];
                    for (int i = 0; i < neighbours.Length; i++)
                    {
                        if (neighbours[i].entity == Entity.Null || closedSet.Contains(neighbours[i].entity))
                        {
                            continue;
                        }
                        MinHeapPathNode node;
                        int newMovementCostToNeighbour = currentNode.gCost + calculator.CalculateHCost(GridPositions[currentNode.Entity].value, GridPositions[neighbours[i].entity].value)+ NodeCosts[neighbours[i].entity].value;
                        if (nodes.TryGetValue(neighbours[i].entity, out node))
                        {
                            if (newMovementCostToNeighbour < node.gCost)
                            {
                                node.gCost = newMovementCostToNeighbour;
                                node.hCost = calculator.CalculateHCost(GridPositions[neighbours[i].entity].value, GridPositions[targetEntity].value);
                                node.fCost = calculator.CalculateFCost(node.gCost, node.hCost);
                                node.Parent = currentNode.Entity;
                                nodes.Remove(node.Entity);
                                nodes.TryAdd(node.Entity, node);
                                openSet.Push(node);
                            }
                        }
                        else
                        {
                            node = new MinHeapPathNode
                            {
                                Entity = neighbours[i].entity,
                                gCost = newMovementCostToNeighbour,
                                hCost = calculator.CalculateHCost(GridPositions[neighbours[i].entity].value, GridPositions[targetEntity].value),
                                Parent = currentNode.Entity,
                                Next = -1,
                            };
                            node.fCost = calculator.CalculateFCost(node.gCost, node.hCost);
                            node.Priority = node.fCost;
                            nodes.TryAdd(node.Entity, node);
                            openSet.Push(node);
                        }                       
                    }
                }
                if (pathSuccess)
                {
                    DynamicBuffer<Waypoints> waypointsbuffer = waypointsBuffers[p_entity];
                    waypointsbuffer.Clear();
                    RetracePath(p_pathRequest, nodes, waypointsbuffer);
                    p_pathInfo.waypoint = waypointsbuffer.Length;
                }
                else
                {
                    p_pathInfo.waypoint = 0;
                }
                p_pathRequest.Processed = true;
            }

            private void RetracePath(PathRequest p_pathRequest, NativeHashMap<Entity, MinHeapPathNode> p_nodes, DynamicBuffer<Waypoints> p_waypointsbuffer)
            {
                MinHeapPathNode currentNode;
                p_nodes.TryGetValue(p_pathRequest.EndPosition, out currentNode);
                while (currentNode.Entity != p_pathRequest.StartPosition)
                {
                    p_waypointsbuffer.Add(new Waypoints() { wayPoint = currentNode.Entity });
                    p_nodes.TryGetValue(currentNode.Parent, out currentNode);
                }
            }
            /*void SimplifyPath(List<PathNode> path, DynamicBuffer<Waypoints> p_buffer)
            {
                List<float3> waypoints = new List<float3>();
                int2 directionOld = int2.zero;

                for (int i = 1; i < path.Count; i++)
                {
                    int2 directionNew = new int2(path[i - 1].x - path[i].x, path[i - 1].y - path[i].y);
                    if (directionNew.x != directionOld.x && directionNew.y != directionOld.y)
                    {
                        p_buffer.Add(new Waypoints() { Position = new int2(path[i].x, path[i].y) });
                    }
                    directionOld = directionNew;
                }
            }*/
        }
    }
}
