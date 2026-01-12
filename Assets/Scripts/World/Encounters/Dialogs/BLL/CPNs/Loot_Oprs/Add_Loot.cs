using Foundations;
using UnityEngine;
using World.Helpers;

namespace World.Encounters.Dialogs
{
    public class Add_Loot : MonoBehaviour, IEncounter_Dialog_CPN
    {
        Vector2 m_velocity_min = new(0, 0);
        Vector2 m_velocity_max = new(3, 3);

        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            CPN_Utility.get_replace_strs_quick(args, out var replace_strs);

            //var loot_id = uint.Parse(CPN_Utility.get_cache_str_value(args[1]));

            //var loot_num_min = int.Parse(CPN_Utility.get_cache_str_value(args[2]));
            //var loot_num_max = int.Parse(CPN_Utility.get_cache_str_value(args[3]));
            //var loot_num = Random.Range(loot_num_min, loot_num_max + 1);

            var uname = m_key_name.Split('&')[0];
            AutoCodes.encounter_dialogs.TryGetValue(uname, out var r_encounter_dialog);
            var diy_prms = r_encounter_dialog.diy_prms;

            var loot_id = uint.Parse(diy_prms["loot_id"]);

            var loot_num_min = int.Parse(diy_prms["loot_num_min"]);
            var loot_num_max = int.Parse(diy_prms["loot_num_max"]);
            var loot_num = Random.Range(loot_num_min, loot_num_max + 1);

            var _event = Encounter_Dialog.instance._event;
            var pos = _event.pos + new Vector2(0, 2);

            add_loot(owner, replace_strs, m_key_name, loot_id, loot_num, pos, (m_velocity_min, m_velocity_max));
        }


        public static void add_loot(IEncounter_Dialog_Window_UI owner, string[] replace_strs, string key_name, uint loot_id, int loot_num, Vector2 pos, (Vector2 min, Vector2 max) velocity_area)
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

            bool? is_interactable = null;
            System.Action ac = btn_on_click;

            owner.change(replace_strs, new_strs, is_interactable, ac);


            #region 子函数 btn_on_click
            void btn_on_click()
            {
                Delay_Drop_Loot.delay(
                        loot_name,
                        do_drop_loot
                        );
            }
            #endregion


            #region 子函数 do_drop_loot
            void do_drop_loot()
            {
                for (int i = 0; i < loot_num; i++)
                {
                    //Vector2 velocity = new(Random.Range(velocity_area.min.x, velocity_area.max.x + 1), Random.Range(velocity_area.min.y, velocity_area.max.y + 1));
                    //Drop_Loot_Helper.drop_loot(loot_id, pos, velocity);

                    Drop_Loot_Helper.drop_loot(loot_id);
                }

                Debug.Log($"获得掉落物{loot_name}，数量{loot_num}");
            }
            #endregion
        }



    }
}

