using Unity.Entities;

namespace dev.jonasjohansson.PathFinding
{
    [InternalBufferCapacity(50)]
    public struct Edge : IBufferElementData, IEdge
    {
        public Entity Value;
        public float Cost;
    }

    public interface IEdge
    {
    }
}