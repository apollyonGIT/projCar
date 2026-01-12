using AutoCodes;
using UnityEngine;


namespace World.Devices.Device_AI
{
    public class Shooter_Huge_Ballista : DScript_Shooter, IRotate
    {
        #region CONST
        new private const string ANIM_SHOOT = "shoot";
        new private const string ANIM_RELOAD = "ready";

        private const float ROTATE_COEF = 0.15F; // 转向难度修正
        private const float MUZZEL_ANGLE_LIMIT = 100F;

        private const float RELOAD_SPRING_THRESHOLD = 0.4F; // 装填阈值，弹力到达这一数值后自动装填一发弩箭

        private const float RELOADING_LEVER_TRIGGER_THRESHOLD = 0.2F; // 拉动扳机的阈值
        private const float RELOADING_LEVER_UNTRIGGER_THRESHOLD = 0.015625F; // 松开扳机的阈值

        private const float AUTO_WORK_ROTATE_EFFICIENCY = 0.75F;
        #endregion

        // -------------------------------------------------------------------------------

        float IRotate.turn_angle => Dir_Indicator_Muzzel.Dir_Angle;

        public readonly Device_Attachment_Ratchet Ratchet_Muzzel_Rotation_Crank = new(-90);
        public Device_Attachment_Dir_Indicator Dir_Indicator_Muzzel; // 在Init中初始化
        public readonly Device_Attachment_Dir_Indicator Dir_Indicator_Deco_Dish_1 = new(Vector2.right, -1, -6f);
        public readonly Device_Attachment_Dir_Indicator Dir_Indicator_Deco_Dish_2 = new(Vector2.right, -1, 6f);

        public readonly Device_Attachment_Dir_Indicator Dir_Indicator_Deco_Dish_Reloading = new(Vector2.right);
        public readonly Device_Attachment_Triggerable_Spring Triggerable_Spring_For_Reloading = new(RELOADING_LEVER_TRIGGER_THRESHOLD, RELOADING_LEVER_UNTRIGGER_THRESHOLD);

        public readonly Device_Attachment_Triggerable_Spring Triggerable_Spring_Lever_For_Shooting = new();

        // -------------------------------------------------------------------------------

        protected override float ammo_velocity_mod { get { return _spring_force_current; } }

        // -------------------------------------------------------------------------------

        private Device_Attachment_Windup_Lever Windup_Lever; // 在Init中构造

        private int auto_shoot_lever_reset_tick;
        private int auto_reload_lever_reset_tick;

        private float _spring_force_current = 1f; // 当前弹簧力

        // ===============================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            Triggerable_Spring_Lever_For_Shooting.on_spring_triggered = action_on_Spring_Lever_trigger;

            Windup_Lever = new(Triggerable_Spring_For_Reloading, Dir_Indicator_Deco_Dish_Reloading);
        }


        public override void InitPos()
        {
            base.InitPos();
            Dir_Indicator_Muzzel = new(bones_direction[BONE_FOR_ROTATE], MUZZEL_ANGLE_LIMIT, ROTATE_COEF);
        }

        protected override void Init_Anim_Event_List(fire_logic record)
        {
            var shoot = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.tick_percent,
                anim_event = (Device d) =>
                {
                    single_shoot(record, ammo_velocity_mod);
                    if (barrel_bullet_stage != Barrel_Ammo_Stage.not_necessarily)
                        Barrel_Ammo_Stage_Change(Barrel_Ammo_Stage.shell_remaining);
                    else
                        current_ammo--;
                    shoot_stage = Shoot_Stage.just_fired; //刚刚射击阶段
                    shoot_finished_action?.Invoke();
                }
            };
            var end_post_cast = new AnimEvent()
            {
                anim_name = ANIM_SHOOT,
                percent = record.rapid_fire_tick_percent,
                anim_event = (Device d) =>
                {
                    shoot_stage = Shoot_Stage.after_post_cast; //射击后摇阶段
                }
            };
            var to_reload = new AnimEvent()
            {
                anim_name = ANIM_RELOAD,
                percent = 1f,
                anim_event = (Device d) => FSM_change_to(Device_FSM_Shooter.reloading)
            };

            anim_events.Add(shoot);
            anim_events.Add(end_post_cast);
            anim_events.Add(to_reload);
        }


        // ===============================================================================

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();

            Spring_Force_Current += Windup_Lever.Tick_Update(Spring_Force_Current);

            if (auto_shoot_lever_reset_tick > 0 && --auto_shoot_lever_reset_tick <= 0)
                Triggerable_Spring_Lever_For_Shooting.Spring_Value_01 = 0;

            if (auto_reload_lever_reset_tick > 0 && --auto_reload_lever_reset_tick <= 0)
                Triggerable_Spring_For_Reloading.Spring_Value_01 = 0;
        }

        // ===============================================================================

        protected override void DeviceBehavior_Rotate_To_Target() { return; }

        protected override bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading = true)
        {
            if (current_ammo < fire_logic_data.capacity)
                current_ammo = (int)fire_logic_data.capacity;
            FSM_change_to(Device_FSM_Shooter.idle);
            return true;
        }

        protected virtual bool DeviceBehavior_Ballista_Try_Rotate(bool ui_manual, float rotation_input)
        {
            Dir_Indicator_Muzzel.Rotate_Delta_Angle(rotation_input);
            bones_direction[BONE_FOR_ROTATE] = Dir_Indicator_Muzzel.Dir_Vector2;

            Dir_Indicator_Deco_Dish_1.Rotate_Delta_Angle(rotation_input);
            Dir_Indicator_Deco_Dish_2.Rotate_Delta_Angle(rotation_input);

            if (!ui_manual)
                Ratchet_Muzzel_Rotation_Crank.rotate(rotation_input, true);

            return true;
        }

        // ===============================================================================

        #region IRotate Funcs

        public void TryToAutoRotate()
        {
            if (target_list.Count == 0)
                return;

            Vector2 to_target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position).normalized;
            var cross_z = to_target_v2.x * Dir_Indicator_Muzzel.Dir_Vector2.y - to_target_v2.y * Dir_Indicator_Muzzel.Dir_Vector2.x;
            var muzzle_angle = Dir_Indicator_Muzzel.Dir_Angle_With_Center_Axis_As_Up;

            if (cross_z < 0 && (!Dir_Indicator_Muzzel.Has_Angle_Limit || muzzle_angle > -Dir_Indicator_Muzzel.Angle_Limit_PosNeg))
                DeviceBehavior_Ballista_Try_Rotate(false, -AUTO_WORK_ROTATE_EFFICIENCY * cross_z);
            else if (cross_z >= 0 && (!Dir_Indicator_Muzzel.Has_Angle_Limit || muzzle_angle < Dir_Indicator_Muzzel.Angle_Limit_PosNeg))
                DeviceBehavior_Ballista_Try_Rotate(false, -AUTO_WORK_ROTATE_EFFICIENCY * cross_z);
        }

        void IRotate.Rotate(float angle)
        {
            // 没有使用接口提供的 Rotate，而是定义了新的 DeviceBehavior_Ram_Try_Rotate 并使用，
            // 目的：1.统一名称前缀DeviceBehavior 2.可以更灵活地自定义输入参数与返回值
        }

        #endregion

        // -------------------------------------------------------------------------------

        protected override bool Auto_Reload_Job_Content()
        {
            if (Spring_Force_Current >= 1)
            {
                if (Triggerable_Spring_For_Reloading.Spring_Value_01 == 1)
                    Triggerable_Spring_For_Reloading.Spring_Value_01 = 0;
                return false;
            }

            if (Triggerable_Spring_For_Reloading.Spring_Value_01 < 1)
            {
                Triggerable_Spring_For_Reloading.Spring_Value_01 = 1;
                auto_reload_lever_reset_tick = 144;
            }

            return true;
        }

        protected override bool Auto_Attack_Job_Content()
        {
            var can_shoot = can_auto_shoot();
            if (can_shoot)
            {
                Triggerable_Spring_Lever_For_Shooting.Spring_Value_01 = 1;
                auto_shoot_lever_reset_tick = 48;
            }
            return can_shoot;
        }

        // ===============================================================================

        protected override void single_shoot(fire_logic record, float ammo_velocity_mod)
        {
            base.single_shoot(record, ammo_velocity_mod);
            Spring_Force_Current = 0f;
            Windup_Lever.Reset_Windup();
        }

        // ===============================================================================

        private void action_on_Spring_Lever_trigger()
        {
            DeviceBehavior_Shooter_Try_Shoot(true, true);
        }

        // ===============================================================================

        // Public for UI Panel
        public float Spring_Force_Current
        {
            get { return _spring_force_current; }
            set
            {
                _spring_force_current = Mathf.Clamp01(value);

                if (current_ammo == 0 && Spring_Force_Current > RELOAD_SPRING_THRESHOLD)
                    DeviceBehavior_Shooter_Try_Reload();
            }
        }

        public void UI_Controlled_Rotate(float rotation_input)
        {
            DeviceBehavior_Ballista_Try_Rotate(true, rotation_input);
        }

    }
}

