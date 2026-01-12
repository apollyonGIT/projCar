using Commons;
using Foundations;
using Spine.Unity;
using UnityEngine;
using World.Devices.Device_AI;
using static World.Devices.Device_AI.NewBasicHook;

namespace World.Devices.DeviceViews
{
    public class BasicHookView : DeviceView_Spine
    {
        public SkeletonUtilityBone hook;

        public LineRenderer radius_line;
        public LineRenderer inner_radius_line;
        public override void notify_on_tick()
        {
            transform.localPosition = owner.position;
            transform.localRotation = EX_Utility.look_rotation_from_left(WorldContext.instance.caravan_dir);
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

            if (radius_line != null && radius_line.positionCount != 0)
            {
                DrawCircle(transform.position, owner.desc.basic_range.Item2, radius_line);
            }

            if (inner_radius_line != null && inner_radius_line.positionCount != 0)
            {
                DrawCircle(transform.position, owner.desc.basic_range.Item1, inner_radius_line);
            }

            var hk = owner as NewBasicHook;

            switch (hk.fsm)
            {
                case Device_FSM_Hook.Shooting:
                case Device_FSM_Hook.Shooting_reeling_in:
                case Device_FSM_Hook.Hooked:
                case Device_FSM_Hook.Breaking_reeling_in:
                    hook.transform.position = new Vector3(hk.hook_position.x, hk.hook_position.y, 10);
                    break;
                default:
                    hook.transform.localPosition = Vector3.zero;
                    break;
            }         

        }

        public override void notify_attack_radius(bool show)
        {
            if (show)
            {
                DrawCircle(transform.position, owner.desc.basic_range.Item2, radius_line);
                DrawCircle(transform.position, owner.desc.basic_range.Item1, inner_radius_line);
            }
            else
            {
                radius_line.positionCount = 0;
                inner_radius_line.positionCount = 0;
            }
        }

        private void DrawCircle(Vector3 center, float radius, LineRenderer lineRenderer)
        {
            lineRenderer.positionCount = 181;
            float x;
            float y;
            float z = 10;
            float angle = 0f;
            for (int i = 0; i < 181; i++)
            {
                x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                y = center.y + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                lineRenderer.SetPosition(i, new Vector3(x, y, z));
                angle += (360f / 180);
            }
        }
    }
}
