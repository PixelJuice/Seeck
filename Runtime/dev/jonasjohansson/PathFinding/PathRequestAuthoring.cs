using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace dev.jonasjohansson.PathFinding
{
    public class PathRequestAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<PathRequest>(entity);
        }
    }
}
