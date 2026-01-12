using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public class DevicePanelAttachment_Beat : DevicePanelAttachment
    {
        public Sprite[] right_sprite;
        public Sprite[] left_sprite;
        protected override void Init_Actions(List<Action> action)
        {
            
        }

        public void SetBeat(bool is_right, int state)
        {
            if (is_right)
            {
                GetComponent<Image>().sprite = right_sprite[state];
            }
            else
            {
                GetComponent<Image>().sprite = left_sprite[state];
            }
        }
    }
}
