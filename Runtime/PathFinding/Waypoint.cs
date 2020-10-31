using Unity.Entities;
using Unity.Mathematics;

namespace dev.jonasjohansson.PathFinding
{
    [InternalBufferCapacity(50)]
    public struct Waypoint : IBufferElementData
    {
        public Entity Value;
    }
}