using Foundations;
using System;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class C1_dice_game : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        const string key_flag = "C1_dice_game_flag";
        const string key_next_node_uname = "@C1?";

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            var cache_dic = Encounter_Dialog.instance.cache_dic;

            Action ac = () => 
            {
                var next_node_uname = valid();
                EX_Utility.dic_cover_add(ref cache_dic, key_next_node_uname, next_node_uname);

                Share_DS.instance.add(key_next_node_uname, next_node_uname);
            };

            Share_DS.instance.add("jump_ac", ac);
        }


        string valid()
        {
            Share_DS.instance.try_get_value("player_score", out int player_score);
            Share_DS.instance.try_get_value("player_trun_score", out int player_trun_score);
            if (player_score + player_trun_score >= 1500)
                return "#success";

            Share_DS.instance.add("dice_game_turn", "ai");
            return "#ai";
        }
    }
}

