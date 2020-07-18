using Unity.Entities;
using Unity.Mathematics;

namespace dev.jonasjohansson.PathFinding
{
    public struct NodeCost : IComponentData
    {
        public int value;
    }
}