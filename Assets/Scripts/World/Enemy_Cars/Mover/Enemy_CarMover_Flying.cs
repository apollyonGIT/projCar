using Commons;
using World.Helpers;

namespace World.Enemy_Cars.Mover
{
    public class Enemy_CarMover_Flying
    {
        public static void @do(Enemy_Car cell)
        {
            var ctx = cell.ctx;
            var mover = cell.car_mover;

            ref var pos = ref ctx.caravan_pos;
            ref var velocity = ref ctx.caravan_velocity;
            ref var acc = ref ctx.caravan_acc;
            ref var vx_stored = ref ctx.caravan_vx_stored;

            //1.计算并更新大篷车加速度 acc
            acc.x = velocity.x * ctx.feedback_1 / ctx.total_weight;
            acc.y = Config.current.gravity;

            //2.计算并更新大篷车速度 velocity
            velocity += acc * Config.PHYSICS_TICK_DELTA_TIME;

            if (vx_stored != 0)
            {
                velocity.x += vx_stored;
                vx_stored = 0;
            }

            //3.计算并更新大篷车位置 pos
            pos += velocity * Config.PHYSICS_TICK_DELTA_TIME;

            //4.判断大篷车是否着陆，并以此判断是否需要进一步调整位置
            if (pos.y <= Road_Info_Helper.try_get_altitude(pos.x))
            {
                mover.do_land(cell);
            }
        }
    }
}

