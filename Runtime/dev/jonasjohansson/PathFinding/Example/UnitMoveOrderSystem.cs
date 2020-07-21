
using dev.jonasjohansson.PathFinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace dev.jonasjohansson
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(PathFindingSystem))]
    public class UnitMoveOrderSystem : JobComponentSystem
    {
        public NativeArray<Entity> grid;
        internal int2 size;
        EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        private int CalculateIndex(int p_x, int p_y, int p_gridWidth)
        {
            return p_x + p_y * p_gridWidth;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            Entity target = grid[UnityEngine.Random.Range(0, grid.Length - 1)];
            JobHandle job = new GiveOrderJob()
            {
                ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent(),
                Time = Time.deltaTime,
                TargetPos = target,
                grid = grid,
                size = size,
                Heights = GetComponentDataFromEntity<Height>(true),

            }.Schedule(this, inputDeps);
            m_EndSimulationEcbSystem.AddJobHandleForProducer(job);
            return job;
        }

        [ExcludeComponent(typeof(PathInfo))]
        struct GiveOrderJob : IJobForEachWithEntity<PathRequest, Translation>
        {
            [ReadOnly]
            public Entity TargetPos;
            [ReadOnly]
            public float Time;
            [ReadOnly]
            public NativeArray<Entity> grid;
            [ReadOnly]
            public int2 size;
            [ReadOnly]
            public ComponentDataFromEntity<Height> Heights;

            public EntityCommandBuffer.Concurrent ecb;
            public void Execute(Entity entity, int index, ref PathRequest c0, ref Translation c1)
            {
                var seed = math.max(1, (uint)(random(new float2((entity.Index + 10) * Time, (index + 10) * Time * 2))));
                var rnd = new Unity.Mathematics.Random(seed);
                c0.Processed = false;
                int startIndex = CalculateIndex((int)math.round(c1.Value.x), (int)math.round(c1.Value.z), size.x);
                c0.StartPosition = grid[startIndex];
                int rand = rnd.NextInt(0, grid.Length);
                while (Heights[grid[rand]].value > 0)
                {
                    rand = rnd.NextInt(0, grid.Length);
                }
                Entity target = grid[rand];
                c0.EndPosition = target;
                ecb.AddComponent(index, entity, new PathInfo { waypoint = -1 });
            }

            float random(float2 p)
            {
                float2 K1 = new float2(
                    23.14069263277926f, // e^pi (Gelfond's constant)
                    2.665144142690225f // 2^sqrt(2) (Gelfondâ€“Schneider constant)
                );
                return math.abs((float)math.frac(math.cos(math.dot(p, K1)) * 12345.6789)) * 1000000;
            }

            private int CalculateIndex(int p_x, int p_y, int p_gridWidth)
            {
                return p_x + p_y * p_gridWidth;
            }
        }
    }
}
