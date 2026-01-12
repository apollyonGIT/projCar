using Commons;
using Foundations;
using System.Linq;
using UnityEngine;
using World.BackPack;
using World.Loots;

namespace World.Encounters.Dialogs
{
    public class Remove_Loot : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            CPN_Utility.get_replace_strs_quick(args, out var replace_strs);

            var loot_id = uint.Parse(CPN_Utility.get_cache_str_value(args[1]));

            var loot_num_min = int.Parse(CPN_Utility.get_cache_str_value(args[2]));
            var loot_num_max = int.Parse(CPN_Utility.get_cache_str_value(args[3]));
            var loot_num = Random.Range(loot_num_min, loot_num_max + 1);

            remove_loot(owner, replace_strs, m_key_name, loot_id, loot_num);
        }


        public static void remove_loot(IEncounter_Dialog_Window_UI owner, string[] replace_strs, string key_name, uint loot_id, int loot_num)
        {
            var ed = Encounter_Dialog.instance;
            if (ed.cache_dic.TryGetValue(key_name, out var current_loot_id))
            {
                loot_id = (uint)current_loot_id;
            }
            else
            {
                EX_Utility.dic_cover_add(ref ed.cache_dic, key_name, loot_id);
            }

            CPN_Utility.try_get_loot_name($"{loot_id}", out var loot_name);
            string[] new_strs = new[] { loot_name, $"{loot_num}" };

            Mission.instance.try_get_mgr("BackPack", out BackPackMgr mgr);

            var is_interactable = valid_can_remove(mgr, loot_id, loot_num);
            System.Action ac = btn_on_click;

            owner.change(replace_strs, new_strs, is_interactable, ac);


            #region 子函数 btn_on_click
            void btn_on_click()
            {
                for (int i = 0; i < loot_num; i++)
                {
                    mgr.RemoveLoot(get_remove_loot(mgr, loot_id));
                }

                Debug.Log($"失去掉落物{loot_name}，数量{loot_num}");
            }
            #endregion
        }


        public static bool valid_can_remove(BackPackMgr mgr, uint remove_loot_id, int count)
        {
            var loot_in_slots = mgr.slots.Where(t => t.loot != null && t.loot.desc.id == remove_loot_id);
            var loot_in_ow_slots = mgr.ow_slots.Where(t => t.loot != null && t.loot.desc.id == remove_loot_id);

            return count <= (loot_in_slots.Count() + loot_in_ow_slots.Count());
        }


        static Loot get_remove_loot(BackPackMgr mgr, uint remove_loot_id)
        {
            var e1 = mgr.ow_slots.Where(t => t.loot != null && t.loot.desc.id == remove_loot_id);
            if (e1.Count() > 0)
            {
                return e1.Last().loot;
            }

            return mgr.slots.Where(t => t.loot != null && t.loot.desc.id == remove_loot_id).Last().loot;
        }
    }
}

