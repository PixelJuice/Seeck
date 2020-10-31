using UnityEngine;
using Unity.Entities;
namespace dev.jonasjohansson.PathFinding
{
    public class WaypointAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<Waypoint>(entity);
        }
    }
}