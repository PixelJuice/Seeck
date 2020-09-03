using dev.jonasjohansson.PathFinding;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace dev.jonasjohansson
{
    public class Grid: MonoBehaviour
    {
        public GameObject squarePrefab;
        public int2 m_gridWorldSize;
        private NativeArray<Entity> m_squares;

        private void Start()
        {
            m_squares = new NativeArray<Entity>(m_gridWorldSize.x * m_gridWorldSize.y,Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var archType = entityManager.CreateArchetype(ComponentType.ReadWrite<GridIndex>(), ComponentType.ReadWrite<Neighbours>(), ComponentType.ReadWrite<GridPosition>(), ComponentType.ReadWrite<NodeCost>(), ComponentType.ReadWrite<Height>());

            for (int x = 0; x < m_gridWorldSize.x; x++)
            {
                for (int y = 0; y < m_gridWorldSize.y; y++)
                { 
                    Entity entity = entityManager.CreateEntity(archType);
                    int index = CalculateIndex(x, y, m_gridWorldSize.x);
                    entityManager.SetComponentData(entity, new GridIndex { value = index });
                    entityManager.SetComponentData(entity, new GridPosition { value = new int2(x,y) });
                    int cost = (int)(Mathf.PerlinNoise(((float)x / (float)m_gridWorldSize.x * 5f), ((float)y / (float)m_gridWorldSize.y * 5f))*10);
                    entityManager.SetComponentData(entity, new NodeCost { value = (cost / 5) * 1000 });
                    entityManager.SetComponentData(entity, new Height { value = cost /5 });
                    m_squares[index] = entity;
                    CreateSquare(x, y, cost/5);
                }
            }
            int2[] m_neighbourOffsetArray = new int2[8];
            m_neighbourOffsetArray[0] = new int2(-1, 0); //Left
            m_neighbourOffsetArray[1] = new int2(+1, 0); //Right
            m_neighbourOffsetArray[2] = new int2(0, +1); //Up
            m_neighbourOffsetArray[3] = new int2(0, -1); //Down
            m_neighbourOffsetArray[4] = new int2(-1, +1); //Left Up
            m_neighbourOffsetArray[5] = new int2(-1, -1); //Left Down
            m_neighbourOffsetArray[6] = new int2(+1, +1); //Right Up
            m_neighbourOffsetArray[7] = new int2(+1, -1); //Right Down
            for (int i = 0; i < m_squares.Length; i++)
            {
                Entity ent = m_squares[i];
                GridPosition currentPos = entityManager.GetComponentData<GridPosition>(ent);
                DynamicBuffer<Neighbours> neighboursbuffer = entityManager.GetBuffer<Neighbours>(ent);
                for (int n = 0; n < m_neighbourOffsetArray.Length; n++)
                {
                    int2 neighbourOffset = m_neighbourOffsetArray[n];
                    int2 neighbourPosition = new int2(currentPos.value.x + neighbourOffset.x, currentPos.value.y + neighbourOffset.y);
                    if (isPositionInGrid(neighbourPosition, m_gridWorldSize.x))
                    {
                        neighboursbuffer.Add(new Neighbours { entity = m_squares[CalculateIndex(neighbourPosition.x, neighbourPosition.y, m_gridWorldSize.x)] });
                    }
                }
            }
            
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitMoveOrderSystem>().grid = m_squares;
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<UnitMoveOrderSystem>().size = m_gridWorldSize;
        }

        private int CalculateIndex(int p_x, int p_y, int p_gridWidth)
        {
            return p_x + p_y * p_gridWidth;
        }

        private void CreateSquare(int p_x, int p_z, int p_y)
        {
            GameObject square = Instantiate(squarePrefab, this.transform);
            square.transform.position = new Vector3(p_x, p_y, p_z);
        }

        private bool isPositionInGrid(int2 p_position, int2 p_grid)
        {
            return
                p_position.x >= 0 &&
                p_position.y >= 0 &&
                p_position.x < p_grid.x &&
                p_position.y < p_grid.y;
        }

        private void OnDestroy()
        {
            m_squares.Dispose();
        }
    }
}
