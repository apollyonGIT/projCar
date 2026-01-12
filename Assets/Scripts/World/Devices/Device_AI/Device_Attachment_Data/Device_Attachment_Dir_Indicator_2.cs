using System;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Dir_Indicator_2 : Device_Attachment_Dir_Indicator
    {
        private readonly float startAngle;
        private readonly float endAngle;
        private readonly float rangeSize;
        private readonly bool isFullCircle;

        public Device_Attachment_Dir_Indicator_2(
            Vector2 init_dir,
            float startAngle,
            float endAngle,
            float rotate_coef = 1f)
            : base(init_dir, -1f, rotate_coef)
        {
            this.startAngle = NormalizeAngle(startAngle);
            this.endAngle = NormalizeAngle(endAngle);
            rangeSize = CalculateRangeSize(this.startAngle, this.endAngle);
            isFullCircle = Mathf.Abs(rangeSize - 360f) < 0.01f;
        }

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;
            return angle < 0 ? angle + 360f : angle;
        }

        private float CalculateRangeSize(float start, float end)
        {
            if (end > start)
                return end - start;
            else
                return 360f - start + end;
        }

        protected override bool Rotate_Angle_Limit()
        {
            float currentAngle = NormalizeAngle(Vector2.SignedAngle(Vector2.right,_dir_v2));

            if (isFullCircle) return true;

            if (IsAngleInRange(currentAngle))
                return true;

            float clockwiseDist = AngleDistance(currentAngle, startAngle);
            float counterClockwiseDist = AngleDistance(currentAngle, endAngle);

            float targetAngle = clockwiseDist < counterClockwiseDist ? startAngle : endAngle;
            _dir_v2 = Quaternion.AngleAxis(targetAngle, Vector3.forward) * Vector2.right;
            return false;
        }

        private bool IsAngleInRange(float angle)
        {
            if (startAngle < endAngle)
                return angle >= startAngle && angle <= endAngle;
            else
                return angle >= startAngle || angle <= endAngle;
        }

        private float AngleDistance(float a, float b)
        {
            float diff = Mathf.Abs(a - b);
            return Mathf.Min(diff, 360f - diff);
        }

        public override Vector2 Dir_Vector2
        {
            set
            {
                base.Dir_Vector2 = value;
                Rotate_Angle_Limit(); 
            }
        }
    }
}
