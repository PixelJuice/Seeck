using UnityEngine;
using Unity.Entities;
namespace dev.jonasjohansson.PathFinding
{
    public class NavigationCapabilitiesAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<NavigationCapabilities>(entity);
        }
    }
}