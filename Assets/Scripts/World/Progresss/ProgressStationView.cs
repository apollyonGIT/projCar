using Addrs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.Device_AI;
using World.Helpers;

namespace World.Progresss
{
    public class ProgressStationView : MonoBehaviour,IPointerClickHandler
    {
        public BasicModule module;
        public Image character_image;

        public void Init(BasicModule module)
        {
            this.module = module;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(module == null)
            {
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Character_Module_Helper.EmptyModule(module);
            }
            else
            {
                Character_Module_Helper.SetModule(module);
            }
        }

        public void tick()
        {
            if (Character_Module_Helper.GetModule(module) != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(Character_Module_Helper.GetModule(module).desc.portrait_small, out var s);
                character_image.sprite = s;
            }
            else
            {
                character_image.sprite = null;
            }
        }
    }
}
