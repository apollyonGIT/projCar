using Foundations;
using UnityEngine;

namespace World.Characters
{
    public class CharacterImage : MonoBehaviour
    {
        private CharacterMgr owner;

        private Vector3 init_position;

        public void init(CharacterMgr owner)
        {
            this.owner = owner;
            init_position = transform.GetComponent<RectTransform>().anchoredPosition;
        }

        public void tick()
        {
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(transform.GetComponent<RectTransform>().anchoredPosition.x,init_position.y + owner.character_focus_pos.y  * Commons.Config.current.role_offset_coef_y);
            var angle = owner.character_focus_pos.x * Commons.Config.current.role_offset_coef_x;

            transform.parent.localRotation = Quaternion.Euler(0, 0, angle);
        }

        public void revert()
        {
            transform.GetComponent<RectTransform>().anchoredPosition = init_position;
            transform.parent.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
