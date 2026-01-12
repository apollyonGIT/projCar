using UnityEngine;
using UnityEngine.UI;
using World.Characters;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.CubiclesUiView
{
    public class AttackCubicle : CubicleUiView
    {
        public Image attack_progress;

        protected override void tick()
        {
            if(owner is DeviceCubicle d && d.device is IAttack iattack && attack_progress!=null)
            {
                attack_progress.fillAmount = (float)iattack.Current_Interval / iattack.Attack_Interval;
            }
        }

        public override void notify_set_worker(Character c)
        {
            base.notify_set_worker(c);
            if (attack_progress != null)
            {
                if (c == null)
                {
                    attack_progress.color = Color.black;
                }
                else
                {
                    Color newColor;
                    ColorUtility.TryParseHtmlString("#00ffd0", out newColor);
                    attack_progress.color = newColor;
                }
            }
        }
    }
}
