using UnityEngine;

namespace Foundations.CurveSimulators
{
    public class CurveSimulator : MonoBehaviour
    {
        public Vector2 x_range = new(0, 10);
        public Vector2 y_range = new(0, 10);

        //==================================================================================================

        public float function(float x)
        {
            var y = x * x * x * x + 5;

            return y;
        }
    }
}

