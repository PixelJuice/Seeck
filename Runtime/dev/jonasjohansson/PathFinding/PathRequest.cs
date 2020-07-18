using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
namespace dev.jonasjohansson.PathFinding
{
    public struct PathRequest : IComponentData
    {
        public bool Processed;
        public Entity StartPosition;
        public Entity EndPosition;
    }
}
