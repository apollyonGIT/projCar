using AutoCodes;
using Commons;
using UnityEngine;

namespace World.Weather.Weathers
{
    public class Sunny : Weather, IWeather_Heal
    {
        #region Heal
        int IWeather_Heal.interval_tick { get => m_interval_tick; set => m_interval_tick = value; }
        int m_interval_tick;

        int IWeather_Heal.current_tick { get => m_current_tick; set => m_current_tick = value; }
        int m_current_tick;

        weather IWeather_Heal.data => data;

        IWeather_Heal heal => this;
        #endregion

        //==================================================================================================

        public Sunny(params object[] values) : base(values)
        {
            m_interval_tick = Mathf.RoundToInt(data.monster_HOT.Item3 * Config.PHYSICS_TICKS_PER_SECOND);
        }

        
        public override void tick()
        {
            base.tick();

            heal.do_heal();
        }
    }
}
