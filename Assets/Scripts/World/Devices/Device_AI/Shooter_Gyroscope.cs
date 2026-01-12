using AutoCodes;
using UnityEngine;
using World.Devices.Device_AI;


namespace World.Devices
{
    public class Shooter_Gyroscope : NewBasicShooter
    {
        #region
        private const float FACTOR_MIN = 1F;
        public const float FACTOR_MAX = 7.5F;
        private const float FACTOR_DECAY_COEF = 0.0025F;
        #endregion

        public float attack_factor = 1f;

        protected override float ammo_velocity_mod => attack_factor;
        protected override float shoot_deg_offset { get; set; } = 180f;


        // ===================================================================================
        public override void InitData(device_all rc)
        {
            bones_direction.Clear();

            base.InitData(rc);
            bones_direction[BONE_FOR_ROTATE] = Vector2.right;
        }



        public override void tick()
        {
            base.tick();

            if (fsm == NewDevice_FSM_Shooter.idle && can_manual_shoot())
            {
                FSM_change_to(NewDevice_FSM_Shooter.shoot);
                shoot_deg_offset = shoot_deg_offset > 90f ? 0f : 180f;
            }

            change_factor_of_anim_speed(attack_factor - (attack_factor - 1) * FACTOR_DECAY_COEF);
        }



        // ===================================================================================



        private void change_factor_of_anim_speed(float f)
        {
            attack_factor = Mathf.Clamp(f, FACTOR_MIN, FACTOR_MAX);
            Current_Interval = (int)(Current_Interval / attack_factor);

            foreach (var view in views)
                view.notify_change_anim_speed(fsm == NewDevice_FSM_Shooter.shoot ? attack_factor : 1f);
        }

        public void UI_Control_Mul_Factor(float factor_mul)
        {
            change_factor_of_anim_speed(attack_factor * factor_mul);
        }

    }
}

