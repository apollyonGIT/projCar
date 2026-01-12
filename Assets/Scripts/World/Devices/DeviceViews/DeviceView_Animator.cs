using Foundations;
using UnityEngine;
using UnityEngine.UIElements;

namespace World.Devices.DeviceViews
{
    public class DeviceView_Animator : DeviceView
    {
        public DeviceAnimator device_animator; 
        public override void notify_change_anim(string anim_name, bool loop)
        {
            var trigger_name = "Trigger_" + char.ToUpper(anim_name[0]) + anim_name.Substring(1);

            device_animator.SetTrigger(trigger_name);
        }

        public override void notify_hurt(int dmg)
        {
            base.notify_hurt(dmg);

            device_animator.SetTrigger("Trigger_Hurt");
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            foreach (var (bone_name, dir) in owner.bones_direction)
            {
                if(bone_name == "roll_control")
                {
                    var angle = Vector2.SignedAngle(Vector2.right,dir);
                    device_animator.SetFloat("Float_FixedAngle", (angle + 360) % 360 / 360f);
                }
               
            }
            device_animator.tick();
        }
    }
}
