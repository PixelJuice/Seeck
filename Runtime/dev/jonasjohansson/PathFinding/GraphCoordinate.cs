using Unity.Entities;
using Unity.Mathematics;

namespace dev.jonasjohansson.PathFinding
{
    public struct GraphCoordinate2D : IComponentData {
        public int2 Value;
    }
}