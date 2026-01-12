using UnityEngine;

namespace World.Weather.WeatherViews
{
    public class WeatherMask :MonoBehaviour
    {
        public float max_alpha = 0.3f;
        public WeatherMgr owner;
        public virtual void init(WeatherMgr owner)
        {
            this.owner = owner;
        }

        public virtual void tick()
        {
            
        }
    }
}
