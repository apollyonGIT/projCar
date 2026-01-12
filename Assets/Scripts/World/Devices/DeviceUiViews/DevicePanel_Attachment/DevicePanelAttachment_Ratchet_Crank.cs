using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 拉杆棘轮。
    /// 使用需要将此脚本挂载在Prefab中拉杆曲柄的头部。
    /// </summary>
    public class DevicePanelAttachment_Ratchet_Crank : DevicePanelAttachment_Ratchet
    {
        public override void Init(List<Action> action = null)
        {
            base.Init(action);
            dragging_point = GetComponent<Transform>();
        }

        protected override void Init_Actions(List<Action> action)
        {

        }
    }
}
