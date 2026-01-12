using UnityEngine;
using UnityEngine.UI;

namespace World.Devices
{
    public class DeviceHpSlider : MonoBehaviour
    {
        public Slider hp_slider;
        public Image rank_image;
        
        public void InitRank(Sprite s)
        {
            rank_image.sprite = s;
        }

        public void UpdateHp(float value)
        {
            hp_slider.value = value;
        }
    }
}
