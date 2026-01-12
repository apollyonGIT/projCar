using AutoCodes;
using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Devices.Device_AI
{
    public class DScript_Melee_Charge_Rocket :DScript_Melee_Charge
    {
        public float acc;
        public float max_speedX;


        private float slot2angle(string slot_name)
        {
            switch (slot_name)
            {
                case "slot_front":
                    return 180f;
                case "slot_front_top":
                    return -150f;
                case "slot_top":
                    return -90f;
                case "slot_back_top":
                    return -30f;
                case "slot_back":
                    return 0f;
            }

            return 0;
        }

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            acc = other_logic_data.diy_prms.TryGetValue("acc", out var acc_str) ? float.Parse(acc_str):1f;
            max_speedX = other_logic_data.diy_prms.TryGetValue("max_speedX", out var max_str) ? float.Parse(max_str) : 1f;
        }

        public override void idle_state_tick()
        {
            base.idle_state_tick();

            if(charge_process == 0)
                FSM_change_to(Device_FSM_Charge.charge);
        }

        public override void attack_state_tick()
        {
            base.attack_state_tick();

            var slot_name = Device_Slot_Helper.GetSlot(this);
            var angle = slot2angle(slot_name);

            var x_acc = acc * Mathf.Cos(angle * Mathf.Deg2Rad);
            var y_acc = acc * Mathf.Sin(angle * Mathf.Deg2Rad);

            if(WorldContext.instance.caravan_velocity.x < max_speedX)
            {
                WorldContext.instance.caravan_velocity += new Vector2(x_acc, 0) * Config.PHYSICS_TICK_DELTA_TIME;
            }

            if(WorldContext.instance.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.sky)
                WorldContext.instance.caravan_velocity += new Vector2(0, y_acc) * Config.PHYSICS_TICK_DELTA_TIME;
        }
    }
}
