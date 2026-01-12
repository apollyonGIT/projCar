using AutoCodes;
using Commons;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Devices
{
    public class Magnet : Summon_Owner
    {
        enum Device_Magnet_FSM
        {
            idle,
            attack,
            broken,
        }

        Device_Magnet_FSM m_fsm;

        //==================================================================================================

        public override bool try_get_summon_model(out Summon_Mono summon_model)
        {
            return Addrs.Addressable_Utility.try_load_asset("summon_Magnet", out summon_model);
        }

    }
}

