using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 支持长按并连续触发效果的设备面板附件。
    /// 使用方法：挂载到Prefab中需要点击触发的组件上。
    /// </summary>
    public class DevicePanelAttachment_Trigger_Hold : DevicePanelAttachment_Trigger, IPointerExitHandler
    {

        private Action down_action, up_action;
        private bool in_area = false;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action = null)
        {
            if(action.Count > 0)
                down_action = action[0];
            if (action.Count > 1)
                up_action = action[1];
        }

        // ===============================================================================

        /// <summary>
        /// 设备面板需要每帧持续调用这一函数，检查控件是否被持续按下。
        /// </summary>
        /// <returns></returns>
        public bool Is_Holding(bool allow_out_of_area)
        {
            return Player_Interacting && (allow_out_of_area || in_area);
        }

        // ===============================================================================

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
                return;
            Player_Interacting = true;
            in_area = true;
            down_action?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            in_area = false;
            base.OnPointerUp(eventData);
            up_action?.Invoke();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            in_area = false;
        }

    }
}
