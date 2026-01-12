using UnityEngine;

namespace World.Devices
{
    public class BlackHole : Summon_Owner
    {
        enum Device_BlackHole_FSM
        {
            idle,
            attack,
            broken,
        }

        Device_BlackHole_FSM m_fsm;

        //==================================================================================================

        public override bool try_get_summon_model(out Summon_Mono summon_model)
        {
            return Addrs.Addressable_Utility.try_load_asset("summon_BlackHole", out summon_model);
        }
    }
}

