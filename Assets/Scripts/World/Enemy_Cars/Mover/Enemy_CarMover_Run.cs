using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemy_Cars.Mover
{
    public class Enemy_CarMover_Run
    {
        public static void @do(Enemy_Car cell)
        {
            var ctx = cell.ctx;
            var mover = cell.car_mover;

            ref var pos = ref ctx.caravan_pos;
            ref var velocity = ref ctx.caravan_velocity;
            ref var acc = ref ctx.caravan_acc;
            ref var vx_stored = ref ctx.caravan_vx_stored;

            //1.使小车加速度受地形坡度修正
            Road_Info_Helper.try_get_leap_rad(pos.x, out var ground_rad);
            var sin_ground_rad = Mathf.Sin(ground_rad);
            var cos_ground_rad = Mathf.Cos(ground_rad);

            var acc_temp = Config.current.gravity * sin_ground_rad;

            //2.计算小车因阻力产生的加速度
            acc_temp += Mathf.Sign(velocity.x) * velocity.magnitude * ctx.feedback_1 / ctx.total_weight + ctx.feedback_0;

            //3.判断大篷车是在加速还是在刹车，并因此给出行驶或制动加速度
            if (ctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.braking || velocity.x * ctx.driving_lever < 0)
            {
                acc_temp += Mathf.Sign(velocity.x) * Config.current.acc_braking * cos_ground_rad;
            }
            else
            {
                var tractive_force = ctx.tractive_force_max * ctx.driving_lever;

                acc_temp += tractive_force / ctx.total_weight;
            }

            //4.计算并更新大篷车加速度 acc
            acc = acc_temp * new Vector2(cos_ground_rad, sin_ground_rad);

            //5.计算并更新大篷车速度 velocity
            velocity += acc * Config.PHYSICS_TICK_DELTA_TIME;

            if (vx_stored != 0)
            {
                velocity.x += vx_stored;
                vx_stored = 0;
            }

            //6.调整速度方向至沿当前地面走向
            velocity = Mathf.Sign(velocity.x) * velocity.magnitude * new Vector2(cos_ground_rad, sin_ground_rad);

            //7.计算大篷车是否可以进行“斜坡飞跃”，并移动大篷车位置
            var bl_0 = Road_Info_Helper.try_get_concavity(pos.x, out var concavity) && concavity < 0;
            var bl_1 = Road_Info_Helper.try_get_ground_p(pos.x, out var ground_p) && velocity.sqrMagnitude / ground_p > -Config.current.gravity * cos_ground_rad;

            if (bl_0 && bl_1)
            {
                mover.do_jump_input_vy(cell, velocity.y);
                return;
            }

            //8.按“贴地移动”来计算并更新大篷车位置 position
            pos.x += velocity.x * Config.PHYSICS_TICK_DELTA_TIME;
            pos.y = Road_Info_Helper.try_get_altitude(pos.x);
        }
    }
}

