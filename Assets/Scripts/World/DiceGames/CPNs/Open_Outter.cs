using Foundations;
using System.Collections.Generic;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Open_Outter : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            var outter_go_name = args[0];
            var is_active = bool.Parse(args[1]);

            System.Action ac = btn_on_click;
            owner.change(null, null, true, ac);


            #region 子函数 btn_on_click
            void btn_on_click()
            {
                Encounter_Dialog.instance.cache_dic.TryGetValue("outter_go_infos", out var _outter_go_infos);
                
                var outter_go_infos = (Dictionary<string, bool>)_outter_go_infos;                
                EX_Utility.dic_cover_add(ref outter_go_infos, outter_go_name, is_active);
                
                Encounter_Dialog.instance.cache_dic["outter_go_infos"] = outter_go_infos;
            }
            #endregion
        }


    }
}

