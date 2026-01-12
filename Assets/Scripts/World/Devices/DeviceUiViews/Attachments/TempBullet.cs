using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class TempBullet : MonoBehaviour
    {
        public List<Image> images = new();
        [HideInInspector]
        public int life_tick;
        [HideInInspector]
        public int current_tick;
        public void init(int life_tick)
        {
            this.life_tick = life_tick;
            current_tick = life_tick;
        }
        public void tick()
        {
            current_tick--;

            foreach(var image in images)
            {
                var color = image.color;
                image.color = new Color(color.r, color.g, color.b, (float)current_tick / (float)life_tick);
            }
        }
    }
}
