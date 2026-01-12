using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.Mover
{
    public class EnemyMover_Fly
    {
        public static void @do(Enemy cell)
        {
            ref var position = ref cell.pos;
            ref var velocity = ref cell.velocity;

            //0.尝试获取必要的参数
            var k_feedback = cell._desc.feedback;
            var acc_active = k_feedback * Mathf.Pow(cell.speed_expt, 2);

            if (cell.mass_attachment != 0)
                acc_active *= cell.mass_self / cell.mass_total;

            //1.计算并更新自身加速度
            var dir_fly = (cell.position_expt - position).normalized;
            var acc = dir_fly * acc_active;

            var vm = velocity.magnitude;
            var vn = velocity.normalized;

            acc += k_feedback * Mathf.Pow(vm, 2) * -vn;

            acc.y += Config.current.gravity * cell.mass_attachment / cell.mass_total;
            acc += cell.acc_attacher;

            //2.计算并更新自身速度
            velocity += acc * Config.PHYSICS_TICK_DELTA_TIME;

            //3.计算并更新自身位置
            position += velocity * Config.PHYSICS_TICK_DELTA_TIME;
            position.y = Mathf.Max(position.y, Road_Info_Helper.try_get_altitude(position.x));
        }


        public static void unconsious(Enemy cell)
        {
            ref var position = ref cell.pos;
            ref var velocity = ref cell.velocity;

            //0.尝试获取必要的参数
            var k_feedback = cell._desc.feedback;

            //1.计算并更新自身加速度
            Vector2 acceleration = new();
            acceleration.y = Config.current.gravity;

            var vm = velocity.magnitude;
            var vn = velocity.normalized;

            acceleration.x = -k_feedback * velocity.x * Mathf.Abs(velocity.x);

            if (cell.acc_attacher != Vector2.zero)
            {
                acceleration.y += -k_feedback * velocity.y * Mathf.Abs(velocity.y);
                acceleration += cell.acc_attacher;
            }

            //2.计算并更新自身速度
            velocity += acceleration * Config.PHYSICS_TICK_DELTA_TIME;

            //3.计算并更新自身位置
            position += velocity * Config.PHYSICS_TICK_DELTA_TIME;

            //4.所有滞空的怪物终将着陆 - 计算是否着陆
            var altitude = Road_Info_Helper.try_get_altitude(position.x);

            if (position.y <= altitude)
            {
                position.y = altitude;
                velocity.y = 0;
                velocity.x = 0;
            }
        }
    }
}

