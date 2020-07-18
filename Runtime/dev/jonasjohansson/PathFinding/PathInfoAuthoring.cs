using Unity.Entities;
using UnityEngine;
namespace dev.jonasjohansson.PathFinding
{
    public class PathInfoAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<PathInfo>(entity);
        }
    }
}
