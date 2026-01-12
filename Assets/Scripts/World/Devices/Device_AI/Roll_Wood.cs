using AutoCodes;
using Commons;
using Foundations;
using Foundations.Tickers;
using UnityEngine;
using World.Audio;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class Roll_Wood : DScript_Shooter
    {
        /// <summary>
        /// 进阶版设计可能考虑有不同数量的滚木的时候 进入不同的状态机 射击不同的fire_logic
        /// </summary>
        /// <param name="rc"></param>

        const float SAWING_SECOND = 15F;
        public readonly Device_Attachment_Triggerable_Spring TriggerableSpring_Lever_For_Shooting = new();

        public float saw_percent;
        public int WoodCnt;

        public Wood_State wood_state;
        public Saw_State saw_state;

        public enum Wood_State
        {
            standby,
            ready,
            cut,    //过去式 砍过一刀了
        }

        public enum Saw_State
        {
            idle,
            sawing,
        }


        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            saw_state = Saw_State.idle;
            wood_state = Wood_State.ready;
            TriggerableSpring_Lever_For_Shooting.on_spring_triggered = action_on_Spring_Lever_trigger;
        }

        private void action_on_Spring_Lever_trigger()
        {
            Shoot();
        }

        public bool Shoot()
        {
            if (can_manual_shoot())
            {
                DeviceBehavior_Shooter_Try_Shoot(true, false);
                return true;
            }

            return false;
        }

        public void SetWood()
        {
            if (wood_state == Wood_State.standby)
                wood_state = Wood_State.ready;
        }

        public bool CutWood()
        {
            if (wood_state == Wood_State.standby || saw_state == Saw_State.sawing)
            {
                return false;
            }
            saw_state = Saw_State.sawing;
            //播放锯木音效
            //AudioSystem.instance.PlayOneShot("event:/Environment/Sawing_Wood_Loop");

            switch (wood_state)
            {
                case Wood_State.ready:
                    Request req = new Request("cut_wood",
                        (_) => { return saw_percent >= 1; },
                        (_) => { saw_percent = 0; },
                        (_) => { wood_state = Wood_State.cut; saw_state = Saw_State.idle; current_ammo = (int)Mathf.Min(current_ammo + 1, fire_logic_data.capacity); },
                        (_) => { saw_percent += 1f / (SAWING_SECOND * Config.PHYSICS_TICKS_PER_SECOND); }
                        );
                    break;
                case Wood_State.cut:

                    Request req_2 = new Request("cut_wood",
                        (_) => { return saw_percent >= 1; },
                        (_) => { saw_percent = 0; },
                        (_) => { wood_state = Wood_State.standby; saw_state = Saw_State.idle; current_ammo = (int)Mathf.Min(current_ammo + 2, fire_logic_data.capacity); },
                        (_) => { saw_percent += 1f / (SAWING_SECOND * Config.PHYSICS_TICKS_PER_SECOND); }
                        );
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override bool Auto_Reload_Job_Content()
        {
            return CutWood();
        }

        protected override bool Auto_Attack_Job_Content()
        {
            return Shoot();
        }

        protected override void DeviceBehavior_Rotate_To_Target()
        {
            return;
        }
        protected override void single_shoot(fire_logic record, float ammo_velocity_mod)
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);

            var salvo = Mathf.Max(record.salvo + BattleContext.instance.ranged_salvo , 1);

            AudioSystem.instance.PlayOneShot(record.SE_fire);

            for (int i = 0; i < salvo; i++)
            {
                var angle = 2 * record.angle ;
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
                    temp_speed = Random.Range(record.speed.Item1, record.speed.Item2) * ammo_velocity_mod;
                    init_speed = (record.speed.Item1 + record.speed.Item2) * 0.5f;
                }

                var plt = record.projectile_life_ticks.Item2 == 0 ? record.projectile_life_ticks.Item1 : Random.Range(record.projectile_life_ticks.Item1, record.projectile_life_ticks.Item2);

                projectiles.TryGetValue(record.projectile_id.ToString(), out var projectile_record);
                var p = BattleUtility.GetProjectile(projectile_record.ammo_type);
                float rot_speed = projectile_record.inertia_moment > 0 ? projectile_record.mass * init_speed / projectile_record.inertia_moment : 0;
                rot_speed *= Random.Range(-1f, 1f);
                var shoot_bone_dir = bones_direction[BONE_FOR_ROTATE];
               // var shoot_dir = Quaternion.AngleAxis((shoot_bone_dir.x >= 0 ? 1 : -1) * shoot_deg_offset, Vector3.forward) * shoot_bone_dir;
                var shoot_dir = Vector3.left;


                var attack_datas = ExecDmg(null, record.damage);
                p.Init(projectile_record, shoot_dir, key_points[record.bone_name].position, (this as ITarget).velocity, angle_1, angle_2, temp_speed, init_speed, faction, plt, this, attack_datas, rot_speed);
                p.view_prefab_index = current_ammo;
                pmgr.AddProjectile(p);
            }
        }
    }
}
