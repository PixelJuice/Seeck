using Unity.Entities;

namespace dev.jonasjohansson.PathFinding
{
    [InternalBufferCapacity(20)]
    public struct Neighbours: IBufferElementData
    {
        public Entity entity;
    }
}