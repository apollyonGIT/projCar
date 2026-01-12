using UnityEngine;
using UnityEngine.UI;

namespace World.Caravans
{
    public class CaravanLeverPin : MonoBehaviour
    {
        public Slider slider;

        const float c_min_lever_value = 0f;
        const float c_max_lever_value = 3f;

        //==================================================================================================

        private void Start()
        {
            slider.minValue = c_min_lever_value;
            slider.maxValue = c_max_lever_value;
        }


        public void tick()
        {
            var lever = WorldContext.instance.driving_lever;
            lever = Mathf.Clamp(lever, c_min_lever_value, c_max_lever_value);
            slider.value = lever;
        }
    }
}
