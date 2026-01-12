using Foundations;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog_Window_Btn_Option : MonoBehaviour, IEncounter_Dialog_Window_UI
    {
        public Button btn_option;
        public TextMeshProUGUI[] btn_option_contents;

        public List<Action> btn_click_ac_list = new();

        //==================================================================================================

        void IEncounter_Dialog_Window_UI.change(params object[] args)
        {
            var replace_str = (string[])args[0];
            var new_str = (string[])args[1];
            var is_interactable = args[2] as bool?;
            var btn_on_click = (Action)args[3];

            if (replace_str != null)
            {
                foreach (var btn_option_content in btn_option_contents)
                {
                    var _btn_option_name = btn_option_content.text;
                    for (int i = 0; i < replace_str.Length; i++)
                    {
                        if (string.IsNullOrEmpty(replace_str[i])) continue;
                        _btn_option_name = _btn_option_name.Replace(replace_str[i], CPN_Utility.get_cache_str_value(new_str[i]));
                    }
                    btn_option_content.text = _btn_option_name;
                }
            }

            if (btn_option.interactable && is_interactable.HasValue)
                btn_option.interactable = is_interactable.Value;

            if (btn_on_click != null)
                btn_click_ac_list.Add(btn_on_click);
        }
    }
}

