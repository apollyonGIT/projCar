using UnityEngine;


namespace World.Devices.Device_AI
{
    public class Shooter_Gatlin : DScript_Shooter
    {
        #region Const
        private const float BARREL_ROTATE_SPEED_UP_ACC = 0.9F * Commons.Config.PHYSICS_TICK_DELTA_TIME;
        private const float BARREL_ROTATE_SPEED_DOWN_ACC = -0.05F * Commons.Config.PHYSICS_TICK_DELTA_TIME;
        private const float BARREL_ROTATE_SPEED_DAMPING_MOD = 0.999F;
        private const float BARREL_ROTATE_SPEED_MAX = 80F;
        private const float SHOOT_PER_ANGLE = 45F; // 每转过一定角度，就射出一发子弹
        private const float CAN_RELOAD_ROTATION_SPEED_LIMIT = 0.05F; // 转速不低于该数值时，无法装填。
        private const float CRANK_ROTATE_SPEED_COEF = 0.8F;
        #endregion

        // -------------------------------------------------------------------------------

        public Device_Attachment_Ammo_Box ammoBox = new (1);
        public Device_Attachment_Ratchet ratchetShoot = new (-90f);

        // -------------------------------------------------------------------------------

        private float _barrel_rotate_angle;
        private float _barrel_rotate_speed_current;

        private float stored_angle_for_fire;

        // ===============================================================================

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();
            if (Barrel_Rotation_Speed_Current > 0)
            {
                Barrel_Rotation_Speed_Current += BARREL_ROTATE_SPEED_DOWN_ACC;
                Barrel_Rotation_Speed_Current *= BARREL_ROTATE_SPEED_DAMPING_MOD;
                _barrel_rotate_angle += Barrel_Rotation_Speed_Current;
                stored_angle_for_fire += Barrel_Rotation_Speed_Current;
            }

            if (stored_angle_for_fire >= SHOOT_PER_ANGLE)
            {
                DeviceBehavior_Shooter_Try_Shoot(true, true);  // 通过旋转角度来判断是否射击，需要忽略默认射击后摇
                stored_angle_for_fire -= SHOOT_PER_ANGLE;
            }
        }

        // ===============================================================================

        public override void TryToAutoAttack()
        {
            if (can_shoot_check_ammo() && can_shoot_check_error_angle())
                Auto_Attack_Job_Content();
        }

        protected override bool Auto_Attack_Job_Content()
        {
            ratchetShoot.rotate(1, true);
            UI_Controlled_Speed_Up_Spining(1);
            return true;
        }

        // ===============================================================================

        protected override bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading)
        {
            if (ammoBox.Use_Ammo_Box(can_manual_reload()))
            {
                current_ammo = (int)fire_logic_data.capacity;
                return true;
            }
            return false;
        }

        // -------------------------------------------------------------------------------

        protected override bool can_manual_reload()
        {
            return base.can_manual_reload() && can_reload_check_barrel_rotate();
        }

        // -------------------------------------------------------------------------------

        private bool can_reload_check_barrel_rotate()
        {
            return Barrel_Rotation_Speed_Current < CAN_RELOAD_ROTATION_SPEED_LIMIT;
        }

        // ===============================================================================

        // For UI Info
        public float Barrel_Rotation_Speed_Current
        {
            get { return _barrel_rotate_speed_current; }
            private set
            {
                _barrel_rotate_speed_current = Mathf.Clamp(value, 0, BARREL_ROTATE_SPEED_MAX);
                ratchetShoot.Rotation_Speed_Limit_In_Right_Dir = _barrel_rotate_speed_current * CRANK_ROTATE_SPEED_COEF;
            }
        }

        public float Barrel_Rotation_Angle
        {
            get { return _barrel_rotate_angle; }
            private set { _barrel_rotate_speed_current = value; }
        }

        public void UI_Controlled_Speed_Up_Spining(float rotation_power)
        {
            Barrel_Rotation_Speed_Current += BARREL_ROTATE_SPEED_UP_ACC * rotation_power;
        }
    }
}