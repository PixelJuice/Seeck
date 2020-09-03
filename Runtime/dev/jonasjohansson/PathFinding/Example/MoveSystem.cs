using dev.jonasjohansson.PathFinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace dev.jonasjohansson
{
    //[DisableAutoCreation]
    public class MoveSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var delta = Time.DeltaTime;
            JobHandle job = new MoveJob()
            {
                ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent(),
                WaypointsBuffer = GetBufferFromEntity<Waypoints>(),
                DeltaTime = delta,
                GridPositions = GetComponentDataFromEntity<GridPosition>(true),
                Gridheights = GetComponentDataFromEntity<Height>(true),

            }.Schedule(this, inputDeps);

            m_EndSimulationEcbSystem.AddJobHandleForProducer(job);

            return job;
        }

        struct MoveJob : IJobForEachWithEntity<Translation, PathInfo>
        {
            [ReadOnly]
            public BufferFromEntity<Waypoints> WaypointsBuffer;
            [ReadOnly]
            public float DeltaTime;
            [ReadOnly]
            public ComponentDataFromEntity<GridPosition> GridPositions;
            [ReadOnly]
            public ComponentDataFromEntity<Height> Gridheights;

            public EntityCommandBuffer.Concurrent ecb;

            public void Execute(Entity entity, int index, ref Translation p_translation, ref PathInfo p_pathInfo)
            {
                if (p_pathInfo.waypoint > 0)
                {
                    DynamicBuffer<Waypoints> waypoints = WaypointsBuffer[entity];
                    Entity targetIndex = waypoints[p_pathInfo.waypoint - 1].wayPoint;
                    int2 pathPosition = GridPositions[targetIndex].value;
                    float height = Gridheights[targetIndex].value;

                    float3 targetPos = new float3(pathPosition.x, height + 1f, pathPosition.y);
                    float3 moveDir = math.normalizesafe(targetPos - p_translation.Value);
                    float speed = 3f;
                    p_translation.Value += moveDir * speed * DeltaTime;
                    if (math.distance(p_translation.Value, targetPos) < .1f)
                    {
                        p_pathInfo.waypoint--;

                    }

                }
                if (p_pathInfo.waypoint == 0)
                {
                    ecb.RemoveComponent<PathInfo>(index, entity);
                }
            }
        }
    }
}
