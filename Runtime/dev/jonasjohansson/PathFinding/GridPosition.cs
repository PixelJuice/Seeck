using Unity.Entities;
using Unity.Mathematics;

namespace dev.jonasjohansson.PathFinding
{
    public struct GridPosition : IComponentData
    {
        public int2 value;
    }
}