using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.Mover
{
    public class EnemyMover_Fly1
    {
        public static void @do(Enemy cell)
        {
            ref var position = ref cell.pos;

            #region PRM
            ref var velocity = ref cell.velocity;
            var distance = Vector2.Distance(cell.pos, cell.position_expt);
            var direction = (cell.position_expt - cell.pos).normalized;
            var mass_attachment = cell.mass_attachment;
            var acc_attacher = cell.acc_attacher;
            #endregion

            #region Outter
            var _desc = cell._desc;
            var base_speed = cell.base_speed;
            var max_force = _desc.max_force;
            var mass = _desc.mass;

            var breaking_rate = Config.current.breaking_rate;
            var breaking_force = Config.current.breaking_force;

            var is_follow = _desc.is_follow;
            #endregion

            if (base_speed == 0) return;

            //4. 计算推力加速度
            var start_breaking_dis = velocity.sqrMagnitude / (4f * max_force * breaking_rate / mass);

            var car_speed = WorldContext.instance.caravan_velocity.x;
            var speed_expt = base_speed * Mathf.Clamp01(distance / start_breaking_dis);
            var velocity_expt = direction * speed_expt + (is_follow ? 0 : 1) * car_speed * Vector2.right;

            var diff_expt = velocity_expt - velocity;
            var diff_expt_dir = diff_expt.normalized;
            var diff_expt_mag = diff_expt.magnitude;

            var dot = Vector2.Dot(diff_expt ,velocity);
            dot = dot > 0 ? 0 : -dot;

            var force = Mathf.Clamp01(diff_expt_mag / speed_expt) * Mathf.Lerp(1, breaking_force, dot) * max_force * diff_expt_dir;

            var acceleration = force / cell.mass_total;

            if (!is_follow)
                acceleration.y += Config.current.gravity * mass_attachment;

            acceleration += acc_attacher;

            //5. 计算并更新自身速度
            velocity += acceleration * Config.PHYSICS_TICK_DELTA_TIME;

            //6. 计算并更新自身位置
            position += (velocity + (is_follow ? 1: 0) * car_speed * Vector2.right) * Config.PHYSICS_TICK_DELTA_TIME;
            position.y = Mathf.Max(position.y, Road_Info_Helper.try_get_altitude(position.x));
        }
    }
}

