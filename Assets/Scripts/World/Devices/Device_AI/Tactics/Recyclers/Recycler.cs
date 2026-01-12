using UnityEngine;
using World.Helpers;

namespace World.Devices
{
    public class Recycler : Summon_Owner
    {
        enum Device_Recycler_FSM
        {
            idle,
            attack,
            broken,
        }

        Device_Recycler_FSM m_fsm;

        //==================================================================================================

        public override bool try_get_summon_model(out Summon_Mono summon_model)
        {
            return Addrs.Addressable_Utility.try_load_asset("summon_Recycler", out summon_model);
        }


        public void add_money(Vector2 pos)
        {
            //Drop_Loot_Helper.drop_loot(6000u, pos, Vector2.one);
        }
    }
}

