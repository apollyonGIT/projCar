using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 棘轮-转轮。
    /// 使用需要将此脚本挂载在Prefab中的交互响应区域（一般是转盘图像主体处）。
    /// </summary>
    public class DevicePanelAttachment_Ratchet_Wheel : DevicePanelAttachment_Ratchet, IBeginDragHandler, IEndDragHandler
    {

        public RectTransform dragging_point_parent;

        // -------------------------------------------------------------------------------

        private GameObject click_pos;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action)
        {

        }

        // ===============================================================================

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            click_pos = new GameObject("ClickPoint");
            dragging_point = click_pos.AddComponent<RectTransform>();
            dragging_point.position = mouse_pos;
            dragging_point.SetParent(dragging_point_parent, false);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            Destroy(click_pos.gameObject);
        }
    }
}
