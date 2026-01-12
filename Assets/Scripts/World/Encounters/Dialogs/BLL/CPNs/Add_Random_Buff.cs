using Foundations;
using System.Collections.Generic;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Add_Random_Buff : MonoBehaviour, IEncounter_Dialog_CPN
    {
        static List<string> m_buffs = new()
        { 
            "奥术智慧",
            "灵魂石",
            "铁甲术",
            "奈萨里奥的庇护",
            "熔岩武器",
            "回复",
            "死亡缠绕"
        };

        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            CPN_Utility.get_replace_strs_quick(args, out var replace_strs);

            var index = Random.Range(0, m_buffs.Count);
            var buff_id = $"{m_buffs[index]}";

            //临时：数量为1
            add_buff(owner, replace_strs, m_key_name, buff_id, 1);
        }


        public static void add_buff(IEncounter_Dialog_Window_UI owner, string[] replace_strs, string key_name, string buff_id, int buff_num)
        {
            var ed = Encounter_Dialog.instance;
            if (ed.cache_dic.TryGetValue(key_name, out var current_buff_id))
            {
                buff_id = (string)current_buff_id;
            }
            else
            {
                EX_Utility.dic_cover_add(ref ed.cache_dic, key_name, buff_id);
            }

            //临时
            var buff_name = buff_id;
            string[] new_strs = new[] { buff_name, $"{buff_num}" };

            bool? is_interactable = null;
            System.Action ac = btn_on_click;

            owner.change(replace_strs, new_strs, is_interactable, ac);


            #region 子函数 btn_on_click
            void btn_on_click()
            {
                for (int i = 0; i < buff_num; i++)
                {
                }

                Debug.Log($"获得祝福{buff_name}，数量{buff_num}");
            }
            #endregion
        }
    }
}

