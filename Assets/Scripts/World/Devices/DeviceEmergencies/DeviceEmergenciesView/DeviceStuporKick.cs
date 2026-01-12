using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class DeviceStuporKick : MonoBehaviour, IPointerClickHandler,IPointerDownHandler,IPointerUpHandler
    {
        public DeviceStuporView owner;
        public void OnPointerClick(PointerEventData eventData)
        {
            (owner.owner as DeviceStupor).Kick();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 20f);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 30f);
        }
    }
}
