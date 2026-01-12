using Commons;
using Foundations;
using UnityEngine;
using World.Helpers;

namespace World.Encounters.Dialogs
{
    public class Add_Relic : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            CPN_Utility.get_replace_strs_quick(args, out var replace_strs);

            var relic_id = uint.Parse(CPN_Utility.get_cache_str_value(args[1]));

            var relic_num_min = int.Parse(CPN_Utility.get_cache_str_value(args[2]));
            var relic_num_max = int.Parse(CPN_Utility.get_cache_str_value(args[3]));
            var relic_num = Random.Range(relic_num_min, relic_num_max + 1);

            add_relic(owner, replace_strs, m_key_name, relic_id, relic_num);
        }


        public static void add_relic(IEncounter_Dialog_Window_UI owner, string[] replace_strs, string key_name, uint relic_id, int relic_num)
        {
            var ed = Encounter_Dialog.instance;
            if (ed.cache_dic.TryGetValue(key_name, out var current_relic_id))
            {
                relic_id = (uint)current_relic_id;
            }
            else
            {
                EX_Utility.dic_cover_add(ref ed.cache_dic, key_name, relic_id);
            }

            AutoCodes.relics.TryGetValue($"{relic_id}", out var r_relic);
            var relic_name = $"{Localization_Utility.get_localization(r_relic.name)}";
            string[] new_strs = new[] { relic_name, $"{relic_num}" };

            bool? is_interactable = null;
            System.Action ac = btn_on_click;

            owner.change(replace_strs, new_strs, is_interactable, ac);


            #region 子函数 btn_on_click
            void btn_on_click()
            {
                for (int i = 0; i < relic_num; i++)
                {
                    Drop_Relic_Helper.drop_relic(relic_id);
                }

                Debug.Log($"获得遗物{relic_name}，数量{relic_num}");
            }
            #endregion
        }
    }
}

