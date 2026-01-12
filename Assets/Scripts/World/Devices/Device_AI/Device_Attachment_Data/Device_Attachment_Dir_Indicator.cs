using System;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Dir_Indicator : Device_Attachment
    {
        public Device_Attachment_Dir_Indicator(Vector2 init_dir, float angle_limit_PosNeg = -1f, float rotate_coef = 1f)
        {
            Angle_Limit_PosNeg = angle_limit_PosNeg;
            Has_Angle_Limit = angle_limit_PosNeg > 0;
            Dir_Vector2 = init_dir;
            this.rotate_coef = rotate_coef;
        }

        // -------------------------------------------------------------------------------

        public readonly float Angle_Limit_PosNeg; // 小于0时表示无角度限制
        public readonly bool Has_Angle_Limit;

        public Action on_rotate_without_reaching_limit;

        // -------------------------------------------------------------------------------

        private readonly float rotate_coef;

        protected Vector2 _dir_v2;

        // ===============================================================================

        public void Rotate_Delta_Angle(float delta_angle)
        {
            Dir_Vector2 = Quaternion.AngleAxis(delta_angle * rotate_coef, Vector3.forward) * Dir_Vector2;
        }

        // ===============================================================================

        protected virtual bool Rotate_Angle_Limit()
        {
            if (!Has_Angle_Limit)
                return true;

            var _dir_angle = Dir_Angle_With_Center_Axis_As_Up;

            if (_dir_angle > Angle_Limit_PosNeg)
            {
                _dir_v2 = Quaternion.AngleAxis(Angle_Limit_PosNeg, Vector3.forward) * Vector2.up;
                return false;
            }
            else if (_dir_angle < -Angle_Limit_PosNeg)
            {
                _dir_v2 = Quaternion.AngleAxis(-Angle_Limit_PosNeg, Vector3.forward) * Vector2.up;
                return false;
            }

            return true;
        }

        // ===============================================================================

        public float Dir_Angle
        {
            get { return Vector2.SignedAngle(Vector2.right, _dir_v2); }
            private set { Dir_Vector2 = Quaternion.AngleAxis(value, Vector3.forward) * Vector2.right; }
        }

        public float Dir_Angle_With_Center_Axis_As_Up
        {
            get { return Vector2.SignedAngle(Vector2.up, _dir_v2); }
        }

        public virtual Vector2 Dir_Vector2
        {
            get { return _dir_v2; }
            set
            {
                _dir_v2 = value;
                if (Rotate_Angle_Limit())
                    on_rotate_without_reaching_limit?.Invoke();
            }
        }

    }
}