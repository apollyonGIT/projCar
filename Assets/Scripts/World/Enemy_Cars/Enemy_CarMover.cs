using Commons;
using System.Reflection;
using UnityEngine;
using World.Caravans;
using World.Helpers;

namespace World.Enemy_Cars
{
    public class Enemy_CarMover
    {
        public EN_caravan_move_type move_type;

        static Assembly m_assembly = Assembly.Load("World");

        //==================================================================================================

        public void init()
        {
            move_type = EN_caravan_move_type.Run;
        }


        public void move(Enemy_Car cell)
        {
            m_assembly.GetType($"World.Enemy_Cars.Mover.Enemy_CarMover_{move_type}").GetMethod("do")?.Invoke(null, new object[] { cell });
        }


        public void do_run()
        {
            move_type = EN_caravan_move_type.Run;
        }


        public void do_jump_input_vy(Enemy_Car cell, float input_vy)
        {
            var ctx = cell.ctx;
            ctx.caravan_velocity.y = input_vy;

            do_jump(cell);
        }


        public void do_jump_input_h(Enemy_Car cell, float input_h)
        {
            var ctx = cell.ctx;
            ctx.caravan_velocity.y = Mathf.Sqrt(2 * input_h * -Config.current.gravity);

            do_jump(cell);
        }


        void do_jump(Enemy_Car cell)
        {
            var ctx = cell.ctx;
            ctx.caravan_status_liftoff = WorldEnum.EN_caravan_status_liftoff.sky;

            move_type = EN_caravan_move_type.Flying;
            move(cell); //规则：执行1帧，以使小车真正飞行一段距离

            move_type = EN_caravan_move_type.Jump;
        }


        public void do_land(Enemy_Car cell)
        {
            move_type = EN_caravan_move_type.Land;

            var ctx = cell.ctx;
            ctx.caravan_status_liftoff = WorldEnum.EN_caravan_status_liftoff.ground;
        }


        /// <summary>
        /// 计算并设置篷车倾斜弧度
        /// </summary>
        public void calc_and_set_caravan_leap_rad(Enemy_Car cell)
        {
            var ctx = cell.ctx;

            float target_angle = 0;
            if (ctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.ground)
            {
                Road_Info_Helper.try_get_leap_rad(ctx.caravan_pos.x, out var rad);
                target_angle = Mathf.Clamp(rad * Mathf.Rad2Deg, -Config.current.caravan_rotate_angle_limit, Config.current.caravan_rotate_angle_limit);
            }

            ref var theta = ref ctx.caravan_rotation_theta;
            ref var omega = ref ctx.caravan_rotation_omega;
            ref var beta = ref ctx.caravan_rotation_beta;

            var delta_angle = theta - target_angle;

            beta = -delta_angle * Config.current.caravan_rotation_damp_1 - omega * (Config.current.caravan_rotation_damp_2);
            omega += beta / Config.PHYSICS_TICKS_PER_SECOND;
            theta += omega / Config.PHYSICS_TICKS_PER_SECOND;

            ctx.caravan_rad = theta * Mathf.Deg2Rad;
        }
    }
}

