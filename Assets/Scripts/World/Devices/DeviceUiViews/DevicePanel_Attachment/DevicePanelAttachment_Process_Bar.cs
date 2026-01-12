using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public class DevicePanelAttachment_Process_Bar : DevicePanelAttachment
    {
        public GameObject bg;
        public Image process;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action = null)
        {

        }

        // ===============================================================================

        public void Update_View(float value_01, bool show_image)
        {
            bg.SetActive(show_image);
            process.gameObject.SetActive(show_image);
            if (show_image)
                process.fillAmount = value_01;
        }

    }
}
