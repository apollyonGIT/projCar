using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.Audio;
using World.Enemys;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class NewBasicShooter : Device, IAttack, ILoad, IAttack_Device,IShoot
    {
        #region CONST
        protected const string ANIM_IDLE = "idle";
        protected const string ANIM_SHOOT = "shoot";
        protected const string ANIM_BROKEN = "idle";
        protected const float SHOOT_ERROR_DEGREE = 5F;
        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.ranged_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.ranged_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.ranged_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.ranged_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.ranged_critical_dmg_rate;
        #endregion

        protected enum NewDevice_FSM_Shooter
        {
            idle,
            shoot,
            reloading,
            broken,
        }
        protected NewDevice_FSM_Shooter fsm;

        protected bool can_shoot_check_post_cast = true;  //判定射击后摇是否结束；

        protected bool can_shoot_check_ammo => Current_Ammo >= 1;
        protected bool can_shoot_check_cd => Current_Interval <= 0;

        protected bool another_ammo_reloaded = true;

        // Virtual for Derived Mod Shooting Data
        protected virtual float shoot_deg_offset { get; set; } = 0f;  //子弹出射角度相对枪口骨骼的角度偏移
        protected virtual float ammo_velocity_mod { get; set; } = 1f;  //子弹出射速度修正

        // Public for UI Panel
        public bool reloading => fsm == NewDevice_FSM_Shooter.reloading;
        public void UI_Controlled_Reloading() { if (Current_Ammo < Max_Ammo) FSM_change_to(NewDevice_FSM_Shooter.reloading); }
        public virtual void UI_Controlled_Shooting() { if (can_manual_shoot()) FSM_change_to(NewDevice_FSM_Shooter.shoot); }

        #region Load Interface
        public int Default_Ammo { get; set; }
        public int Max_Ammo { get; set; }
        public int Current_Ammo { get; set; }
        public float Reloading_Process { get; set; }
        public float Reload_Speed { get; set; }
        public float Reload_Speed_Factor { get; set; }
        public int Reload_Per_Ammo { get; set; } = 1; 
        #endregion

        #region Attack Interface
        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }
        public float Attack_Interval_Factor { get; set; }
        #endregion

        #region Shooter Interface
        private float proj_spread;
        public float Proj_Spread
        {
            get
            {
                return Mathf.Max(-0.99f, proj_spread);      //保证结果大于0
            }
            set
            {
                proj_spread = value;
            }
        }
        public float Proj_Speed { get; set; }

        private int proj_num;
        public int Proj_Num
        {
            get
            {
                return proj_num;
            }
            set
            {
                proj_num = value;
            } 
        }
        #endregion

        #region SFX
        //private string se_fire;
        protected string se_reloaded;
        #endregion

        //============================================================================================================

        protected void InitBaseData(device_all rc)
        {
            base.InitData(rc);
        }

        protected void BaseTick()
        {
            base.tick();
        }

        public override void UpdateData()
        {
            Max_Ammo = (int)(Default_Ammo * (1 + BattleContext.instance.ranged_capacity / 1000f));

            fire_logics.TryGetValue(desc.fire_logic.ToString(), out var record);
            Attack_Interval = (int)(record.cd * (1 + Attack_Interval_Factor));

            Reload_Speed = record.reload_speed * (1 + Reload_Speed_Factor);

            base.UpdateData();
        }

        public override void InitData(device_all rc)
        {

            fire_logics.TryGetValue(rc.fire_logic.ToString(), out var record);

            Default_Ammo = (int)record.capacity;
            Max_Ammo = (int)(Default_Ammo * (1 + BattleContext.instance.ranged_capacity / 1000f));
            Current_Ammo = Max_Ammo;

            Reload_Speed = record.reload_speed * (1 + Reload_Speed_Factor);

            Attack_Interval = (int)(record.cd * (1 + Attack_Interval_Factor));
            Current_Interval = 0;

            Proj_Spread = 0;
            Proj_Speed = 0;
            Proj_Num = 0;

            se_reloaded = record.SE_reloaded;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, Vector2.right);

            base.InitData(rc);

            #region Animation Event
            var shoot = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.tick_percent,
                anim_event = (Device d) =>
                {
                    single_shoot(record, ammo_velocity_mod);
                    Current_Ammo--;
                }
            };
            var end_post_cast = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.rapid_fire_tick_percent,
                anim_event = (Device d) =>
                {
                    can_shoot_check_post_cast = true;
                    Current_Interval = Attack_Interval;
                }
            };
            var back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = 1f,
                anim_event = (Device d) => FSM_change_to(NewDevice_FSM_Shooter.idle)
            };

            anim_events.Add(shoot);
            anim_events.Add(end_post_cast);
            anim_events.Add(back_to_idle);
            #endregion
        }

        public override void Start()
        {
            base.Start();
            FSM_change_to(NewDevice_FSM_Shooter.idle);
        }

        protected virtual void single_shoot(fire_logic record, float ammo_velocity_mod)
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);

            var salvo = Mathf.Max(record.salvo + BattleContext.instance.ranged_salvo + Proj_Num,1);

            AudioSystem.instance.PlayOneShot(record.SE_fire);

            for (int i = 0; i < salvo; i++)
            {
                var angle = 2 * record.angle *(1 + Proj_Spread);
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

                init_speed *= (1 + Proj_Speed);

                var plt = record.projectile_life_ticks.Item2 == 0 ? record.projectile_life_ticks.Item1 : Random.Range(record.projectile_life_ticks.Item1, record.projectile_life_ticks.Item2);

                projectiles.TryGetValue(record.projectile_id.ToString(), out var projectile_record);
                var p = BattleUtility.GetProjectile(projectile_record.ammo_type);
                float rot_speed = projectile_record.inertia_moment > 0 ? projectile_record.mass * init_speed / projectile_record.inertia_moment : 0;
                rot_speed *= Random.Range(-1f, 1f);
                var shoot_bone_dir = bones_direction[BONE_FOR_ROTATE];
                var shoot_dir = Quaternion.AngleAxis((shoot_bone_dir.x >= 0 ? 1 : -1) * shoot_deg_offset, Vector3.forward) * shoot_bone_dir;



                //不再维护 有事联系 Shooter_Click
                Attack_Data attack_data = new()
                {
                };

                attack_data.calc_device_coef(this);

                List<Attack_Data> attack_datas = new List<Attack_Data>();
                attack_datas.Add(attack_data);  

                p.Init(projectile_record, shoot_dir, key_points[record.bone_name].position, (this as ITarget).velocity, angle_1, angle_2, temp_speed, init_speed, faction, plt, this, attack_datas, rot_speed, projectile_hit_callback);
                pmgr.AddProjectile(p);
            }
        }


        public virtual void projectile_hit_callback(ITarget target)
        {
        }


        public override void tick()
        {
            if (!is_validate && fsm != NewDevice_FSM_Shooter.broken)
                FSM_change_to(NewDevice_FSM_Shooter.broken);

            if (fsm != NewDevice_FSM_Shooter.broken && is_validate && player_oper )
                Aiming();

            switch (fsm)
            {
                case NewDevice_FSM_Shooter.idle:
                    if (Current_Interval > 0)
                        Current_Interval--;

                    if (target_list.Count != 0 && !player_oper)
                    {
                        rotate_bone_to_target(BONE_FOR_ROTATE);
                        if (can_auto_shoot())
                            FSM_change_to(NewDevice_FSM_Shooter.shoot);         //有目标就切换到射击模式
                    }
                    break;
                case NewDevice_FSM_Shooter.shoot:
                    if (Current_Interval > 0)
                        Current_Interval--;

                    if(!player_oper)
                        rotate_bone_to_target(BONE_FOR_ROTATE);

                    if (can_auto_shoot())
                        FSM_change_to(NewDevice_FSM_Shooter.shoot);
                    break;
                case NewDevice_FSM_Shooter.reloading:
                    ammo_reloading();
                    if (Current_Ammo < Max_Ammo)
                        break;
                    AudioSystem.instance.PlayOneShot(se_reloaded);
                    FSM_change_to(NewDevice_FSM_Shooter.idle);
                    break;
                case NewDevice_FSM_Shooter.broken:
                    if (is_validate)
                        FSM_change_to(NewDevice_FSM_Shooter.idle);
                    break;
                default:
                    break;
            }

            base.tick();
        }

        protected void FSM_change_to(NewDevice_FSM_Shooter target_fsm)
        {
            fsm = target_fsm;
            switch (fsm)
            {
                case NewDevice_FSM_Shooter.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    rotate_speed = desc.rotate_speed.Item1;
                    break;
                case NewDevice_FSM_Shooter.shoot:
                    can_shoot_check_post_cast = false;
                    ChangeAnim(ANIM_SHOOT, false);
                    rotate_speed = desc.rotate_speed.Item2;
                    break;
                case NewDevice_FSM_Shooter.reloading:
                    ChangeAnim(ANIM_IDLE, true);
                    rotate_speed = desc.rotate_speed.Item1;
                    Reloading_Process = 0;
                    another_ammo_reloaded = true;
                    can_shoot_check_post_cast = true; //reset post_cast check when reloading
                    break;
                case NewDevice_FSM_Shooter.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    rotate_speed = 0;
                    Reloading_Process = 0;
                    another_ammo_reloaded = true;
                    can_shoot_check_post_cast = true;  //reset post_cast check when broken
                    break;
                default:
                    break;
            }
        }
        //------------------------------------------------------------------------------------------------------------
       public virtual void TryToAutoAttack()
        {
            if (player_oper) //有玩家操控时 以玩家操控优先
                return;

            switch (fsm)
            {
                case NewDevice_FSM_Shooter.idle:
                    if (target_list.Count == 0)
                    {
                        try_get_target();
                    }
                    break;
                case NewDevice_FSM_Shooter.shoot:
                    //Current_Interval--;

                    if (target_list.Count == 0)
                    {
                        try_get_target();
                    }

                    /*rotate_bone_to_target(BONE_FOR_ROTATE);

                    if (can_auto_shoot())
                        FSM_change_to(NewDevice_FSM_Shooter.shoot);*/
                    break;

                default:
                    break;
            }
        }

        private int auto_load_delay_tick = Random.Range(10, 50);
        private int auto_load_over_tick;
        void ILoad.TryToAutoLoad()
        {
            if (player_oper) //有玩家操控时 以玩家操控优先
                return;

            if (Current_Ammo <= 0 && fsm == NewDevice_FSM_Shooter.idle)     //没子弹 且射击结束
                FSM_change_to(NewDevice_FSM_Shooter.reloading);

            /*    if (fsm == NewDevice_FSM_Shooter.reloading)
            {
                if (can_manual_reload)
                    auto_load_over_tick++;

                if (auto_load_over_tick > auto_load_delay_tick)
                {
                    manual_reload_success();
                    auto_load_delay_tick = Random.Range(10, 50);
                    auto_load_over_tick = 0;
                }
            }*/
        }
        //------------------------------------------------------------------------------------------------------------
        protected virtual bool can_auto_shoot()
        {
            return can_shoot_check_cd && can_shoot_check_error_angle() && can_shoot_check_ammo && can_shoot_check_post_cast;     
        }
        protected virtual bool can_manual_shoot()
        {
            return can_shoot_check_cd && can_shoot_check_ammo && can_shoot_check_post_cast;
        }
        protected virtual bool can_shoot_check_error_angle()
        {
            if (target_list.Count == 0)
                return false;

            Vector2 target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            var current_v2 = bones_direction[BONE_FOR_ROTATE];
            var delta_deg = Mathf.Abs(Vector2.SignedAngle(current_v2, target_v2));
            return delta_deg <= SHOOT_ERROR_DEGREE;
        }
        protected override bool try_get_target()
        {
            var target = BattleUtility.select_target_in_circle_min_angle(position, bones_direction[BONE_FOR_ROTATE], desc.basic_range.Item2, faction, (ITarget t) =>
            {
                return target_can_be_selected(t) && !target_list.Contains(t) && !outrange_targets.ContainsKey(t);       //目标可选，且不在已选列表中也不在超出射程列表中
            });

            if (target != null)
            {
                if (target is Enemy enemy)
                    enemy.Select(true);
                target_list.Add(target);
            }

            return target != null;
        }
        private void ammo_reloading()
        {
            Reloading_Process += Reload_Speed * (1 + BattleContext.instance.ranged_reloading_speedup / 1000f);

            if (Reloading_Process < 1)
                return;

            Reloading_Process = 0;
            Current_Ammo += Reload_Per_Ammo;
            another_ammo_reloaded = true;
        }

        #region PlayerControl
        public override void StartControl()
        {
            InputController.instance.left_hold_event += TryToFire;
            InputController.instance.left_release_event += ResetFire;

            base.StartControl();
        }

        public override void EndControl()
        {
            InputController.instance.RemoveLeftHold(TryToFire);
            InputController.instance.left_release_event -= ResetFire;

            base.EndControl();
        }

        protected virtual void Aiming()
        {
            var dir = InputController.instance.GetWorldMousePosition() - new Vector3(position.x, position.y, 10);
            rotate_bone_to_dir(BONE_FOR_ROTATE, dir);
        }

        public override void OperateClick()
        {
            if (can_manual_shoot())
            {
                if (fsm == NewDevice_FSM_Shooter.idle || fsm == NewDevice_FSM_Shooter.shoot)
                    FSM_change_to(NewDevice_FSM_Shooter.shoot);
            }
        }

        public override void OperateDrag(Vector2 dir)
        {
            rotate_bone_to_dir(BONE_FOR_ROTATE, -dir);
        }

        private void ResetFire()
        {

        }
        private void TryToFire()
        {
            Fire();
        }
        protected virtual bool Fire()
        {
            if (can_manual_shoot())
                if (fsm == NewDevice_FSM_Shooter.idle || fsm == NewDevice_FSM_Shooter.shoot)
                {
                    FSM_change_to(NewDevice_FSM_Shooter.shoot);
                    return true;
                }
            return false;
        }
        #endregion
    }
}
