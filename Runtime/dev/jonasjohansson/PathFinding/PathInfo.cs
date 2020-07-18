using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace dev.jonasjohansson.PathFinding
{
    public struct PathInfo : IComponentData
    {
        public int waypoint;
    }
}
