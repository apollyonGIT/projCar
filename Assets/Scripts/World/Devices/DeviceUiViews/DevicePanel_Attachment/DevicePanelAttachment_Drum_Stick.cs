using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    public class DevicePanelAttachment_Drum_Stick : DevicePanelAttachment,IPointerClickHandler,IPointerUpHandler, IPointerDownHandler
    {
        public bool is_right;

        public Action click_action;

        public void OnPointerDown(PointerEventData eventData)
        {
            click_action?.Invoke();
            if (is_right)
            {
                transform.localEulerAngles = new(0, 0, 20);
            }
            else
            {
                transform.localEulerAngles = new(0, 0, -20);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (is_right)
            {
                transform.localEulerAngles = new(0, 0, 0);
            }
            else
            {
                transform.localEulerAngles = new(0, 0, 0);
            }
        }

        protected override void Init_Actions(List<Action> action)
        {
            click_action = action[0];
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            
        }
    }
}
