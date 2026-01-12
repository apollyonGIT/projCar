using TMPro;
using UnityEngine;
using World.Characters;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.CubiclesUiView
{
    public class LoadingCubicle : CubicleUiView
    {
        public TextMeshProUGUI ammo_text;

        protected override void tick()
        {
            if(owner is DeviceCubicle d && d.device is ILoad iload && ammo_text!=null)
            {
                ammo_text.text = $"{iload.Current_Ammo}/{iload.Max_Ammo}";
            }
        }

        public override void notify_set_worker(Character c)
        {
            base.notify_set_worker(c);
            if (ammo_text != null)
            {
                if (c == null)
                {
                    ammo_text.color = Color.black;
                }
                else
                {
                    Color newColor;
                    ColorUtility.TryParseHtmlString("#00ffd0", out newColor);
                    ammo_text.color = newColor;
                }
            }
        }
    }
}
