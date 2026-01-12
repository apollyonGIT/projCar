using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.Mover
{
    public class EnemyMover_Walk
    {
        public static void @enter(Enemy cell)
        {
            ref var velocity = ref cell.velocity;
            var position_expt = cell.position_expt;
            var speed_expt = cell.speed_expt;

            Road_Info_Helper.try_get_leap_rad(cell.pos.x, out var ground_rad);
            var sin_ground_rad = Mathf.Sin(ground_rad);
            var walk_dir = (position_expt - cell.pos).normalized * sin_ground_rad;

            //3. 进入状态时，立即更改当前速度
            velocity = walk_dir.normalized * speed_expt;
        }


        public static void @do(Enemy cell)
        {
            #region Outter
            var speed_expt = cell.speed_expt;
            var position_expt = cell.position_expt;
            var mass_attachment = cell.mass_attachment;
            var k_feedback = cell._desc.feedback;
            #endregion

            #region PRM
            ref var velocity = ref cell.velocity;
            ref var position = ref cell.pos;
            
            Road_Info_Helper.try_get_leap_rad(position.x, out var ground_rad);
            var cos_ground_rad = Mathf.Cos(ground_rad);
            var walk_dir = ((position_expt - position) * cos_ground_rad).normalized;

            var acc_attacher = cell.acc_attacher;
            #endregion

            //3. 每当speed_expt发生变化时，立即更改当前速度
            if (cell.is_speed_expt_change)
                velocity = walk_dir.normalized * speed_expt;

            //4. 每帧速度计算
            var speed_diff = speed_expt - velocity.magnitude;
            var speed_delta_change = speed_diff * (1 - k_feedback) * (1 - Mathf.Pow(k_feedback, 2) * mass_attachment / (mass_attachment + 1)) * Config.PHYSICS_TICK_DELTA_TIME;
            velocity = velocity.normalized * (velocity.magnitude + speed_delta_change);

            //5. 外力滞空判断
            if (acc_attacher.y > 0 && Mathf.Abs(acc_attacher.y) > Mathf.Abs(Config.current.gravity) * Config.current.knock_air_threshold_factor)
            {
                cell.mover.move_type = EN_enemy_move_type.Hover;
            }

            //6. 计算并更新自身位置
            position.x += velocity.x * Config.PHYSICS_TICK_DELTA_TIME;
            position.y = Road_Info_Helper.try_get_altitude(position.x);
        }
    }
}

