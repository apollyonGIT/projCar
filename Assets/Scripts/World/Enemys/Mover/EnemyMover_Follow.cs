using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.Mover
{
    public class EnemyMover_Follow
    {
        public static void @do(Enemy cell)
        {
            #region Outter
            var speed_expt = cell.speed_expt;
            var position_expt = cell.position_expt;
            ref var position = ref cell.pos;
            ref var velocity = ref cell.velocity;

            ref var velocity_spectre = ref cell.velocity_spectre;

            var k_feedback = cell._desc.feedback;
            var acc_active = k_feedback * Mathf.Pow(speed_expt, 2);
            #endregion

            //2. 计算并更新自身加速度
            var dir_fly = (position_expt - position).normalized;
            var acceleration = dir_fly * acc_active;

            var vm = velocity_spectre.magnitude;
            var vn = velocity_spectre.normalized;

            acceleration -= k_feedback * Mathf.Pow(vm, 2) * vn;
            acceleration += cell.acc_attacher;

            //3. 计算并更新自身速度
            velocity_spectre += acceleration * Config.PHYSICS_TICK_DELTA_TIME;

            velocity = velocity_spectre;
            velocity.x += WorldContext.instance.caravan_velocity.x;

            //4. 计算并更新自身位置
            position += velocity * Config.PHYSICS_TICK_DELTA_TIME;
            position.y = Mathf.Max(position.y, Road_Info_Helper.try_get_altitude(position.x));
        }
    }
}

