using UnityEngine;
using UnityEngine.EventSystems;
using World.Characters;
using World.Cubicles;
using World.Helpers;

namespace World.Devices.DeviceUiViews
{
    public class DeviceStationView : MonoBehaviour,IUiCharacter
    {
        public BasicCubicle cubicle;

        public CharacterView cubicle_character;

        public void InitCub(BasicCubicle cubicle)
        {
            cubicle_character.init(null);           //用于初始化character_cubicle的init_position
            this.cubicle = cubicle;
        }

        public void SetCharacter(Character c)
        {
            if (c == null)
                cubicle_character.character_image.revert();
            cubicle_character.init(c);
        }

        public void tick()
        {
            cubicle_character.tick();
            cubicle_character.character_image.tick();
        }

        

        void IUiCharacter.UiCancelCharacter()
        {
            
        }

        void IUiCharacter.UiSetCharacter(Character c)
        {
            Cubicle_Helper.SetCubicle(cubicle);
        }

        public void PointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Cubicle_Helper.EmptyCubicle(cubicle);
            }
            else
            {
                Cubicle_Helper.SetCubicle(cubicle);
            }
        }
    }
}
