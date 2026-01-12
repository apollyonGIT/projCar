using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews.DevicePanel_Attachment
{
    /// <summary>
    /// 弓弦
    /// </summary>
    public class DevicePanelAttachment_Bow_String : DevicePanelAttachment
    {
        public Image left_string, right_string;
        public float half_string_length;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action)
        {

        }

        // ===============================================================================

        public void Update_String_View(float pull_length)
        {
            var angle = Mathf.Atan2(pull_length, half_string_length);

            left_string.transform.localScale = new Vector3(1 / Mathf.Cos(angle), 1, 1);
            right_string.transform.localScale = new Vector3(1 / Mathf.Cos(angle), 1, 1);

            left_string.transform.localRotation = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg);
            right_string.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }

        public void Update_String_View(Color color)
        {
            left_string.color = color;
            right_string.color = color;
        }

        public void Update_String_View(float pull_length, Color color)
        {
            Update_String_View(pull_length);
            Update_String_View(color);
        }

    }
}
