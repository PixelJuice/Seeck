using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace dev.jonasjohansson.PathFinding
{
    public class AStarSystem : JobComponentSystem
    {
        EntityQuery m_requestQuery;
        ManhattanHeuristicCalculator m_calculator;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_requestQuery = GetEntityQuery(typeof(Waypoint), ComponentType.ReadOnly<PathRequest>(), ComponentType.ReadOnly<NavigationCapabilities>());
            m_requestQuery.AddChangedVersionFilter(typeof(PathRequest));
            m_calculator = new ManhattanHeuristicCalculator();
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new Search()
            {
                GraphSize = 10000,
                AverageEdgeCount = 8,
                PathRequestsChunkComponent = GetArchetypeChunkComponentType<PathRequest>(false),
                NavigationCapabilitiesChunkComponent = GetArchetypeChunkComponentType<NavigationCapabilities>(true),
                WaypointChunkBuffer = GetArchetypeChunkBufferType<Waypoint>(false),
                GraphCoordinates = GetComponentDataFromEntity<GraphCoordinate2D>(true),
                Edges = GetBufferFromEntity<Edge>(true),
                Iterations = 1000,
                Calculator = m_calculator
            }.Schedule(m_requestQuery, inputDeps);  
        }
        [BurstCompile]
        struct Search: IJobChunk
        {
            public ArchetypeChunkComponentType<PathRequest> PathRequestsChunkComponent;
            [WriteOnly] public ArchetypeChunkBufferType<Waypoint> WaypointChunkBuffer;
            [ReadOnly] public ArchetypeChunkComponentType<NavigationCapabilities> NavigationCapabilitiesChunkComponent;
            [ReadOnly] public BufferFromEntity<Edge> Edges;
            [ReadOnly]
            public ComponentDataFromEntity<GraphCoordinate2D> GraphCoordinates;
            [ReadOnly] public int GraphSize;
            [ReadOnly] public int AverageEdgeCount;
            [ReadOnly] public int Iterations;
            [ReadOnly] public ManhattanHeuristicCalculator Calculator;
            public void Execute(ArchetypeChunk chunk, int chunkIndex,int firstEntityIndex)
            {
                NativeArray<PathRequest> PathRequests = chunk.GetNativeArray(PathRequestsChunkComponent);
                BufferAccessor<Waypoint> Waypoints = chunk.GetBufferAccessor(WaypointChunkBuffer);
                NativeArray<NavigationCapabilities> NavigationCapabilities = chunk.GetNativeArray(NavigationCapabilitiesChunkComponent);

                for (int i = 0; i < chunk.Count; i++)
                {
                    PathRequest request = PathRequests[i];
                    if (request.Processed)
                    {
                        continue;
                    }
                    DynamicBuffer<Waypoint> waypoints = Waypoints[i];
                    if (request.IsNew)
                    {
                        waypoints.Clear();
                    }
                    request.IsNew = false;
                    if (request.StartNode == request.EndNode)
                    {
                        request.Processed = true;
                        waypoints.Add(new Waypoint { Value = request.StartNode});
                        continue;
                    }
                    NativeHashMap<Entity, int> costs = new NativeHashMap<Entity, int>((Iterations + 1) * AverageEdgeCount, Allocator.Temp);
                    NativeHashMap<Entity, Entity>  cameFrom = new NativeHashMap<Entity, Entity>((Iterations + 1) * AverageEdgeCount, Allocator.Temp);
                    NativeMinHeapForGraphNodes openSet = new NativeMinHeapForGraphNodes((Iterations + 1) * AverageEdgeCount, Allocator.Temp);

                    if(ProcessPath(in Calculator, ref request, ref cameFrom, ref openSet, ref costs, NavigationCapabilities[i])) {
                        RetracePath(ref waypoints, ref request, in cameFrom);
                    }
                    openSet.Dispose();
                    costs.Dispose();
                    cameFrom.Dispose();
                    request.Iteration++;
                    request.Processed = true;
                    PathRequests[i] = request;
                }
            }

            bool ProcessPath(in ManhattanHeuristicCalculator p_calculator, ref PathRequest p_request, ref NativeHashMap<Entity, Entity> p_cameFrom, ref NativeMinHeapForGraphNodes p_openSet, ref NativeHashMap<Entity, int> p_costs, in NavigationCapabilities p_navigationCapabilities) {
                int iterations = Iterations;
                int hCost = p_calculator.CalculateHCost(GraphCoordinates[p_request.StartNode].Value,GraphCoordinates[p_request.EndNode].Value);               
                MinHeapGraphNode head = new MinHeapGraphNode(p_request.StartNode, hCost, hCost, 0);
                p_costs.Add(head.Entity, 0);
                p_openSet.Push(head);
                MinHeapGraphNode closest = head;
                MinHeapGraphNode current;
                while (p_openSet.HasNext())
                {
                    current = p_openSet.Pop();
                    //is node closer to our goal?
                    if (current.hCost < closest.hCost)
                    {
                        closest = current;
                    }
                    //Goal reached
                    if (current.Entity == p_request.EndNode)
                    {
                        return true;
 
                    }
                    if (iterations == 0)
                    {
                        if (p_request.CloserIsEnough)
                        {
                            p_request.EndNode = closest.Entity;
                            return true;
                        }
                        return false;
                    }
                    iterations--;
                    DynamicBuffer<Edge> currentEdges = Edges[current.Entity];
                    for (int i = 0; i < currentEdges.Length; i++)
                    {
                        var edge = currentEdges[i];
                        float costToNode = Calculator.CalculateGCost(current.gCost, edge, p_navigationCapabilities);
                        
                        if (float.IsInfinity(edge.Cost))
                        {
                            continue;
                        }
                        int gCost = (int)costToNode;
                        p_costs.TryGetValue(edge.Value, out int oldCost);
                        if (!(oldCost <= 0) && !(gCost < oldCost))
                        {
                            continue;
                        }
                        p_costs[edge.Value] = (int)gCost;
                        p_cameFrom[edge.Value] = current.Entity;
                        hCost = Calculator.CalculateHCost(GraphCoordinates[edge.Value].Value, GraphCoordinates[p_request.EndNode].Value);
                        int fCost = gCost + hCost;
                        MinHeapGraphNode newNode = new MinHeapGraphNode(edge.Value, fCost, hCost, gCost);
                        p_openSet.Push(newNode);
                    }
                }
                if (p_request.CloserIsEnough)
                {
                    p_request.EndNode = closest.Entity;
                }
                return false;
            }
            private void RetracePath(ref DynamicBuffer<Waypoint> p_waypoints, ref PathRequest p_request, in NativeHashMap<Entity, Entity> p_cameFrom)
            {
                p_waypoints.Add(new Waypoint { Value = p_request.EndNode });
                var current = p_cameFrom[p_request.EndNode];
                while (current != p_request.StartNode)
                {
                    p_waypoints.Add(new Waypoint { Value = current });
                    current = p_cameFrom[current];
                }
                //p_waypoints.Reverse();
            }
        }   
    }
}