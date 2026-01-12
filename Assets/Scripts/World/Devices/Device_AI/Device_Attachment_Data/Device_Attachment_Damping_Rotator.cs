using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Damping_Rotator : Device_Attachment
    {
        public Device_Attachment_Damping_Rotator(float init_angle)
        {
            _ui_info_indicator_shoot_angle = init_angle;
        }

        // -------------------------------------------------------------------------------

        #region Const
        private const float DIR_INDICATOR_DAMPING_1 = 0.07f;
        private const float DIR_INDICATOR_DAMPING_2 = 0.08f;
        #endregion

        // -------------------------------------------------------------------------------

        private float _ui_info_indicator_shoot_angle;
        private float _ui_info_indicator_shoot_angle_expt;
        private float _ui_info_indicator_shoot_speed;

        // ===============================================================================

        public void Tick_Update_Damping_Rotate()
        {
            _ui_info_indicator_shoot_speed += (_ui_info_indicator_shoot_angle_expt - _ui_info_indicator_shoot_angle) * DIR_INDICATOR_DAMPING_1;
            _ui_info_indicator_shoot_speed -= DIR_INDICATOR_DAMPING_2 * Mathf.Sign(_ui_info_indicator_shoot_speed) * Mathf.Pow(_ui_info_indicator_shoot_speed, 2);
            _ui_info_indicator_shoot_angle += _ui_info_indicator_shoot_speed;
        }

        public void Set_Expected_Angle(float angle_expt)
        {
            _ui_info_indicator_shoot_angle_expt = angle_expt;
        }

        // ===============================================================================

        public float Dir_Angle
        {
            get { return _ui_info_indicator_shoot_angle; }
        }
    }
}