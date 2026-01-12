using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Melee_Tentacle :BasicMelee_Click
    {
        protected override void rotate_bone_to_target(string bone_name)
        {
            if (target_list.Count == 0)
                return;

            var target = target_list[0];
            Vector2 target_v2;
            target_v2 = BattleUtility.get_v2_to_target_collider_pos(target, position);
            var angle = Vector2.Angle(Vector2.right, target_v2);
            if (angle > 180)
            {
                target_v2 = Vector2.left;
            }
            else if(angle < 0)
            {
                target_v2 = Vector2.right;
            }
            bones_direction[bone_name] = BattleUtility.rotate_v2(bones_direction[bone_name], target_v2, rotate_speed);
        }


        protected override void rotate_bone_to_dir(string bone_name, Vector2 dir)
        {
            var target_v2 = dir;    
            var angle = Vector2.Angle(Vector2.right, target_v2);
            if (angle > 180)
            {
                target_v2 = Vector2.left;
            }
            else if (angle < 0)
            {
                target_v2 = Vector2.right;
            }

            bones_direction[bone_name] = BattleUtility.rotate_v2(bones_direction[bone_name], target_v2, rotate_speed);
        }
    }
}
