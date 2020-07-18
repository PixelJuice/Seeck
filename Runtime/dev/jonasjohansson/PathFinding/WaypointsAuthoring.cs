using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace dev.jonasjohansson.PathFinding
{
    public class WaypointsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<Waypoints>(entity);
        }
    }
}
