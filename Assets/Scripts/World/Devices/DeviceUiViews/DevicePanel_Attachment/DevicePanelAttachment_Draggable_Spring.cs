using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 可拖动的弹簧，会在松手时复位。
    /// 用于模拟弓弦、枪栓等设备。
    /// 使用时，请将此脚本挂载在Prefab中不需要拖动的基座上。
    /// </summary>
    public class DevicePanelAttachment_Draggable_Spring : DevicePanelAttachment_Draggable, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        private Action on_drag_end;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action = null)
        {
            if (action != null)
                on_drag_end = action[0];
        }

        // ===============================================================================

        public override void OnBeginDrag(PointerEventData eventData)
        {
            topper_move_distance_abs = 0;
            base.OnBeginDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            on_drag_end?.Invoke();  // 执行顺序应当放在置零之前，否则数据中的Triggered一定会被置为false
            topper_move_distance_abs = 0;
            topper_rt.anchoredPosition = topper_pos_init;    //顶部部件回到初始位置
            base.OnEndDrag(eventData);
        }
    }
}
