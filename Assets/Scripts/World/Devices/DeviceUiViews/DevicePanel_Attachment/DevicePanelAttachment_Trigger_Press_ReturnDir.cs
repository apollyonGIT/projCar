using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 可以获取点击方向的触发器
    /// </summary>
    public class DevicePanelAttachment_Trigger_Press_ReturnDir : DevicePanelAttachment_Trigger
    {
        public RectTransform fan_center;

        // -------------------------------------------------------------------------------

        private Vector2 mouse_pos;
        private Action trigger_action;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action = null)
        {
            if (action != null)
                trigger_action = action[0];
        }

        // ===============================================================================

        public Vector2 Get_Raw_Dir_Of_Click()
        {
            return mouse_pos - (Vector2)fan_center.position;
        }

        // ===============================================================================

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(fan_center, eventData.position, eventData.pressEventCamera, out var mouse_position);
            mouse_pos = mouse_position;

            trigger_action?.Invoke();
            Player_Interacting = true;
        }

    }
}
