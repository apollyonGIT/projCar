using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.Mover
{
    public class EnemyMover_Slide
    {
        public static void @do(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            ref var position = ref cell.pos;
            ref var velocity = ref cell.velocity;

            //1.计算并更新自身加速度的数值大小
            Road_Info_Helper.try_get_leap_rad(cell.pos.x, out var ground_rad);
            var sin_ground_rad = Mathf.Sin(ground_rad);
            var cos_ground_rad = Mathf.Cos(ground_rad);

            var acc_temp_gravity = Config.current.gravity * sin_ground_rad;
            var acc_temp_grip = Config.current.monster_grip * cos_ground_rad;

            if (velocity.x == 0)
                acc_temp_grip = Mathf.Abs(acc_temp_gravity) <= acc_temp_grip ? -acc_temp_gravity : -acc_temp_grip;
            else if (velocity.x > 0)
                acc_temp_grip *= -1;

            var acc_temp = acc_temp_gravity + acc_temp_grip;
            acc_temp += Vector2.Dot(cell.acc_attacher, new(cos_ground_rad, sin_ground_rad));

            //2.结合地形方向，计算并更新自身加速度
            Vector2 acceleration = new(acc_temp * cos_ground_rad, acc_temp * sin_ground_rad);
            if (cell.acc_attacher.y > -acceleration.y)
            {
                cell.mover.move_type = EN_enemy_move_type.Hover;
            }

            //3.计算并更新自身速度
            var dvx = acceleration.x * Config.PHYSICS_TICK_DELTA_TIME;
            var abs_dvx = Mathf.Abs(dvx);

            if (velocity.x != 0 && acc_temp_gravity < acc_temp_grip && dvx != 0)
            {
                dvx *= Mathf.Min(Mathf.Abs(velocity.x), abs_dvx) / abs_dvx;
            }

            velocity.x += dvx;

            //4.计算并更新自身位置
            position.x += velocity.x * Config.PHYSICS_TICK_DELTA_TIME;
            position.y = Road_Info_Helper.try_get_altitude(position.x);
        }
    }
}

