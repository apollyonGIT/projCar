using Commons;
using Foundations;
using System.Reflection;
using UnityEngine;
using World.Helpers;

namespace World.Caravans
{
    public enum EN_caravan_move_type
    {
        None,
        Run,
        Jump,
        Flying,
        Land
    }


    public class CaravanMover
    {
        public static EN_caravan_move_type move_type => m_move_type;
        static EN_caravan_move_type m_move_type;

        static Assembly m_assembly = Assembly.Load("World");
        public static CaravanMgr mgr;

        //==================================================================================================

        public static void init()
        {
            var ctx = WorldContext.instance;

            Mission.instance.try_get_mgr("CaravanMgr", out mgr);
            m_move_type = ctx.init_caravan_move_type;

            ref var pos = ref ctx.caravan_pos;
            var ground_y = Road_Info_Helper.try_get_altitude(pos.x);
            if (pos.y < ground_y)
                m_move_type = EN_caravan_move_type.Run;

            if (m_move_type == EN_caravan_move_type.Run)
            {
                pos.y = ground_y;
                ctx.caravan_status_liftoff = WorldEnum.EN_caravan_status_liftoff.ground;
            }
            
        }


        public static void move()
        {
            m_assembly.GetType($"World.Caravans.Mover.CaravanMover_{m_move_type}").GetMethod("do")?.Invoke(null, null);
        }


        public static void do_run()
        {
            m_move_type = EN_caravan_move_type.Run;
        }


        public static void do_jump_input_vy(float input_vy)
        {
            var ctx = mgr.ctx;
            ctx.caravan_velocity.y = input_vy;

            do_jump();
        }


        public static void do_jump_input_h(float input_h)
        {
            var ctx = mgr.ctx;
            ctx.caravan_velocity.y = Mathf.Sqrt(2 * input_h * -Config.current.gravity);

            do_jump();
        }


        static void do_jump()
        {
            var ctx = mgr.ctx;
            ctx.caravan_status_liftoff = WorldEnum.EN_caravan_status_liftoff.sky;

            m_move_type = EN_caravan_move_type.Flying;
            ctx.is_ban_land = true;
            move(); //规则：执行1帧，以使小车真正飞行一段距离
            ctx.is_ban_land = false;

            m_move_type = EN_caravan_move_type.Jump;
        }


        public static void do_land()
        {
            m_move_type = EN_caravan_move_type.Land;

            var ctx = mgr.ctx;
            ctx.caravan_status_liftoff = WorldEnum.EN_caravan_status_liftoff.ground;

            BattleContext.instance.land_event?.Invoke();
        }


        /// <summary>
        /// 计算并设置篷车倾斜弧度
        /// </summary>
        public static void calc_and_set_caravan_leap_rad()
        {
            var ctx = mgr.ctx;

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

