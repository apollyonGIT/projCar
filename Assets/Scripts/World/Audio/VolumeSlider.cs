using UnityEngine;
using UnityEngine.UI;

namespace World.Audio
{
    public class VolumeSlider : MonoBehaviour
    {
        public AudioSource audioSource;
        public Slider slider;


        private void Update()
        {
            if (audioSource != null)
            {
                audioSource.volume = slider.value;
            }
        }

        public void Init()
        {
            slider.value = audioSource.volume;
        }
    }
}
