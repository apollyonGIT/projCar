using System.Collections.Generic;
using UnityEngine;
using World.Cubicles;


namespace World.Devices.DeviceUiViews
{
    public static class DeviceUIView_Common_Action
    {
        public static void Set_Highlight_By_If_Cubicle_Has_Worker(DeviceCubicle dc, List<DevicePanelAttachment_Highlightable> hls)
        {
            if (dc != null && hls != null)
                foreach (var highlight in hls)
                    highlight.Auto = dc.worker != null;
        }

    }


    public class DeviceUI_Component_Blinker
    {
        #region Const
        private const short BLINK_INTERVAL_MAX = 15;
        #endregion

        private short blink_interval;
        private bool blink_state;

        public void update_blink(ref UnityEngine.UI.Image img, Color color_darker)
        {
            if (--blink_interval <= 0)
            {
                blink_interval = BLINK_INTERVAL_MAX; // BLINK_TICK_MAX
                blink_state = !blink_state;
            }
            img.color = blink_state ? color_darker : new Color(255f, 129f, 129f);
        }

        public void stop_blink(ref UnityEngine.UI.Image img)
        {
            img.color = Color.white;
        }
    }
}
