using Commons;
using Foundations;
using UnityEngine;

namespace World.Devices.DeviceViews
{
    public class MeleeView:DeviceView_Spine
    {
        public float offset_angle = 0f;

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
                    Vector2 _dir = dir;
                    if(bone_name == "roll_control")
                    {
                        _dir = Quaternion.AngleAxis(offset_angle, Vector3.forward) * dir;
                    }

                    if (bone != null)
                    {
                        if (Vector2.SignedAngle(Vector2.right, _dir) > 90)
                        {
                            anim.skeleton.FlipX = true;
                            bone.Rotation = Vector2.SignedAngle(_dir, Vector2.left);
                        }
                        else if (Vector2.SignedAngle(Vector2.right, _dir) < -90)
                        {
                            anim.skeleton.FlipX = true;
                            bone.Rotation = Vector2.SignedAngle(_dir, Vector2.left);
                        }
                        else
                        {
                            anim.skeleton.FlipX = false;
                            bone.Rotation = Vector2.SignedAngle(Vector2.right, _dir);
                        }
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

            foreach (var collider in colliders)
            {
                collider.collider_tick();
            }
        }
    }
}
