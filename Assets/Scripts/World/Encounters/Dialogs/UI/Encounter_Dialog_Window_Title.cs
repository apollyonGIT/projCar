using TMPro;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog_Window_Title : MonoBehaviour, IEncounter_Dialog_Window_UI
    {
        public TextMeshProUGUI content;

        //==================================================================================================

        void IEncounter_Dialog_Window_UI.change(params object[] args)
        {
            var replace_str = (string[])args[0];
            var new_str = (string[])args[1];

            if (replace_str != null)
            {
                var _content = content.text;
                for (int i = 0; i < replace_str.Length; i++)
                {
                    _content = _content.Replace(replace_str[i], CPN_Utility.get_cache_str_value(new_str[i]));
                }
                content.text = _content;
            }
        }
    }
}

