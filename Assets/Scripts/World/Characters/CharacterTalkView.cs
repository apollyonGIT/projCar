using Commons;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace World.Characters
{
    public class CharacterTalkView : MonoBehaviour
    {

        public TextMeshProUGUI talk_text;
        
        public void init(List<string> talk_strings)
        {
            var r = Random.Range(0, talk_strings.Count);
            talk_text.text = Localization_Utility.get_localization(talk_strings[r]);
        }
    }
}
