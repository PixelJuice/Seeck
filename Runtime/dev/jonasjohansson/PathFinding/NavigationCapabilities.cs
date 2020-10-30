using System;
using Unity.Entities;

namespace dev.jonasjohansson.PathFinding
{
[Serializable]
    public struct NavigationCapabilities : IComponentData, INavigationCapabilities
    {
        public float MaxSlopeAngle;
        public float MaxClimbHeight;
        public float MaxDropHeight;
    }

    public interface INavigationCapabilities
    {
    }
}