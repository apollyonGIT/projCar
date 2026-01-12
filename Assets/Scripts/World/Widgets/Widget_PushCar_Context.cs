using Foundations;
using Foundations.Tickers;
using UnityEngine;
using World.Devices.Device_AI;
using World.Helpers;

namespace World.Widgets
{
    public class Widget_PushCar_Context : Singleton<Widget_PushCar_Context>
    {
        public int cd = 60;
        public int current_cd = 0;

        private const float PUSH_VX_MAX = 0.8F * 0.5F;   //0.8F是cos^2 + cos的估测值

        //Module为逻辑
        public CaravanPushModule push_module;

        public void attach()
        {
            Ticker.instance.do_when_tick_start += tick;
            push_module = new CaravanPushModule();
        }

        public void detach()
        {
            Ticker.instance.do_when_tick_start -= tick;
        }

        private void tick()
        {
            if (current_cd > 0)
                current_cd--;
            push_module.tick();
        }

        public bool AbleToPush()
        {
            var check_move_type = WorldContext.instance.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.ground;
            var check_vx = WorldContext.instance.caravan_velocity.x < PUSH_VX_MAX;  //用v.x而不是v.magnitude是为了避免开根号以提高性能，代价是会有一些误差
            return check_move_type && check_vx;            
        }
        public bool AbleToPush_CheckCD => current_cd <= 0; //cd都没好推啥

        public void PushCaravan()
        {
            current_cd = cd;

            var car = WorldContext.instance;

            Road_Info_Helper.try_get_leap_rad(car.caravan_pos.x, out var ground_rad);
            var cos_ground_rad = Mathf.Cos(ground_rad);

            WorldContext.instance.caravan_vx_stored += Mathf.Clamp(cos_ground_rad + 0.1f - car.caravan_velocity.magnitude * 2f, 0f, 0.5f);
        }
    }
}
