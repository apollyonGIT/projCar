using AutoCodes;
using Commons;
using Foundations;
using System;
using UnityEngine;
using World.Cubicles;
using World.Enemys;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class DScript_Shooter : Device, IAttack_Device,IAttack_New,IReload_New
    {
        #region Data

        #region CONST
        protected const string ANIM_IDLE = "idle";
        protected const string ANIM_SHOOT = "shoot";
        protected const string ANIM_RELOAD = "idle";
        protected const string ANIM_BROKEN = "idle";
        protected const float SHOOT_ERROR_DEGREE = 5F;
        #endregion

        //-------------------------------------------------------------------------------------------------------------
        public enum Device_FSM_Shooter
        {
            idle,
            shoot,
            reloading,
            broken,
        }
        public Device_FSM_Shooter fsm { get; protected set; }

        public bool activated;        //

        //-------------------------------------------------------------------------------------------------------------

        protected enum Shoot_Stage
        {
            fire_prepare,   //准备开火，但并没有开火
            just_fired,   // 刚创建飞射物
            after_post_cast,  //射击后摇
            fire_end,  //未开始射击
        }

        protected Shoot_Stage shoot_stage; //射击阶段

        protected enum Barrel_Ammo_Stage
        {
            not_necessarily, //枪膛内不需要子弹
            empty,  //枪膛内没有子弹
            loaded_into_barrel, //枪膛内有子弹
            shell_remaining, //枪膛内有弹壳
        }
        protected Barrel_Ammo_Stage barrel_bullet_stage = Barrel_Ammo_Stage.not_necessarily; //枪膛内子弹状态

        protected Action shoot_finished_action;

        //-------------------------------------------------------------------------------------------------------------

        // Virtual for Derived Mod Shooting Data
        /// <summary>
        /// 子弹出射角度相对枪口骨骼的角度偏移
        /// </summary>
        protected virtual float shoot_deg_offset { get { return 0f; } }

        /// <summary>
        /// 子弹出射速度修正
        /// </summary>
        protected virtual float ammo_velocity_mod { get; set; } = 1f;
        protected virtual int shoot_salvo { get; set; }

        //-------------------------------------------------------------------------------------------------------------

        private DeviceAttackCubicle cubicle_attack;

        //-------------------------------------------------------------------------------------------------------------

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.ranged_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.ranged_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.ranged_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.ranged_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.ranged_critical_dmg_rate;
        #endregion

        #region SFX
        //private string se_fire;
        protected string se_reloaded;
        #endregion

        #endregion

        //=============================================================================================================

        #region DeviceData

        // 弹匣相关
        public int current_ammo;
        public int max_ammo;
        public float self_reloading_process;


        //射击相关
        public int current_shoot_cd;

        //装填启动进度
        public float reload_start_process;


        //工作进度相关
        public float shoot_job_process;         //这个进度为100%时会进行一次射击动作 
        public float reload_job_process;

        public fire_logic fire_logic_data;
        #endregion


        #region Init & UpdateData

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

            fire_logics.TryGetValue(desc.fire_logic.ToString(), out fire_logic_data);

            base.UpdateData();
        }

        //-------------------------------------------------------------------------------------------------------------

        public override void InitData(device_all rc)
        {
            fire_logics.TryGetValue(rc.fire_logic.ToString(), out fire_logic_data);

            shoot_salvo = (int)fire_logic_data.salvo;

            barrel_bullet_stage = fire_logic_data.need_loaded_in_barrel_before_shoot ? Barrel_Ammo_Stage.empty : Barrel_Ammo_Stage.not_necessarily;

            se_reloaded = fire_logic_data.SE_reloaded;

            current_ammo = (int)fire_logic_data.capacity;

            reload_start_process = 0;
            reload_job_process = 0;
            shoot_job_process = 0;
            current_shoot_cd = 0;

            base.InitData(rc);
            Init_Anim_Event_List(fire_logic_data);

            if (cubicle_list != null)
                foreach (var cubicle in cubicle_list)
                    if (cubicle is DeviceAttackCubicle)
                        cubicle_attack = cubicle as DeviceAttackCubicle;
        }


        protected virtual AnimEvent shoot_event()
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
                        current_ammo--;
                    shoot_stage = Shoot_Stage.just_fired; //刚刚射击阶段
                    shoot_finished_action?.Invoke();
                }
            };
        }
            
        protected virtual void Init_Anim_Event_List(fire_logic record)
        {
            var shoot = shoot_event();

            var end_post_cast = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.rapid_fire_tick_percent,
                anim_event = (Device d) =>
                {
                    shoot_stage = Shoot_Stage.after_post_cast; //射击后摇阶段
                }
            };
            var back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = 1f,
                anim_event = (Device d) => FSM_change_to(Device_FSM_Shooter.idle)
            };

            anim_events.Add(shoot);
            anim_events.Add(end_post_cast);
            anim_events.Add(back_to_idle);
        }

        //-------------------------------------------------------------------------------------------------------------

        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_FSM_Shooter.idle);
        }

        #endregion

        //=============================================================================================================

        #region tick() & FSM_change_to()

        /// <summary>
        /// 派生类尽量不要重写此方法。有新增逻辑优先考虑重写 tick_after_fsm_while_unbroken() 方法。
        /// </summary>
        public override void tick()
        {
            if (!is_validate && fsm != Device_FSM_Shooter.broken)
                FSM_change_to(Device_FSM_Shooter.broken);

            if (state != DeviceState.stupor)
            {
                switch (fsm)
                {
                    case Device_FSM_Shooter.idle:
                        idle_state_tick();
                        break;
                    case Device_FSM_Shooter.shoot:
                        shoot_state_tick();
                        break;
                    case Device_FSM_Shooter.reloading:
                        reloading_state_tick();
                        break;
                    case Device_FSM_Shooter.broken:
                        if (is_validate)
                            FSM_change_to(Device_FSM_Shooter.idle);
                        break;
                    default:
                        break;
                }

                if (fsm != Device_FSM_Shooter.broken)
                    tick_after_fsm_while_unbroken();
            }

            base.tick();
        }

        protected virtual void idle_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();
            current_shoot_cd = Mathf.Max(0, current_shoot_cd - 1);
            reload_start_process = Mathf.Max(0, reload_start_process - fire_logic_data.reload_trigger_speed * 5);
        }


        protected virtual void shoot_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();
            current_shoot_cd = Mathf.Max(0, current_shoot_cd - 1);
            reload_start_process = Mathf.Max(0, reload_start_process - fire_logic_data.reload_trigger_speed * 5);
        }

        protected virtual void reloading_state_tick()
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


        /// <summary>
        /// 仅在 state != stupor 时被调用，且在每个fsm状态的行为之后被调用，且被调用时 fsm != broken。
        /// </summary>
        protected virtual void tick_after_fsm_while_unbroken() { }

        //-------------------------------------------------------------------------------------------------------------

        protected virtual void FSM_change_to(Device_FSM_Shooter target_fsm)
        {
            fsm = target_fsm;
            switch (fsm)
            {
                case Device_FSM_Shooter.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    rotate_speed = desc.rotate_speed.Item1;
                    shoot_stage = Shoot_Stage.fire_end; //未开始射击
                    break;
                case Device_FSM_Shooter.shoot:
                    ChangeAnim(ANIM_SHOOT, false);
                    rotate_speed = desc.rotate_speed.Item2;
                    shoot_stage = Shoot_Stage.fire_prepare; //刚开始射击
                    break;
                case Device_FSM_Shooter.reloading:
                    ChangeAnim(ANIM_RELOAD, true);
                    rotate_speed = desc.rotate_speed.Item1;
                    shoot_stage = Shoot_Stage.fire_end; //未开始射击
                    break;
                case Device_FSM_Shooter.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    rotate_speed = 0;
                    shoot_stage = Shoot_Stage.fire_end; //未开始射击
                    break;
                default:
                    break;
            }
        }

        #endregion

        // ============================================================================================================

        #region Unsorted Fundamental Funcs

        public virtual void EnforcedShoot()
        {
            fire_logics.TryGetValue(desc.fire_logic.ToString(), out var logic);
            single_shoot(logic, ammo_velocity_mod);
        }

        protected override bool try_get_target()
        {
            if ((desc.target_counts != 0 && (target_list.Count + outrange_targets.Count) >= desc.target_counts) || desc.target_counts == 0)
                return false;

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

        //-------------------------------------------------------------------------------------------------------------

        protected virtual void single_shoot(fire_logic record, float ammo_velocity_mod)
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);

            var salvo = Mathf.Max(shoot_salvo + BattleContext.instance.ranged_salvo, 1);

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

                for(int a_i = 0; a_i < attack_datas.Count; a_i++)
                {
                    attack_datas[a_i].calc_device_coef(this);
                }

                p.Init(projectile_record, shoot_dir, key_points[record.bone_name].position, (this as ITarget).velocity, angle_1, angle_2, temp_speed, init_speed, faction, plt, this, attack_datas, rot_speed, projectile_hit_callback);
                pmgr.AddProjectile(p);
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        public virtual void projectile_hit_callback(ITarget target)
        {
        }

        #endregion

        // ============================================================================================================

        #region DeviceBehavior: SelecteTarget, Rotate, Shoot, Reload, LoadIntoBarrel

        protected virtual void DeviceBehavior_Select_Target()
        {
            if (target_list.Count == 0)
                try_get_target();
        }

        //-------------------------------------------------------------------------------------------------------------

        protected virtual void DeviceBehavior_Rotate_To_Target()
        {
            if (can_rotate())
                rotate_bone_to_target(BONE_FOR_ROTATE);
        }

        //-------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// 设备的射击函数      玩家手动射击无需确认开火的角度是否正确
        /// </summary>
        /// <param name="manual_shoot"></param>
        /// <param name="ignore_post_cast"></param>
        /// <param name="shoot_finished"></param>
        /// <returns></returns>
        protected virtual bool DeviceBehavior_Shooter_Try_Shoot(bool manual_shoot, bool ignore_post_cast = false, Action shoot_finished = null)
        {
            var can_shoot = manual_shoot ? can_manual_shoot(ignore_post_cast) : can_auto_shoot();
            if (can_shoot)
            {
                current_shoot_cd = fire_logic_data.cd;
                FSM_change_to(Device_FSM_Shooter.shoot);
                shoot_finished_action = shoot_finished;
            }
            return can_shoot;
        }

        protected virtual bool DeviceBehaviour_Shooter_Try_Start_Reloading()
        {
            if(fsm!= Device_FSM_Shooter.reloading && fsm!= Device_FSM_Shooter.broken && current_ammo != fire_logic_data.capacity)
            {
                reload_start_process += (fire_logic_data.reload_trigger_speed + fire_logic_data.reload_trigger_speed * 5);

                if(reload_start_process >= 1)
                {
                    FSM_change_to(Device_FSM_Shooter.reloading);
                    self_reloading_process = 0;
                    return true;
                }
            }

            return false;

        }


        //-------------------------------------------------------------------------------------------------------------

        protected virtual bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading)
        {
            var can_reload = manual_instantly_reloading ? can_manual_reload() : can_auto_reload();
            if (can_reload)
            {
                BattleContext.instance.load_event?.Invoke(this);
                var reload_num = fire_logic_data.reload_num;
                current_ammo = reload_num > 0 ? current_ammo + reload_num : (int)fire_logic_data.capacity;


                current_ammo = Mathf.Min(current_ammo,(int)fire_logic_data.capacity);   
            }
            return can_reload;
        }

        //-------------------------------------------------------------------------------------------------------------

        protected virtual bool DeviceBehavior_Shooter_Try_Load_Bullet_Into_Barrel()
        {
            if (!can_load_bullet_into_barrel())
                return false;

            if (can_successfully_load_bullet_into_barrel())
            {
                Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage.loaded_into_barrel);
                return true;
            }
            Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage.empty);
            return false;
        }

        protected virtual bool can_successfully_load_bullet_into_barrel()
        {
            return current_ammo > 0;
        }

        protected virtual void Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage target_stage)
        {
            barrel_bullet_stage = target_stage;

            if (target_stage == Barrel_Ammo_Stage.loaded_into_barrel)
                current_ammo--;
        }

        #endregion

        // ============================================================================================================

        #region NPC Auto Work: AutoReload & AutoAttack

        public virtual void TryToAutoReload()
        {
            if (current_ammo != 0)
                return;

            if ( reload_job_process < 1)
            {
                reload_job_process += fire_logic_data.job_speed[0];
                return;
            }

            if (Auto_Reload_Job_Content())
                reload_job_process = 0;
        }

        /// <summary>
        /// 设备上弹的具体逻辑，具体到各个设备上，自动上弹可能并不只包括“上弹”这一个动作，而是还含有“拉栓上膛”、“整理弹匣”等动作，因此需要根据具体情况重载。
        /// </summary>
        /// <returns></returns>
        protected virtual bool Auto_Reload_Job_Content()
        {
            return DeviceBehaviour_Shooter_Try_Start_Reloading();
        }


        //-------------------------------------------------------------------------------------------------------------

        public virtual void TryToAutoAttack()
        {
            if (target_list.Count == 0 || target_in_radius(target_list[0]) == false)
                return;

            if (shoot_job_process <1)
            {
                shoot_job_process +=  fire_logic_data.job_speed[1];
                return;
            }

            if (Auto_Attack_Job_Content())
                shoot_job_process = 0;
        }

        /// <summary>
        /// 设备开火的具体逻辑，具体到各个设备上，自动攻击可能并不只包括“攻击”这一个动作，而是还含有“拉栓上膛”、“整理弹匣”等动作，因此需要根据具体情况重载。
        /// </summary>
        /// <returns></returns>
        protected virtual bool Auto_Attack_Job_Content()
        {
            return DeviceBehavior_Shooter_Try_Shoot(false, false);
        }

        #endregion

        // ============================================================================================================

        #region Condition: Can Rotate

        protected virtual bool can_rotate()
        {
            return can_rotate_check_if_actived() || can_rotate_check_attack_cubicle_occupied();
        }

        //-------------------------------------------------------------------------------------------------------------

        private bool can_rotate_check_if_actived()
        {
            return activated;  //激活时，允许旋转
        }

        private bool can_rotate_check_attack_cubicle_occupied()
        {
            return cubicle_attack.worker != null;
        }

        #endregion

        // ============================================================================================================

        #region Condition: Can Shoot

        protected virtual bool can_auto_shoot()
        {
            // 注：不应在此处添加关于 Current_Interval 的判定 —— 这是NPC自动射击工作的间隔，不是自动射击的间隔。
            return can_shoot_check_ammo()
                && can_shoot_check_fsm()
                && can_shoot_check_error_angle()
                && can_shoot_check_shoot_stage(false)
                && can_shoot_check_cd();
        }
        protected virtual bool can_manual_shoot(bool ignore_post_cast = false)
        {
            return can_shoot_check_ammo()
                && can_shoot_check_fsm()
                && can_shoot_check_shoot_stage(ignore_post_cast)
                && can_shoot_check_cd();
        }

        // ------------------------------------------------------------------------------------------------------------

        protected bool can_shoot_check_cd()
        {
            return  current_shoot_cd <= 0;
        }

        protected virtual bool can_shoot_check_ammo()
        {
            if (barrel_bullet_stage != Barrel_Ammo_Stage.not_necessarily)
                return barrel_bullet_stage == Barrel_Ammo_Stage.loaded_into_barrel;
            return current_ammo >= 1;
        }

        protected virtual bool can_shoot_check_error_angle()
        {
            if (target_list.Count == 0)
                return false;

            Vector2 target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            var current_v2 = bones_direction[BONE_FOR_ROTATE];
            var delta_deg = Vector2.Angle(current_v2, target_v2);
            return delta_deg <= SHOOT_ERROR_DEGREE;
        }

        protected virtual bool can_shoot_check_shoot_stage(bool ignore_post_cast)
        {
            return shoot_stage >= (ignore_post_cast ? Shoot_Stage.just_fired : Shoot_Stage.after_post_cast);
        }

        protected bool can_shoot_check_fsm()
        {
            return fsm == Device_FSM_Shooter.idle || fsm == Device_FSM_Shooter.shoot;
        }

        #endregion

        // ============================================================================================================

        #region Condition: Can Reload

        protected bool can_auto_reload()
        {
            return can_reload_check_ammo()
                && can_reload_check_fsm();
        }

        protected virtual bool can_manual_reload()
        {
            return can_reload_check_ammo()
                && can_reload_check_fsm();
        }

        // ------------------------------------------------------------------------------------------------------------

        protected virtual bool can_reload_check_ammo()
        {
            return current_ammo < fire_logic_data.capacity;
        }

        protected bool can_reload_check_fsm()
        {
            return fsm != Device_FSM_Shooter.broken;
        }

        #endregion

        // ============================================================================================================

        #region Condition: Can Load Bullet Into Barrel

        protected bool can_load_bullet_into_barrel()
        {
            return can_load_bullet_into_barrel_check_barrle_ammo_stage()
                && can_load_bullet_into_barrel_check_fsm();
        }

        // ------------------------------------------------------------------------------------------------------------

        protected virtual bool can_load_bullet_into_barrel_check_barrle_ammo_stage()
        {
            return barrel_bullet_stage != Barrel_Ammo_Stage.not_necessarily;
        }

        protected bool can_load_bullet_into_barrel_check_fsm()
        {
            return fsm != Device_FSM_Shooter.broken;
        }

        #endregion

        // ============================================================================================================



        #region 给外部使用的函数

        /// <summary>
        /// 2=目标已锁定；
        /// 1=有目标但未锁定；
        /// 0=没有目标；
        /// </summary>
        /// <returns></returns>
        public int Get_Targets_Status()
        {
            if (can_shoot_check_error_angle())
                return 2;
            if (target_list.Count > 0)
                return 1;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>0=该武器不需要上膛; 1=未上膛; 2=已上膛 ; 3=已击发，残留弹壳</returns>
        public int Get_Barrel()
        {
            return (int)barrel_bullet_stage;  //返回枪膛内是否有子弹，及其状态
        }

        // ------------------------------------------------------------------------------------------------------------

        public virtual void Try_Reloading()
        {
            if (DeviceBehavior_Shooter_Try_Reload(true))
                DevicePlayAudio(se_reloaded);
        }

        /// <summary>
        /// 默认开火需要考虑枪械射击后摇；可通过参数ignore_post_cast来忽略后摇。
        /// </summary>
        /// <param name="ignore_post_cast">true = 开火无视枪械射击后摇</param>
        public virtual bool Try_Shooting(bool ignore_post_cast = false, Action shoot_finished = null)
        {
            return DeviceBehavior_Shooter_Try_Shoot(true, ignore_post_cast, shoot_finished);
        }

        public virtual bool Try_Start_Reloading()
        {
            return DeviceBehaviour_Shooter_Try_Start_Reloading();
        }   

        public virtual void Load_Bullet_Into_Barrel()
        {
            DeviceBehavior_Shooter_Try_Load_Bullet_Into_Barrel();
        }

        public bool CanShoot()
        {
            return can_manual_shoot();
        }

        public bool CanReload()
        {
            return can_manual_reload();
        }

        #endregion

    }
}
