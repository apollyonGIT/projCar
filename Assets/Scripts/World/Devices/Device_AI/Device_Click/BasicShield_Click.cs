using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.Helpers;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class BasicShield_Click : Device, IShield, IAttack_Device
    {

        #region Data

        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_BLOCK_ATK = "attack_1";
        private const string ANIM_CHARGING_FINISHED = "ready";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_FOR_ATK = "collider_1";
        private const float PROJECTILE_REFLECT_COEF = 1.1F;

        private const float ACCELERATION_COEF = 0.05F;
        private const float ROTATE_DECAY_COEF = 0.9995F;
        private const float ROTATE_DECAY_COEF_WHILE_BROKEN = 0.85F;
        #endregion

        //-------------------------------------------------------------------------------------------------------------

        public Device_Attachment_Ratchet Ratchet_Shield_Rotation_Crank = new(-90);

        //-------------------------------------------------------------------------------------------------------------

        protected enum Device_FSM_Shield
        {
            idle,
            blocking,
            broken,
        }

        //-------------------------------------------------------------------------------------------------------------

        private Device_FSM_Shield fsm;

        #region shield_move
        private float move_speed;
        private Vector2 position_last_tick = Vector2.zero;
        private Vector2 expt_position;

        private float _angular_speed;
        #endregion

        #region shield_block
        private Dictionary<string,int> atk_dmg;
        private float atk_ft;
        private string SE_block;
        #endregion

        #region 实现 IShield
        public float ShieldEnergy_Deduct_By_Blocking { get; set; }
        public int Shield_Blocking_Interval_Current { get; set; }
        public int Shield_Blocking_Interval_Max { get; set; }
        public float Def_Range { get; set; }
        public Vector2 Def_Dir { get; set; }
        public Vector2 Shield_Dir => bones_direction[BONE_FOR_ROTATE];
        #endregion

        #region 实现 IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;

        #endregion

        #endregion

        //=============================================================================================================

        #region Init & UpdateData

        public override void InitData(device_all rc)
        {
            shield_logics.TryGetValue(rc.shield_logic.ToString(), out var record);

            Def_Range = record.def_range;

            atk_dmg = record.damage;
            atk_ft = record.knockback_ft;
            Shield_Blocking_Interval_Max = record.cd;

            move_speed = record.atk_part_speed;

            ShieldEnergy_Deduct_By_Blocking = record.energy_reduce_by_block;

            SE_block = record.SE_block;

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

        //-------------------------------------------------------------------------------------------------------------

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
            Def_Dir = bones_direction[BONE_FOR_ROTATE];
        }

        #endregion

        //=============================================================================================================

        #region tick() & FSM_change_to()

        public override void tick()
        {
            if (WorldContext.instance.is_need_reset)
                position_last_tick -= new Vector2(WorldContext.instance.reset_dis, 0);

            if (!is_validate && fsm != Device_FSM_Shield.broken)
                FSM_change_to(Device_FSM_Shield.broken);

            if (state != DeviceState.stupor)
            {
                switch (fsm)
                {
                    case Device_FSM_Shield.idle:
                        DeviceBehavior_Cooled_Down();
                        DeviceBehavior_Rotate_Around_Center(get_default_dir(), ROTATE_DECAY_COEF);
                        break;

                    case Device_FSM_Shield.blocking:
                        DeviceBehavior_Cooled_Down();
                        DeviceBehavior_Rotate_Around_Center(Def_Dir, ROTATE_DECAY_COEF);
                        break;

                    case Device_FSM_Shield.broken:
                        DeviceBehavior_Rotate_Around_Center(get_default_dir(), ROTATE_DECAY_COEF_WHILE_BROKEN);
                        if (is_validate)
                            FSM_change_to(Device_FSM_Shield.idle);
                        break;
                    default:
                        break;
                }
            }
            // expt_position += WorldContext.instance.caravan_velocity * Config.PHYSICS_TICK_DELTA_TIME;

            base.tick();
        }

        private void FSM_change_to(Device_FSM_Shield target_fsm)
        {
            fsm = target_fsm;
            switch (fsm)
            {
                case Device_FSM_Shield.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    CloseCollider(COLLIDER_FOR_ATK);
                    break;
                case Device_FSM_Shield.blocking:
                    OpenCollider(COLLIDER_FOR_ATK, collider_hit_target);
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_FSM_Shield.broken:
                    ChangeAnim(ANIM_BROKEN, false);
                    CloseCollider(COLLIDER_FOR_ATK);
                    break;
                default:
                    break;
            }
        }

        #endregion

        //=============================================================================================================

        #region Unsorted Fundamental Funcs

        /// <summary>
        /// 获取在Idle状态下的默认方向
        /// </summary>
        /// <returns></returns>
        protected Vector2 get_default_dir()
        {
            var slot = Device_Slot_Helper.GetSlot(this);

            switch (slot)
            {
                case "slot_top":
                    return Vector2.up;
                case "slot_back_top":
                    return new Vector2(-1, 1);
                case "slot_back":
                    return Vector2.left;
                case "slot_front_top":
                    return new Vector2(1, 1);
                case "slot_front":
                default:
                    return Vector2.right;
            }
        }

        private void collider_hit_target(ITarget t)
        {
            if (!can_block())
                return;

            DeviceBehavior_Defend(true);

            var attack_datas = ExecDmg(t, atk_dmg);
            foreach(var attack_data in attack_datas)
            {
                attack_data.calc_device_coef(this); //如果需要计算设备的参数修正，就调用一下这个函数

                t.hurt(this, attack_data, out var dmg_data);
                BattleContext.instance.ChangeDmg(this, dmg_data.dmg);
                t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
            }

            float final_atk_ft = atk_ft;

            if (t.hp <= 0)
                kill_enemy_action?.Invoke(t);

            t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, Shield_Dir, final_atk_ft);
        }

        #endregion

        //=============================================================================================================

        #region DeviceBehavior(Cooled Down, Rorate Around, Defend)

        private void DeviceBehavior_Cooled_Down()
        {
            if (Shield_Blocking_Interval_Current <= 0)
                ChangeAnim(ANIM_CHARGING_FINISHED, false);
            else
                Shield_Blocking_Interval_Current--;
        }

        /// <summary>
        /// 需要保证在每个状态下都被调用，否则move_position会被重置到position.
        /// 同时每个状态下每帧都需要给定当前帧的 expt_position
        /// </summary>
        private void DeviceBehavior_Rotate_Around_Center(Vector2 expected_dir, float shut_down_coef)
        {
            bones_direction[BONE_FOR_ROTATE] = expected_dir;
            _angular_speed *= shut_down_coef;

            Def_Dir = Quaternion.AngleAxis(_angular_speed, Vector3.forward) * Def_Dir;

            if (fsm == Device_FSM_Shield.blocking)
                expt_position = position + Def_Dir * Def_Range;
            else
                expt_position = position;

            var distance = expt_position - position_last_tick;
            var move_distance_per_tick = move_speed * Config.PHYSICS_TICK_DELTA_TIME;
            if (distance.magnitude > move_distance_per_tick)
                expt_position = position_last_tick + distance.normalized * move_distance_per_tick;
            position_last_tick = expt_position;
            position = expt_position;
        }

        /// <summary>
        /// 与敌人或与敌人的飞射物触发碰撞
        /// </summary>
        private void DeviceBehavior_Defend(bool reset_interval)
        {
            ChangeAnim(ANIM_BLOCK_ATK, false);
            if (reset_interval)
                Shield_Blocking_Interval_Current = Shield_Blocking_Interval_Max;
            Audio.AudioSystem.instance.PlayOneShot(SE_block);
        }

        #endregion

        //=============================================================================================================

        bool IShield.Try_Rebound_Projectile(Projectile proj, Vector2 proj_vel)
        {
            if (!can_block())
                return false;

            var v_new = Vector2.Reflect(proj_vel - velocity, -Shield_Dir) * PROJECTILE_REFLECT_COEF + velocity;
            proj.ResetProjectile(v_new, Shield_Dir, faction, MovementStatus.normal);
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.rebound_event?.Invoke(proj);

            DeviceBehavior_Defend(false);

            return true;
        }

        //=============================================================================================================

        #region Condition: Can Block

        protected bool can_block()
        {
            return can_block_check_fsm()
                && can_block_check_cd();
        }

        protected bool can_block_check_fsm()
        {
            return fsm == Device_FSM_Shield.blocking;
        }

        protected bool can_block_check_cd()
        {
            return Shield_Blocking_Interval_Current <= 0;
        }

        #endregion

        //=============================================================================================================

        #region UI Info & UI Controlled Actions

        public void UI_Controlled_Start_Def()
        {
            if(state!= DeviceState.stupor)
                FSM_change_to(Device_FSM_Shield.blocking);
        }

        public void UI_Controlled_Accelerate_Rotation(float input)
        {
            if (fsm == Device_FSM_Shield.blocking)
                _angular_speed += input * ACCELERATION_COEF;
        }

        public void UI_Controlled_Reset_Block_CD()
        {
            Shield_Blocking_Interval_Current = 0;
        }

        #endregion

    }
}
