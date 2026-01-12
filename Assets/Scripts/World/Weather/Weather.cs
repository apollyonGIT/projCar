using AutoCodes;

namespace World.Weather
{
    public class Weather
    {
        public weather data;
        public float transition_factor = 1f;   //过渡系数 0-1

        public Weather(params object[] values)
        {
            data = values[0] as weather;
        }
        public virtual void start()
        {

        }


        public virtual void tick()
        {
            
        }


        public virtual void end()
        {

        }
    }
}
