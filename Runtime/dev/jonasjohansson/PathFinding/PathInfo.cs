using Unity.Entities;
namespace dev.jonasjohansson.PathFinding
{
    public struct PathInfo : IComponentData
    {
        public int Waypoint;
        public bool EndReached;
    }
}
