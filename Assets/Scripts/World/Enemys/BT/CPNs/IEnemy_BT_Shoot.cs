using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public interface IEnemy_BT_Shoot
    {
        string muzzle_bone_name { get; }

        int shoot_cd_max { get; set; }
        Vector2 projectile_speed_range { get; set; }

        //==================================================================================================

        public void init_data(Enemy cell)
        {
            AutoCodes.monsters.TryGetValue($"{cell._desc.id}", out var monster_r);
            if (AutoCodes.fire_logics.TryGetValue($"{monster_r.fire_logic}", out var fire_logic_r))
            {
                shoot_cd_max = fire_logic_r.cd;
                projectile_speed_range = new(fire_logic_r.speed.Item1, fire_logic_r.speed.Item2);
            }
        }


        public void do_shoot(Enemy cell, Vector2 shoot_target_pos)
        {
            cell.try_get_bone_pos(muzzle_bone_name, out Vector2 muzzle_pos);

            var dx = Mathf.Abs(shoot_target_pos.x - muzzle_pos.x);

            var target_vx = cell.target != null ? cell.target.velocity.x : 0;
            var t = dx * 0.05f + target_vx * 0.1f;

            var projectile_speed = Mathf.Lerp(projectile_speed_range.x, projectile_speed_range.y, t);

            if (!parabolic_trajectory_prediction(muzzle_pos, shoot_target_pos, projectile_speed, out Vector2 target_dir))
            {
                target_dir.y = Mathf.Max(target_dir.y, Mathf.Abs(target_dir.x));
            }

            Enemy_Shoot_Helper.@do(cell, muzzle_pos, target_dir, projectile_speed);
        }


        //抛物线预测，黑盒，未来替换
        bool parabolic_trajectory_prediction(Vector2 start_pos, Vector2 target_pos, float init_speed, out Vector2 target_dir, bool high_trajectory = false)
        {
            var L = target_pos.x - start_pos.x;
            var H = target_pos.y - start_pos.y;
            var g = -Config.current.gravity;

            var target_left = L < 0;
            if (target_left)
                L = -L;

            var discriminant_t = init_speed * init_speed - 2 * g * H < 0;
            if (discriminant_t)
                return false_return(out target_dir);

            #region Parms Explain
            // define: sq = sqrt( L^2 + H^2 )
            // ( g * L^2 / v^2 + H ) / sq = ( L / sq * sin2θ - H / sq * cos2θ )
            // define: cosφ = L / sq ; sinφ = H / sq
            // ( g * L^2 / v^2 + H ) / sq = sin( 2θ - φ )
            #endregion

            var discriminant_sin = (g * Mathf.Pow(L / init_speed, 2) + H) / Mathf.Sqrt(L * L + H * H);
            if (Mathf.Abs(discriminant_sin) > 1)
                return false_return(out target_dir);

            // all angles are in radians
            var phi = Mathf.Atan2(H, L);   // tanφ = H / L

            var double_theta_1 = Mathf.Asin(discriminant_sin);
            var double_theta_2 = Mathf.PI - double_theta_1;

            float repeat_right_semisphere(float input) => (input + Mathf.PI * 0.5f) % Mathf.PI - Mathf.PI * 0.5f;

            var angle_rad_1 = repeat_right_semisphere((double_theta_1 + phi) * 0.5f);
            var angle_rad_2 = repeat_right_semisphere((double_theta_2 + phi) * 0.5f);

            var shooting_angle = high_trajectory ? Mathf.Max(angle_rad_1, angle_rad_2) : Mathf.Min(angle_rad_1, angle_rad_2);

            #region Check
            //float vx = init_speed * Mathf.Cos(shooting_angle);
            //float vy = init_speed * Mathf.Sin(shooting_angle);
            //float tx_check = L / vx;
            //float delta_check = Mathf.Sqrt(vy * vy - 2 * g * H);
            //float ty_check_1 = (vy + delta_check) / g;
            //float ty_check_2 = (vy - delta_check) / g;
            #endregion

            target_dir = new Vector2(Mathf.Cos(shooting_angle), Mathf.Sin(shooting_angle));

            if (target_left)
                target_dir.x = -target_dir.x;

            return true;

            bool false_return(out Vector2 target_dir)
            {
                target_dir = target_pos - start_pos;
                var tdx = Mathf.Abs(target_dir.x);
                if (target_dir.y < tdx)
                    target_dir.y = tdx;
                return false;
            }
        }  

    }
}

