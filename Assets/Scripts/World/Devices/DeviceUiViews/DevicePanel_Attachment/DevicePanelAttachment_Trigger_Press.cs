using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 为保证操作手感，制作了此类并非点击触发，而是按下鼠标就会立刻触发的组件。
    /// 使用方法：挂载到Prefab中需要点击触发的组件上。
    /// </summary>
    public class DevicePanelAttachment_Trigger_Press : DevicePanelAttachment_Trigger
    {

        private Action trigger_action;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action = null)
        {
            if (action != null)
                trigger_action = action[0];
        }

        // ===============================================================================

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
                return;
            trigger_action?.Invoke();
            Player_Interacting = true;
        }

    }
}
