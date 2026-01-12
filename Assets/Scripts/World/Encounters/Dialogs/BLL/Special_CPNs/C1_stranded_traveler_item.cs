using Foundations;
using UnityEngine;
using World.BackPack;

namespace World.Encounters.Dialogs
{
    public class C1_stranded_traveler_item : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        const string key_flag = "C1_stranded_traveler_item_flag";
        const string key_next_node_uname = "@C1?";

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            var cache_dic = Encounter_Dialog.instance.cache_dic;
            if (!cache_dic.ContainsKey(key_flag))
            {
                var add_pool_loot_id = args[1];
                var add_loot_num_min = int.Parse(args[2]);
                var add_loot_num_max = int.Parse(args[3]);

                var remove_pool_loot_id = args[4];
                var remove_loot_num_min = int.Parse(args[5]);
                var remove_loot_num_max = int.Parse(args[6]);

                
                CPN_Utility.try_get_loot_id(add_pool_loot_id, out var add_loot_id);
                var add_loot_num = Random.Range(add_loot_num_min, add_loot_num_max + 1);
                CPN_Utility.try_get_loot_name($"{add_loot_id}", out var add_loot_name);

                CPN_Utility.try_get_loot_id(remove_pool_loot_id, out var remove_loot_id);
                var remove_loot_num = Random.Range(remove_loot_num_min, remove_loot_num_max + 1);
                CPN_Utility.try_get_loot_name($"{remove_loot_id}", out var remove_loot_name);

                cache_dic.Add($"@C1_add_loot_id", $"{add_loot_id}");
                cache_dic.Add($"@C1_add_loot_name", add_loot_name);
                cache_dic.Add($"@C1_add_loot_num", $"{add_loot_num}");

                cache_dic.Add($"@C1_remove_loot_id", $"{remove_loot_id}");
                cache_dic.Add($"@C1_remove_loot_name", remove_loot_name);
                cache_dic.Add($"@C1_remove_loot_num", $"{remove_loot_num}");

                string C1_stranded_traveler_item_flag = "data_end";
                cache_dic.Add(key_flag, C1_stranded_traveler_item_flag);
            }

            //判断跳转
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr mgr);

            cache_dic.TryGetValue("@C1_remove_loot_id", out var _remove_loot_id);
            cache_dic.TryGetValue("@C1_remove_loot_num", out var _remove_loot_num);

            var next_node_uname = Remove_Loot.valid_can_remove(mgr, uint.Parse((string)_remove_loot_id), int.Parse((string)_remove_loot_num)) ? "C1" : "!C1";
            Share_DS.instance.add(key_next_node_uname, next_node_uname);
        }
    }
}

