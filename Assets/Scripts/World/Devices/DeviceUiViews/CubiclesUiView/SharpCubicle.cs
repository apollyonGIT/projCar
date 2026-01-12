using UnityEngine.UI;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.CubiclesUiView
{
    public class SharpCubicle : CubicleUiView
    {
        public Image sharp_progress;
        protected override void tick()
        {
            if (owner is DeviceCubicle d && d.device is ISharp sharp && sharp_progress!=null)
            {
                sharp_progress.fillAmount = sharp.Sharpness_Current / 100f;
            }
        }
    }
}
