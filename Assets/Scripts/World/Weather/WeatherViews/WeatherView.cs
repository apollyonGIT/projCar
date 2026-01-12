using UnityEngine;

namespace World.Weather.WeatherViews
{
    public class WeatherView :MonoBehaviour
    {
        public WeatherMgr owner;
        public Weather weather;

        public virtual void init(WeatherMgr owner,Weather weather)
        {
            this.owner = owner;
            this.weather = weather;
        }

        public virtual void tick()
        {

        }
    }
}
