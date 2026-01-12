using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class EXMoveAuthoring : MonoBehaviour
{
    public float value;

    //==================================================================================================

    class Baker : Baker<EXMoveAuthoring>
    {
        public override void Bake(EXMoveAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EXMove { value = authoring.value });
        }
    }
}


public struct EXMove : IComponentData
{
    public float value;
}
