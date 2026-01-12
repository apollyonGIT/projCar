using Foundations;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class C1_traveller_peasant : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        const string key_flag = "C1_traveller_peasant_flag";
        const string key_next_node_uname = "@C1?";

        string[] next_node_uname_array = new[] 
        { 
            "C1_0",
            "C1_1",
            "C1_2",
        };

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            var next_node_uname = next_node_uname_array[0];

            var cache_dic = Encounter_Dialog.instance.cache_dic;
            if (!cache_dic.ContainsKey(key_flag))
            {
                cache_dic.Add(key_flag, "done");

                //规则：等比例随机抽取
                next_node_uname = CPN_Utility.get_random_cell(next_node_uname_array);
                cache_dic.Add(key_next_node_uname, next_node_uname);
            }
            else
            {
                cache_dic.TryGetValue(key_next_node_uname, out var _next_node_uname);
                next_node_uname = (string)_next_node_uname;
            }

            Share_DS.instance.add(key_next_node_uname, next_node_uname);
        }
    }
}

