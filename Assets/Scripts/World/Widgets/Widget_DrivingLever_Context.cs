using Commons;
using Foundations;
using Foundations.Tickers;
using UnityEngine;
using World.Devices.Device_AI;

namespace World.Widgets
{
    public class Widget_DrivingLever_Context : Singleton<Widget_DrivingLever_Context>
    {
        public float target_lever { get; private set; }

        public CaravanModule driving_module;

        //==================================================================================================

        public void attach()
        {
            Ticker.instance.do_when_tick_start += tick;

            driving_module = new CaravanModule();
        }

        public void detach()
        {
            Ticker.instance.do_when_tick_start -= tick;
        }

        private void tick()
        {
            ref var driving_lever = ref WorldContext.instance.driving_lever;
            driving_module.tick();
        }

        //==================================================================================================

        public void SetLever(float value, bool also_set_target_lever)
        {
            value = Mathf.Clamp01(value);

            if (also_set_target_lever)
                target_lever = value;

            WorldContext.instance.driving_lever = value;
        }

        //==================================================================================================

        public void Drag_Lever_by_Blower(float value, bool also_set_target_lever)
        {
            // New Test 2025.06.09
            SetLever(value, also_set_target_lever);

            if (WorldContext.instance.driving_lever != 0)
                WorldContext.instance.caravan_status_acc = WorldEnum.EN_caravan_status_acc.driving;
        }

        public void Drag_Lever(bool can_drag, bool drag_to_up, bool also_set_target_lever)
        {

            // 2025.08.22: 一眼盯帧，鉴定为可删
            var base_lever = WorldContext.instance.driving_lever;
            if (can_drag)
            {
                var dir_speed = drag_to_up ? Config.current.lever_up_speed : Config.current.lever_down_speed;
                base_lever += (dir_speed - base_lever) * Config.current.lever_move_delta;

                SetLever(base_lever, also_set_target_lever);

                if (WorldContext.instance.driving_lever != 0)
                    WorldContext.instance.caravan_status_acc = WorldEnum.EN_caravan_status_acc.driving;
            }
        }
    }
}

