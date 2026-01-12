using UnityEngine;
using World.Devices.Device_AI;


namespace World.Devices
{
    public class Shooter_Trebuchet_Outdated : NewBasicShooter
    {
        // Const
        private const float SHOOT_DEG_OFFSET = 72F;


        // Fields
        override protected float shoot_deg_offset => SHOOT_DEG_OFFSET;



        public void UI_Controlled_Turn_Dir(float dir_x) => bones_direction[BONE_FOR_ROTATE] = new Vector2(dir_x, 0.001f);
        public float Get_Dir_X() => Mathf.Clamp(bones_direction[BONE_FOR_ROTATE].x, -1f, 1f);

        // ===================================================================================


        override protected bool can_auto_shoot()
        {
            // Base:
            // return can_shoot_check_cd && can_shoot_check_error_angle() && can_shoot_check_ammo && can_shoot_check_post_cast;
            return can_shoot_check_cd && can_shoot_check_enemy_pos() && can_shoot_check_ammo && can_shoot_check_post_cast;

            bool can_shoot_check_enemy_pos()
            {
                // 抛石机有射界限制，无法命中自己头顶的敌人。
                if (target_list.Count == 0)
                    return false;

                var t_pos_delta = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
                var abs_dx = Mathf.Abs(t_pos_delta.x);
                return Mathf.Atan2(t_pos_delta.y, abs_dx) * Mathf.Rad2Deg < SHOOT_DEG_OFFSET;
            }
        }
    }
}

