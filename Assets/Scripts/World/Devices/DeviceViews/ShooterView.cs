using Spine.Unity;
using UnityEngine;
using World.Devices.Device_AI;

namespace World.Devices.DeviceViews
{
    public class ShooterView : DeviceView_Spine
    {
        public LineRenderer aim_line;

        public LineRenderer radius_line;
        public LineRenderer inner_radius_line;

        public SkeletonUtilityBone arrow_bone;
        public SkeletonUtilityBone spring_bone;

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if (owner.player_oper && dkp.Count > 0 && aim_line != null)
            {
                aim_line.positionCount = 2;
                var cv = WorldContext.instance.caravan_dir.normalized;
                var bv = owner.bones_direction["roll_control"];
                var v1 = dkp[0].transform.position;
                var v2 = new Vector3(v1.x + (bv.x * cv.x - bv.y * cv.y) * 100, v1.y + (bv.x * cv.y + bv.y * cv.x) * 100, v1.z);

                if (!owner.desc.rotate_by_car)
                {
                    v2 = new Vector3(v1.x + bv.x * 100, v1.y + bv.y * 100, v1.z);
                }

                aim_line.SetPosition(0, v1);
                aim_line.SetPosition(1, v2);
            }


            if (radius_line != null && radius_line.positionCount != 0)
            {
                DrawCircle(transform.position, owner.desc.basic_range.Item2, radius_line);
            }

            if (inner_radius_line != null && inner_radius_line.positionCount != 0)
            {
                DrawCircle(transform.position, owner.desc.basic_range.Item1, inner_radius_line);
            }

            if (owner is Shooter_Bow sb)
            {
                if (sb.fsm == DScript_Shooter.Device_FSM_Shooter.idle)
                {
                    var delta = 0.23f; 
                    var factor = sb.TriggerableSpring_for_Shooting.Spring_Value_01;
                    var dir = owner.bones_direction["roll_control"].normalized;
                    var default_pos = transform.position + new Vector3(dir.x, dir.y, 0) * delta;


                    var pos = default_pos - new Vector3(dir.x, dir.y, 0) * delta * factor;

                    arrow_bone.transform.position = new Vector3(pos.x, pos.y, 10);
                    spring_bone.transform.position = new Vector3(pos.x, pos.y, 10);
                }
            }
        }

        public override void notify_player_oper(bool oper)
        {
            if (aim_line != null)
            {
                if (oper)
                    aim_line.enabled = true;
                else
                    aim_line.enabled = false;
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
