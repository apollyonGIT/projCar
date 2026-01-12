using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Add_Pool_Relic : MonoBehaviour, IEncounter_Dialog_CPN
    {
        Vector2 m_velocity_min = new(0, 0);
        Vector2 m_velocity_max = new(3, 3);

        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            CPN_Utility.get_replace_strs_quick(args, out var replace_strs);

            var pool_relic_id = CPN_Utility.get_cache_str_value(args[1]);

            var relic_num_min = int.Parse(CPN_Utility.get_cache_str_value(args[2]));
            var relic_num_max = int.Parse(CPN_Utility.get_cache_str_value(args[3]));
            var relic_num = Random.Range(relic_num_min, relic_num_max + 1);

            var _event = Encounter_Dialog.instance._event;
            var pos = _event.pos + new Vector2(0, 2);

            if (!CPN_Utility.try_get_relic_id(pool_relic_id, out var relic_id)) return;

            Add_Relic.add_relic(owner, replace_strs, m_key_name, relic_id, relic_num);
        }
    }
}

