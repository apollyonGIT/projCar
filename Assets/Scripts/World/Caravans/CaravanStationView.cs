using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.Device_AI;
using World.Helpers;

namespace World.Caravans
{
    public class CaravanStationView : MonoBehaviour,IPointerClickHandler
    {
        public BasicModule module;

        public Image character_image;

        public void Init(BasicModule module)
        {
            this.module = module;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (module == null)
                return;

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Character_Module_Helper.EmptyModule(module);
            }
            else
            {
                Character_Module_Helper.SetModule(module);
            }
        }
    }
}
