using Commons;
using Foundations;
using UnityEngine;

namespace World.Devices.DeviceViews
{
    public class WheelView : DeviceView_Spine
    {
        public override void notify_on_tick()
        {
            transform.localPosition = owner.position;
            transform.localRotation = EX_Utility.look_rotation_from_left(WorldContext.instance.caravan_dir);


            if (owner.device_type == Device.DeviceType.melee)
                transform.localScale = Vector3.one * BattleContext.instance.melee_scale_factor;

            if (anim != null)
            {
                anim.Update(Config.PHYSICS_TICK_DELTA_TIME);

                foreach (var (bone_name, dir) in owner.bones_direction)
                {
                    var bone = anim.skeleton.FindBone(bone_name);
                    if (bone != null)
                    {
                        bone.Rotation = Vector2.SignedAngle(Vector2.right, dir);
                    }
                }

                foreach (var anim_event in owner.anim_events)
                {
                    if (anim_event.anim_name == anim.AnimationName)
                    {
                        anim_event_trigger(anim_event);
                    }
                }
            }
        }
    }
}