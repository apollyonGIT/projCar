using AutoCodes;
using Commons;
using Foundations;
using UnityEngine;
using static World.WorldEnum;
using World.Projectiles;

namespace World.Helpers
{
    public class Enemy_Shoot_Helper
    {
        public static void @do(Enemys.Enemy enemy, Vector2 projectile_init_pos, Vector2 shoot_dir, float projectile_speed)
        {
            AutoCodes.monsters.TryGetValue($"{enemy._desc.id}", out var monster_r);
            if (!AutoCodes.fire_logics.TryGetValue($"{monster_r.fire_logic}", out var fire_logic_r))
                return;

            #region prms
            int prm_salvo = (int)fire_logic_r.salvo;
            float prm_angle = fire_logic_r.angle;

            uint prm_projectile_id = fire_logic_r.projectile_id;
            var prm_damage = fire_logic_r.damage;

            float prm_speed = projectile_speed;
            float prm_init_speed = projectile_speed;

            int prm_life_ticks = fire_logic_r.projectile_life_ticks.Item2 == 0 ? fire_logic_r.projectile_life_ticks.Item1 : Random.Range(fire_logic_r.projectile_life_ticks.Item1, fire_logic_r.projectile_life_ticks.Item2);
            #endregion

            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);

            for (int i = 0; i < prm_salvo; i++)
            {
                var angle = 2 * prm_angle;
                var ave_a = angle / prm_salvo;
                var angle_1 = -prm_angle + (prm_salvo - i - 1) * ave_a;
                var angle_2 = prm_angle - i * ave_a;

                projectiles.TryGetValue(prm_projectile_id.ToString(), out var projectile_record);
                float rot_speed = projectile_record.inertia_moment > 0 ? projectile_record.mass * prm_init_speed / projectile_record.inertia_moment : 0;
                rot_speed *= Random.Range(-1f, 1f);

                var p = BattleUtility.GetProjectile(projectile_record.ammo_type);

                var attack_datas = new System.Collections.Generic.List<Attack_Data>();
                foreach (var d in prm_damage)
                {
                    Attack_Data attack_data = new()
                    {
                        atk = d.Value,
                        diy_atk_str = d.Key,
                    };

                    attack_datas.Add(attack_data);
                }

                p.Init(projectile_record, shoot_dir, projectile_init_pos, enemy.velocity, angle_1, angle_2, prm_speed, prm_init_speed, Faction.opposite, prm_life_ticks,enemy, attack_datas, rot_speed);
                pmgr.AddProjectile(p);
            }
        }

    }
}
