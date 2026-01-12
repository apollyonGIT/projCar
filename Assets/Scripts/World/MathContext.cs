using Foundations;
using UnityEngine;

namespace World
{
    public class MathContext : Singleton<MathContext>
    {
        public float Curve_01_In_Out(float t, float n = 2)
        {
            if (t < 0.5f)
            {
                return Mathf.Pow(2, n - 1) * Mathf.Pow(t, n);
            }
            else
            {
                return 1 - Mathf.Pow(2, n - 1) * Mathf.Pow(1 - t, n);
            }
        }

        public float Curve_010_Smooth(float t, float n = 2)
        {
            if (t < 0.5f)
            {
                return Curve_01_In_Out(t * 2, n);
            }
            else
            {
                return Curve_01_In_Out((1 - t) * 2, n);
            }
        }

        public float Curve_Parabola(float t)
        {
            return 1 - 4 * Mathf.Pow(0.5f - t, 2);
        }

        public float Curve_01_In(float t, float n = 2)
        {
            return 1 - Mathf.Pow(1 - t, n);
        }

        public float Curve_01_Out(float t, float n = 2)
        {
            return Mathf.Pow(t, n);
        }

        public float Curve_01_Bounce(float t, float n = 0.5f)
        {
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - n / 4) * (2 * Mathf.PI) / n) + 1;
        }
    }
}
