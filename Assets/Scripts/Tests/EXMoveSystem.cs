using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct EXMoveSystem : ISystem
{
    float2 target;
    int time;

    public void OnCreate(ref SystemState state)
    {
        target.x = UnityEngine.Random.Range(-10, 10);
        target.y = UnityEngine.Random.Range(0, 10);
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (localTransform, _EXMove) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EXMove>>())
        {
            //float power = 1f;

            //for (int i = 0; i < 1000000; i++)
            //{
            //    power *= 2f;
            //    power /= 2f;
            //}

            //localTransform.ValueRW = localTransform.ValueRW.RotateZ(-_EXMove.ValueRO.value * SystemAPI.Time.DeltaTime * power);
            time++;
            if (time > 100)
            {
                target.x = UnityEngine.Random.Range(-10,10);
                target.y = UnityEngine.Random.Range(0, 10);

                time = 0;
            }

            float2 current = new (localTransform.ValueRW.Position.x, localTransform.ValueRW.Position.y);
            float2 dir = target - current;
            dir = math.normalize(dir);

            float3 _dir = new(dir.x, dir.y, 0);

            localTransform.ValueRW.Position += _dir * 5 * SystemAPI.Time.DeltaTime; 

            //var dis = math.distance(localTransform.ValueRW.Position, float3.zero);
            //if (dis >= 10)
            //{
            //    is_refresh = true;
            //}
        }
    }
}


