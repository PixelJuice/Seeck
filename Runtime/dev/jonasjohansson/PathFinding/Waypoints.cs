using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace dev.jonasjohansson.PathFinding
{
    [InternalBufferCapacity(50)]
    public struct Waypoints : IBufferElementData
    {
        public Entity wayPoint;
    }
}
