using AutoCodes;
using System;
using UnityEngine;


namespace World.Devices.Device_AI
{
    public class Shooter_Bow : DScript_Shooter
    {
        #region Const
        private const float SPRING_TRIGGER_VALUE = 0.05F;
        private const float SPRING_UNTRIGGER_VALUE = 0.01F;

        private const int ARROW_PRERELOADING_TICKS = 90; // 预装填时间
        private const int ARROW_RELOADING_TICKS_MAX = 60; // 每根箭的装填时间

        private const int STORED_ARROW_MAX = 3; // 最大储存箭数
        #endregion

        // -------------------------------------------------------------------------------

        public Device_Attachment_Triggerable_Spring TriggerableSpring_for_Shooting = new(SPRING_TRIGGER_VALUE, SPRING_UNTRIGGER_VALUE, null, null);
        public Device_Attachment_Holding_Timer HoldingTimer_reloader;  // 在Init中构造

        // -------------------------------------------------------------------------------

        private int reloading_interval_ticks;
        private int reloading_prereloading_ticks;

        private int _stored_arrow;
        private bool _is_reloading;
        private bool _ui_just_shoot = false; // UI面板上是否刚刚射击
        private float _npc_shoot_spring_added_per_tick = 0.01F; // NPC拉弓时每帧增加的拉力值
        private int _npc_shoot_spring_max = 65; // NPC拉弓时最大拉弓时长
        private int _npc_shoot_spring_current = 0; // NPC拉弓时的计时器

        // ===============================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            HoldingTimer_reloader = new(shooter_bow_start_reloading);
        }

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();

            if (barrel_bullet_stage == Barrel_Ammo_Stage.empty && _stored_arrow > 0)
            {
                _stored_arrow--;
                barrel_bullet_stage = Barrel_Ammo_Stage.loaded_into_barrel;
            }

            HoldingTimer_reloader.Tick_Update(!_is_reloading);

            if (_is_reloading)
            {
                if (current_ammo >= fire_logic_data.capacity)
                {
                    _is_reloading = false;
                    return;
                }

                if (--reloading_prereloading_ticks > 0)
                    return;

                if (reloading_interval_ticks > 0)
                    reloading_interval_ticks--;

                if (reloading_interval_ticks <= 0)
                {
                    DeviceBehavior_Shooter_Try_Reload(true);
                    reloading_interval_ticks = ARROW_RELOADING_TICKS_MAX;
                }
            }
        }

        // ===============================================================================

        protected override void FSM_change_to(Device_FSM_Shooter target_fsm)
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
                    ChangeAnim_Percent(ANIM_SHOOT, false, 0.8f);
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

        // ===============================================================================

        protected override bool DeviceBehavior_Shooter_Try_Shoot(bool ui_manual_shoot, bool ignore_post_cast = false, Action shoot_finished = null)
        {
            var can_shoot = ui_manual_shoot ? can_manual_shoot(ignore_post_cast) : can_auto_shoot();
            if (can_shoot)
            {
                // Override New Start 
                ammo_velocity_mod = Mathf.Clamp01(TriggerableSpring_for_Shooting.Spring_Value_01 * 2f);
                Just_Shoot = true;
                // Override New End

                FSM_change_to(Device_FSM_Shooter.shoot);
                shoot_finished_action = shoot_finished;
            }
            return can_shoot;
        }

        // ===============================================================================

        protected override bool Auto_Attack_Job_Content()
        {
            if (barrel_bullet_stage != Barrel_Ammo_Stage.loaded_into_barrel)
                return false;

            if (!can_shoot_check_error_angle())
            {
                _npc_shoot_spring_current = 0;
                TriggerableSpring_for_Shooting.Spring_Value_01 = 0f;
                return false;
            }

            if (_npc_shoot_spring_current < _npc_shoot_spring_max)
            {
                _npc_shoot_spring_current++;
                TriggerableSpring_for_Shooting.Spring_Value_01 += _npc_shoot_spring_added_per_tick;
                return false;
            }
            else
            {
                _npc_shoot_spring_current = 0;
                var b = base.Auto_Attack_Job_Content();
                TriggerableSpring_for_Shooting.Spring_Value_01 = 0f;
                return b;
            }
        }

        // -------------------------------------------------------------------------------

        protected override bool Auto_Reload_Job_Content()
        {
            return shooter_bow_add_stored_arrow() ? true : current_ammo == 0 && simulated_holding_to_reload();

            bool simulated_holding_to_reload()
            {
                if (!_is_reloading)
                    UI_Hold_And_Set_Reloading_Trigger();
                return false;
            }
        }

        // ===============================================================================

        private void shooter_bow_start_reloading()
        {
            if (_is_reloading)
                return;

            current_ammo = 0;
            _is_reloading = true;
            reloading_interval_ticks = ARROW_RELOADING_TICKS_MAX;
            reloading_prereloading_ticks = ARROW_PRERELOADING_TICKS;
        }

        private bool shooter_bow_add_stored_arrow()
        {
            if (!_is_reloading && _stored_arrow < STORED_ARROW_MAX && current_ammo > 0)
            {
                _stored_arrow++;
                current_ammo--;
                return true;
            }
            return false;
        }

        // ===============================================================================

        protected override bool can_load_bullet_into_barrel_check_barrle_ammo_stage()
        {
            return barrel_bullet_stage == Barrel_Ammo_Stage.empty;
        }

        protected override void Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage target_stage)
        {
            if (target_stage == Barrel_Ammo_Stage.shell_remaining)
                target_stage = Barrel_Ammo_Stage.empty;
            base.Barrel_Ammo_Stage_Change(target_stage);
        }

        // ===============================================================================
        // For UI Panel

        public override void Load_Bullet_Into_Barrel()
        {
            shooter_bow_add_stored_arrow();
        }

        // -------------------------------------------------------------------------------

        public void UI_Hold_And_Set_Reloading_Trigger()
        {
            HoldingTimer_reloader.Set_Is_Holding();
        }

        public void Shoot_On_Release_Bow_String()
        {
            if (TriggerableSpring_for_Shooting.Triggered)
                DeviceBehavior_Shooter_Try_Shoot(true);
        }

        public bool Just_Shoot
        {
            get { return _ui_just_shoot; }
            set { _ui_just_shoot = value; }
        }

        public float Reloading_Prereoading_Ticks_T
        { get { return Mathf.InverseLerp(0, ARROW_PRERELOADING_TICKS, reloading_prereloading_ticks); } }

        public bool Is_Reloading
        { get { return _is_reloading; } }

        public int Stored_Arrow
        { get { return _stored_arrow; } }

    }
}