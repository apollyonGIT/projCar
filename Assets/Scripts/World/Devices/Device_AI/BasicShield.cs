using AutoCodes;
using Commons;
using Foundations;
using UnityEngine;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class BasicShield : Device, IShield, IAttack_Device,IEnergy
    {

        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_BLOCK_ATK = "attack_1";
        private const string ANIM_CHARGING_FINISHED = "ready";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_FOR_ATK = "collider_1";
        private const float PROJECTILE_REFLECT_COEF = 1.1F;
        #endregion

        #region IShield
        public float ShieldEnergy_Deduct_By_Blocking { get; set; }
        public int Shield_Blocking_Interval_Current { get; set; }
        public int Shield_Blocking_Interval_Max { get; set; }
        public float Def_Range { get; set; }
        public Vector2 Def_Dir { get; set; }
        public Vector2 Shield_Dir => bones_direction[BONE_FOR_ROTATE];
        #endregion

        protected enum Device_FSM_Shield
        {
            idle,
            blocking,
            broken,
        }
        private Device_FSM_Shield fsm;


        #region shield_block
        private int atk_dmg;
        private float atk_ft;
        private bool shield_can_block_projectile => fsm != Device_FSM_Shield.broken;
        private bool shield_can_block_enemy => fsm == Device_FSM_Shield.blocking && Shield_Blocking_Interval_Current <= 0;
        private string SE_block;
        #endregion

        #region shield_rotate
        private float get_rotate_speed => desc.rotate_speed.Item1;
        private Vector2 get_default_dir => position - Get_Caravan_Body_Pos();

        #endregion

        #region shield_move
        private float move_speed;

        public Vector2 position_last_tick = Vector2.zero;
        private Vector2 expt_position;
        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;

        #endregion

        #region IEnergy
        public float Default_Energy { get ; set ; }
        public float Current_Energy { get; set; }
        public float Max_Energy { get; set; }
        public float Energy_Recover { get; set; }
        public float Energy_Recover_Factor { get; set; }
        public int Energy_FreezeTick_Current { get; set; }
        public int Energy_FreezeTick_Max { get; set; }
        
        
        #endregion

        //============================================================================================================

        public override void InitData(device_all rc)
        {

            shield_logics.TryGetValue(rc.shield_logic.ToString(), out var record);

            Def_Range = record.def_range;

            atk_ft = record.knockback_ft;
            Shield_Blocking_Interval_Max = record.cd;

            move_speed = record.atk_part_speed;

            Default_Energy = record.energy_max;
            Current_Energy = record.energy_max;
            Max_Energy = record.energy_max;
            Energy_Recover= record.energy_recover.Item1;
            Energy_FreezeTick_Max = record.energy_freezetick;
            ShieldEnergy_Deduct_By_Blocking = record.energy_reduce_by_block;

            SE_block = record.SE_block;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, get_default_dir);

            base.InitData(rc);

            #region AnimEvent
            var block_atk_anim_back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_BLOCK_ATK,
                percent = 1f,
                anim_event = (Device d) => ChangeAnim(ANIM_IDLE, true)
            };

            var charge_ready_anim_back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_CHARGING_FINISHED,
                percent = 1f,
                anim_event = (Device d) => ChangeAnim(ANIM_IDLE, true)
            };
            #endregion

            anim_events.Add(block_atk_anim_back_to_idle);
            anim_events.Add(charge_ready_anim_back_to_idle);
        }

        // =========================================================================================================

        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_FSM_Shield.idle);
        }

        public override void InitPos()
        {
            position_last_tick = position;
            expt_position = position;

            base.InitPos();
        }

        // ------------------------------------------------------------------------------------------------------------

        public override void tick()
        {
            if (WorldContext.instance.is_need_reset)
            {
                position_last_tick -= new Vector2(WorldContext.instance.reset_dis, 0);
            }

            if (!is_validate && fsm != Device_FSM_Shield.broken)
                FSM_change_to(Device_FSM_Shield.broken);

            switch (fsm)
            {
                case Device_FSM_Shield.idle:
                    expt_position = position;
                    shield_device_rotate_to(Get_Default_Dir());

                    pertick_check_and_try_recover(Energy_Recover);
                    pertick_check_and_try_reload();
                    break;

                case Device_FSM_Shield.blocking:
                    expt_position = Get_Caravan_Body_Pos() + Def_Dir * Def_Range;
                    shield_device_rotate_to(Def_Dir);

                    pertick_check_and_try_recover(Energy_Recover);
                    pertick_check_and_try_reload();

                    if (Current_Energy <= 0)
                    {
                        FSM_change_to(Device_FSM_Shield.idle);      //没能量就休息
                        Energy_FreezeTick_Current = Energy_FreezeTick_Max;
                    }
                    break;

                case Device_FSM_Shield.broken:
                    shield_device_rotate_to(get_default_dir);
                    expt_position = position;
                    if (is_validate)
                        FSM_change_to(Device_FSM_Shield.idle);
                    break;
                default:
                    break;
            }

            // expt_position += WorldContext.instance.caravan_velocity * Config.PHYSICS_TICK_DELTA_TIME;
            shield_device_move_to_expected_pos();

            base.tick();
        }

        private void FSM_change_to(Device_FSM_Shield target_fsm)
        {
            fsm = target_fsm;
            switch (fsm)
            {
                case Device_FSM_Shield.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    rotate_speed = get_rotate_speed;
                    CloseCollider(COLLIDER_FOR_ATK);
                    break;
                case Device_FSM_Shield.blocking:
                    OpenCollider(COLLIDER_FOR_ATK, (ITarget t) =>
                    {
                        if (!shield_can_block_enemy)
                            return;

                        trigger_defend(true);

                        float final_atk_ft = atk_ft;

                        shield_logics.TryGetValue(desc.shield_logic.ToString(), out var logic);
                        var attack_datas = ExecDmg(t, logic.damage);

                        foreach(var attack_data in attack_datas)
                        {
                            attack_data.calc_device_coef(this); //如果需要计算设备的参数修正，就调用一下这个函数

                            t.hurt(this, attack_data, out var dmg_data);
                            BattleContext.instance.ChangeDmg(this, dmg_data.dmg);
                            t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
                        }

                        if (t.hp <= 0)
                            kill_enemy_action?.Invoke(t);

                        t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, Shield_Dir, final_atk_ft);
                    });

                    ChangeAnim(ANIM_IDLE, true);
                    rotate_speed = get_rotate_speed;
                    break;
                case Device_FSM_Shield.broken:
                    ChangeAnim(ANIM_BROKEN, false);
                    rotate_speed = 0;
                    CloseCollider(COLLIDER_FOR_ATK);
                    break;
                default:
                    break;
            }
        }

        // ------------------------------------------------------------------------------------------------------------
        private void shield_device_rotate_to(Vector2 expected_dir)
        {
            rotate_bone_to_dir(BONE_FOR_ROTATE, expected_dir);
        }

        /// <summary>
        /// 需要保证在每个状态下都被调用，否则move_position会被重置到position.
        /// 同时每个状态下每帧都需要给定当前帧的 expt_position
        /// </summary>
        private void shield_device_move_to_expected_pos()
        {

            var distance = expt_position - position_last_tick;
            var move_distance_per_tick = move_speed * Config.PHYSICS_TICK_DELTA_TIME;
            if (distance.magnitude > move_distance_per_tick)
                expt_position = position_last_tick + distance.normalized * move_distance_per_tick;
            position_last_tick = expt_position;
        }

        /// <summary>
        /// 车体坐标应有一向上方的偏移。这一数据会考虑在后期配置在caravan表中。
        /// </summary>
        /// <returns></returns>
        private Vector2 Get_Caravan_Body_Pos()
        {
            return WorldContext.instance.caravan_pos + Vector2.up;
        }

        /// <summary>
        /// 获取在Idle状态下的默认方向
        /// </summary>
        /// <returns></returns>
        private Vector2 Get_Default_Dir()
        {
            return position - Get_Caravan_Body_Pos();
        }


        // --------------------------------------------------------------------------------------------------

        private void pertick_check_and_try_reload()
        {
            if (Shield_Blocking_Interval_Current > 0)
                if (--Shield_Blocking_Interval_Current <= 0)
                    ChangeAnim(ANIM_CHARGING_FINISHED, false);
        }

        private void pertick_check_and_try_recover(float recover_value)
        {
            if (Energy_FreezeTick_Current > 0)
                Energy_FreezeTick_Current--;
            else if (Current_Energy < Max_Energy)
            {
                Current_Energy += recover_value;
                if (Current_Energy > Max_Energy)
                    Current_Energy = Max_Energy;
            }
        }

        /// <summary>
        /// 与敌人或与敌人的飞射物触发碰撞
        /// </summary>
        private void trigger_defend(bool reset_interval)
        {
            ChangeAnim(ANIM_BLOCK_ATK, false);
            if (fsm == Device_FSM_Shield.blocking)
                Current_Energy -= ShieldEnergy_Deduct_By_Blocking;
            if (reset_interval)
                Shield_Blocking_Interval_Current = Shield_Blocking_Interval_Max;
            Audio.AudioSystem.instance.PlayOneShot(SE_block);
        }

        // --------------------------------------------------------------------------------------------------

        bool IShield.Try_Rebound_Projectile(Projectile proj, Vector2 proj_vel)
        {
            if (!shield_can_block_projectile)
                return false;

            var v_new = Vector2.Reflect(proj_vel - velocity, -Shield_Dir) * PROJECTILE_REFLECT_COEF + velocity;
            proj.ResetProjectile(v_new, Shield_Dir, faction, MovementStatus.normal);
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.rebound_event?.Invoke(proj);

            trigger_defend(false);

            return true;
        }

        // --------------------------------------------------------------------------------------------------


        public void Def_Start_By_UI_Control(Vector2 target_pos)
        {
            var dir = (target_pos - Get_Caravan_Body_Pos()).normalized;
            shield_device_rotate_to(dir);
            Def_Dir = dir;
            FSM_change_to(Device_FSM_Shield.blocking);
        }

        public void Def_End_By_UI_Control()
        {
            if (fsm == Device_FSM_Shield.blocking)
                FSM_change_to(Device_FSM_Shield.idle);
        }


        #region PlayerControl(To Be Deleted)

        public override void OperateDrag(Vector2 dir)
        {
            Vector2 pos_to_pointer = dir;
            var ptp_n = pos_to_pointer.normalized;
            var ptp_m = pos_to_pointer.magnitude;

            shield_device_rotate_to(ptp_n);

            expt_position = position + ptp_n * Mathf.Clamp(ptp_m - 1f, 0, Def_Range);
        }

        public void TryToAccelerate()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
