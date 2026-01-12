using System;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Ratchet : Device_Attachment
    {
        public Device_Attachment_Ratchet(float init_angle, float rotate_speed_in_right_dir = DEFAULT_ROTATE_SPEED_MAX, float rotate_speed_in_wrong_dir = CRANK_ROTATE_SPEED_MIN)
        {
            _dir_angle = init_angle;
            _rotation_speed_limit_in_right_dir = rotate_speed_in_right_dir;
            rotate_speed_min = rotate_speed_in_wrong_dir;
        }

        #region Const
        private const float CRANK_ROTATE_SPEED_MIN = -10F; //曲柄反向旋转时的转速不超过这个
        private const float DEFAULT_ROTATE_SPEED_MAX = 12F;
        #endregion

        // -------------------------------------------------------------------------------

        public Action on_rotate;

        // -------------------------------------------------------------------------------

        private readonly float rotate_speed_min;

        private float _dir_angle;
        private float _rotation_speed_limit_in_right_dir;

        // ===============================================================================

        public void Dir_Angle_Normalize(int circle)
        {
            _dir_angle += 360F * circle;
        }

        // ===============================================================================

        public float Rotation_Speed_Limit_In_Right_Dir
        {
            set { _rotation_speed_limit_in_right_dir = value; }
        }

        public float Dir_Angle
        {
            get { return _dir_angle; }
            private set { _dir_angle = value; }
        }

        public void rotate(float input_01, bool right_dir)
        {
            Dir_Angle += input_01 * (right_dir ? _rotation_speed_limit_in_right_dir : rotate_speed_min);
            on_rotate?.Invoke();
        }
    }
}