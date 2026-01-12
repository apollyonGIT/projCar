using AutoCodes;
using System.Collections.Generic;
using UnityEngine;


namespace World.Devices.Device_AI
{
    public class Shooter_Trebuchet : DScript_Shooter
    {
        #region Const
        new private const float SHOOT_ERROR_DEGREE = 15F;

        private const float UI_LOADED_INDICATOR_LOADED = 0F;
        private const float UI_LOADED_INDICATOR_UNLOADED = -90F;

        private const float RELOADING_LEVER_TRIGGER_THRESHOLD = 0.2F; // 装填：拉动扳机的阈值
        private const float RELOADING_LEVER_UNTRIGGER_THRESHOLD = 0.015625F; // 装填：松开扳机的阈值

        private const float RELOAD_SPRING_THRESHOLD = 0.4F; // 装填阈值，弹力到达这一数值后自动装填一发弩箭

        private const float DIR_BORDER_TIER_1 = 40F;
        private const float DIR_BORDER_TIER_2 = 76F;

        private const float DIR_BORDER_1 = -DIR_BORDER_TIER_2;
        private const float DIR_BORDER_2 = -DIR_BORDER_TIER_1;
        private const float DIR_BORDER_3 = 0;
        private const float DIR_BORDER_4 = DIR_BORDER_TIER_1;
        private const float DIR_BORDER_5 = DIR_BORDER_TIER_2;

        private readonly List<float> DIR_BORDERS = new() { DIR_BORDER_1, DIR_BORDER_2, DIR_BORDER_3, DIR_BORDER_4, DIR_BORDER_5 };

        private const float SHOOT_DEG_OFFSET_LEFT_LOW = 90F + (DIR_BORDER_TIER_1 + DIR_BORDER_TIER_2) * 0.5F;
        private const float SHOOT_DEG_OFFSET_LEFT_HIGH = 90F + DIR_BORDER_TIER_1 * 0.5F;
        private const float SHOOT_DEG_OFFSET_RIGHT_HIGH = 90F - DIR_BORDER_TIER_1 * 0.5F;
        private const float SHOOT_DEG_OFFSET_RIGHT_LOW = 90F - (DIR_BORDER_TIER_1 + DIR_BORDER_TIER_2) * 0.5F;
        #endregion

        // -------------------------------------------------------------------------------

        public readonly Device_Attachment_Triggerable_Spring TriggerableSpring_For_Reloading = new(RELOADING_LEVER_TRIGGER_THRESHOLD, RELOADING_LEVER_UNTRIGGER_THRESHOLD);
        public readonly Device_Attachment_Dir_Indicator DirIndicator_Deco_Dish_Reloading = new(Vector2.right);
        public readonly Device_Attachment_Triggerable_Spring TriggerableSpring_Lever_For_Shooting = new();

        // -------------------------------------------------------------------------------

        protected override float ammo_velocity_mod { get { return _spring_force_current; } }
        protected override float shoot_deg_offset { get { return Expected_Shoot_Dir_Angle - Vector2.SignedAngle(Vector2.right, bones_direction[BONE_FOR_ROTATE]); } }

        // -------------------------------------------------------------------------------

        private enum Shooter_Trebuchet_Shoot_Dir
        {
            Never_Selected = -1,
            Right_Low = 0,
            Right_High = 1,
            Left_High = 2,
            Left_Low = 3,
        }
        private Shooter_Trebuchet_Shoot_Dir shoot_dir = Shooter_Trebuchet_Shoot_Dir.Never_Selected;

        private float Expected_Shoot_Dir_Angle
        {
            get
            {
                return shoot_dir switch
                {
                    Shooter_Trebuchet_Shoot_Dir.Right_Low => SHOOT_DEG_OFFSET_RIGHT_LOW,
                    Shooter_Trebuchet_Shoot_Dir.Right_High => SHOOT_DEG_OFFSET_RIGHT_HIGH,
                    Shooter_Trebuchet_Shoot_Dir.Left_High => SHOOT_DEG_OFFSET_LEFT_HIGH,
                    Shooter_Trebuchet_Shoot_Dir.Left_Low => SHOOT_DEG_OFFSET_LEFT_LOW,
                    _ => 90F,
                };
            }
        }

        private int auto_reload_lever_reset_tick;

        private float _spring_force_current = 1f; // [0, 1]

        private readonly Device_Attachment_Damping_Rotator DampRotater_ui_indicator_ammo_loaded = new(UI_LOADED_INDICATOR_LOADED);
        private readonly Device_Attachment_Damping_Rotator DampRotater_ui_indicator_shoot_dir = new(90f);

        private Device_Attachment_Windup_Lever Windup_Lever; // 在Init中构造

        // ===============================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            TriggerableSpring_Lever_For_Shooting.on_spring_triggered = action_on_Spring_Lever_trigger;

            Windup_Lever = new(TriggerableSpring_For_Reloading, DirIndicator_Deco_Dish_Reloading);
        }

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();

            Spring_Force_Current += Windup_Lever.Tick_Update(Spring_Force_Current);

            var shoot_dir_compensation = Mathf.Lerp(-0.25f, 0.25f, Mathf.InverseLerp(SHOOT_DEG_OFFSET_RIGHT_LOW, SHOOT_DEG_OFFSET_LEFT_LOW, Expected_Shoot_Dir_Angle));
            var bone_angle = Mathf.Lerp(-0.5f, 0.25f, Spring_Force_Current) + shoot_dir_compensation;
            var bone_dir = new Vector2(Mathf.Cos(bone_angle), Mathf.Sin(bone_angle));
            rotate_bone_to_dir(BONE_FOR_ROTATE, bone_dir);

            DampRotater_ui_indicator_ammo_loaded.Tick_Update_Damping_Rotate();
            DampRotater_ui_indicator_shoot_dir.Tick_Update_Damping_Rotate();

            if (auto_reload_lever_reset_tick > 0 && --auto_reload_lever_reset_tick <= 0)
                TriggerableSpring_For_Reloading.Spring_Value_01 = 0;
        }

        // -------------------------------------------------------------------------------

        // ===============================================================================

        protected override void DeviceBehavior_Rotate_To_Target() { return; }

        // ===============================================================================

        protected override bool Auto_Reload_Job_Content()
        {
            if (Spring_Force_Current >= 1)
            {
                if (TriggerableSpring_For_Reloading.Spring_Value_01 == 1)
                    TriggerableSpring_For_Reloading.Spring_Value_01 = 0;
                return false;
            }

            if (TriggerableSpring_For_Reloading.Spring_Value_01 < 1)
            {
                TriggerableSpring_For_Reloading.Spring_Value_01 = 1;
                auto_reload_lever_reset_tick = 144;
            }

            return true;
        }

        // -------------------------------------------------------------------------------

        protected override bool Auto_Attack_Job_Content()
        {
            if (target_list.Count == 0)
                return false;

            if (can_shoot_check_error_angle())
                return trebuchet_shoot();

            var target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            return trebuchet_set_shoot_dir(target_v2);
        }

        // ===============================================================================

        protected override bool can_shoot_check_error_angle()
        {
            if (target_list.Count == 0)
                return false;

            Vector2 target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            //var current_v2 = bones_direction[BONE_FOR_ROTATE];  
            var current_shoot_rad = Expected_Shoot_Dir_Angle * Mathf.Deg2Rad;
            Vector2 current_v2 = new(Mathf.Cos(current_shoot_rad), Mathf.Sin(current_shoot_rad));
            var delta_deg = Vector2.Angle(current_v2, target_v2);
            return delta_deg <= SHOOT_ERROR_DEGREE;  // 常量 SHOOT_ERROR_DEGREE 数值重写
        }

        // ===============================================================================

        protected override void single_shoot(fire_logic record, float ammo_velocity_mod)
        {
            base.single_shoot(record, ammo_velocity_mod);
            Spring_Force_Current = 0f;
        }

        private bool trebuchet_shoot()
        {
            return DeviceBehavior_Shooter_Try_Shoot(true, false, action_on_shoot);
        }

        private bool trebuchet_set_shoot_dir(Vector2 click_dir)
        {
            if (!Can_Set_Dir_Check_FSM())
                return false;   // shoot 动画开始后就不允许再调整方向了

            var angle = Vector2.SignedAngle(Vector2.up, click_dir);

            for (int i = 0; i < DIR_BORDERS.Count - 1; i++)
            {
                if (angle >= DIR_BORDERS[i] && angle < DIR_BORDERS[i + 1])
                {
                    shoot_dir = (Shooter_Trebuchet_Shoot_Dir)i;
                    DampRotater_ui_indicator_shoot_dir.Set_Expected_Angle(-90f + Expected_Shoot_Dir_Angle);
                    return true;
                }
            }

            return false;
        }

        // ===============================================================================

        private void action_on_Spring_Lever_trigger()
        {
            trebuchet_shoot();
        }

        private void action_on_shoot()
        {
            DampRotater_ui_indicator_ammo_loaded.Set_Expected_Angle(UI_LOADED_INDICATOR_UNLOADED);
            Windup_Lever.Reset_Windup();
        }

        // ===============================================================================

        // For UI Info

        public float Spring_Force_Current
        {
            get { return _spring_force_current; }
            set
            {
                _spring_force_current = Mathf.Clamp01(value);

                if (current_ammo == 0 && Spring_Force_Current > RELOAD_SPRING_THRESHOLD && DeviceBehavior_Shooter_Try_Reload(true))
                    DampRotater_ui_indicator_ammo_loaded.Set_Expected_Angle(UI_LOADED_INDICATOR_LOADED);
            }
        }

        public float UI_Info_Indicator_Ammo_Loaded() { return DampRotater_ui_indicator_ammo_loaded.Dir_Angle; }

        public float UI_Info_Indicator_Shoot_Angle() { return DampRotater_ui_indicator_shoot_dir.Dir_Angle; }

        public bool Can_Set_Dir_Check_FSM() { return fsm == Device_FSM_Shooter.idle; }

        /// <summary>
        /// <para>anticlockwise: 0, 1, 2, 3</para>
        /// <para>unset: -1</para>
        /// </summary>
        /// <returns></returns>
        public int UI_Info_Selected_Dir_Index()
        {
            return (int)shoot_dir;
        }

        // -------------------------------------------------------------------------------

        public void UI_Controlled_Set_Shoot_Dir(Vector2 click_dir)
        {
            trebuchet_set_shoot_dir(click_dir);
        }

    }
}