using UnityEngine;

namespace World.Devices.Device_AI
{
    public class DScript_Melee_Charge_CircularSaw : DScript_Melee_Charge
    {
        public override void idle_state_tick()
        {
            base.idle_state_tick();

            if(charge_process == 0)
                FSM_change_to(Device_FSM_Charge.charge);
        }

        public override void attack_state_tick()
        {
            base.attack_state_tick();

            var current_dir = bones_direction[BONE_FOR_ROTATE];
            bones_direction[BONE_FOR_ROTATE] = Quaternion.AngleAxis(30, Vector3.forward) * current_dir;
        }
    }
}
