using TMPro;
using UnityEngine;

namespace World.Characters
{
    public class CharacterMindHoveringInfo :MonoBehaviour
    {
        public TextMeshProUGUI reason_text;
        public TextMeshProUGUI delta_text;

        public void Init(string reason,string delta)
        {
            reason_text.text = reason;
            delta_text.text = delta;
        }
    }
}
