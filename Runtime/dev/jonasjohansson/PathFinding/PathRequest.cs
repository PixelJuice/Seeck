using Unity.Entities;

namespace dev.jonasjohansson.PathFinding
{
    public struct PathRequest : IComponentData, IPathRequest {
        public bool Processed;
        public bool IsNew;
        public bool CloserIsEnough;
        public Entity StartNode;
        public Entity EndNode;
        public int Iteration;
    }

    public interface IPathRequest
    {
    }
}