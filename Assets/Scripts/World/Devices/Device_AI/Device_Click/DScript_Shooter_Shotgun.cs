using AutoCodes;
using Commons;
using Foundations;
using System;
using UnityEngine;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class DScript_Shooter_Shotgun : DScript_Shooter
    {
        protected override void idle_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();

            current_shoot_cd = Mathf.Max(0, current_shoot_cd - 1);
            if (current_ammo == 0)
            {
                FSM_change_to(Device_FSM_Shooter.reloading);
            }
        }

        protected override void shoot_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();

            current_shoot_cd = Mathf.Max(0, current_shoot_cd - 1);
        }

        protected override void reloading_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();

            current_shoot_cd = Mathf.Max(0, current_shoot_cd - 1);
            self_reloading_process += fire_logic_data.reload_speed;

            if (self_reloading_process >= 1)
            {
                self_reloading_process = 0;
                DeviceBehavior_Shooter_Try_Reload(false);

                if (current_ammo == fire_logic_data.capacity)
                    FSM_change_to(Device_FSM_Shooter.idle);
            }
        }


        protected override AnimEvent shoot_event()
        {
            return new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = fire_logic_data.tick_percent,
                anim_event = (Device d) =>
                {
                    single_shoot(fire_logic_data, ammo_velocity_mod);
                    if (barrel_bullet_stage != Barrel_Ammo_Stage.not_necessarily)
                        Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage.shell_remaining);
                    else
                        current_ammo -= Mathf.Min(current_ammo, Mathf.Max(shoot_salvo + BattleContext.instance.ranged_salvo, 1));

                    shoot_stage = Shoot_Stage.just_fired; //刚刚射击阶段
                    shoot_finished_action?.Invoke();
                }
            };
        }

        protected override void single_shoot(fire_logic record, float ammo_velocity_mod)
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);

            var salvo = Mathf.Max(shoot_salvo + BattleContext.instance.ranged_salvo, 1);

            salvo = Mathf.Min(salvo, current_ammo); //获取还剩余的子弹数

            DevicePlayAudio(record.SE_fire);

            BattleContext.instance.fire_event?.Invoke(key_points[record.bone_name], bones_direction[BONE_FOR_ROTATE]);

            for (int i = 0; i < salvo; i++)
            {
                var angle = 2 * record.angle;
                var ave_a = angle / salvo;
                var angle_1 = -record.angle + (salvo - i - 1) * ave_a;
                var angle_2 = record.angle - i * ave_a;

                float temp_speed;
                float init_speed;
                if (record.speed.Item2 == 0)
                {
                    temp_speed = record.speed.Item1 * ammo_velocity_mod;
                    init_speed = record.speed.Item1;
                }
                else
                {
                    temp_speed = UnityEngine.Random.Range(record.speed.Item1, record.speed.Item2) * ammo_velocity_mod;
                    init_speed = (record.speed.Item1 + record.speed.Item2) * 0.5f;
                }

                var plt = record.projectile_life_ticks.Item2 == 0 ? record.projectile_life_ticks.Item1 : UnityEngine.Random.Range(record.projectile_life_ticks.Item1, record.projectile_life_ticks.Item2);

                projectiles.TryGetValue(record.projectile_id.ToString(), out var projectile_record);
                var p = BattleUtility.GetProjectile(projectile_record.ammo_type);
                float rot_speed = projectile_record.inertia_moment > 0 ? projectile_record.mass * init_speed / projectile_record.inertia_moment : 0;
                rot_speed *= UnityEngine.Random.Range(-1f, 1f);
                var shoot_bone_dir = bones_direction[BONE_FOR_ROTATE];
                var shoot_dir = Quaternion.AngleAxis((shoot_bone_dir.x >= 0 ? 1 : -1) * shoot_deg_offset, Vector3.forward) * shoot_bone_dir;

                var attack_datas = ExecDmg(null, record.damage);

                for (int a_i = 0; a_i < attack_datas.Count; a_i++)
                {
                    attack_datas[a_i].calc_device_coef(this);
                }

                p.Init(projectile_record, shoot_dir, key_points[record.bone_name].position, (this as ITarget).velocity, angle_1, angle_2, temp_speed, init_speed, faction, plt, this, attack_datas, rot_speed, projectile_hit_callback);
                pmgr.AddProjectile(p);
            }
        }
    }
}
