using Commons;
using Foundations;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;
using World.VFXs;

namespace World.Weather.Weathers
{
    public class ThunderStorm : Weather
    {
        private Vector2 gap_range = new Vector2(0.2f,5f);
        private float dmg_stupor = 1f;
        public float delay = 1f;
        private Vector2 distance_range = new Vector2(5f, 10f);

        private int thunder_tick = 0;

        public ThunderStorm(params object[] values) : base(values)
        {
            if(data.diy_prms != null)
            {
                data.diy_prms.TryGetValue("gap_range", out var gap_range_str);
                if (gap_range_str != null)
                {
                    var arr = gap_range_str.Split(',');
                    if (arr.Length == 2)
                    {
                        gap_range = new Vector2(float.Parse(arr[0]), float.Parse(arr[1]));
                    }
                }

                data.diy_prms.TryGetValue("dmg_stupor", out var dmg_stupor_str);
                if (dmg_stupor_str != null)
                {
                    dmg_stupor = float.Parse(dmg_stupor_str);
                }

                data.diy_prms.TryGetValue("delay", out var delay_str);
                if (delay_str != null)
                {
                    delay = float.Parse(delay_str);
                }

                data.diy_prms.TryGetValue("distance_range", out var distance_range_str);
                if (distance_range_str != null)
                {
                    var arr = distance_range_str.Split(',');
                    if (arr.Length == 2)
                    {
                        distance_range = new Vector2(float.Parse(arr[0]), float.Parse(arr[1]));
                    }
                }
            }
            
        }
        public override void tick()
        {
            base.tick();

            thunder_tick--;

            if(thunder_tick <= 0)
            {
                thunder_tick = (int)(Random.Range(gap_range.x,gap_range.y) * Config.PHYSICS_TICKS_PER_SECOND);
                var pos = WorldContext.instance.caravan_pos;

                var x_offset = Random.Range(distance_range.x, distance_range.y);
                var thunder_x = pos.x + x_offset;
                var thunder_y = Road_Info_Helper.try_get_altitude(thunder_x);
                instantiate_thunder(new Vector2(thunder_x,thunder_y));

                Request_Helper.delay_do("instantiate_thunder",(int)(delay * Config.PHYSICS_TICKS_PER_SECOND), (Request req) =>
                {
                    var targets = BattleUtility.select_all_target_in_rect(new Vector2(thunder_x -1,thunder_y),new Vector2(thunder_x+1,thunder_y +20),WorldEnum.Faction.opposite);
                    foreach(var target in targets)
                    {
                        var atk = new Attack_Data
                        {
                            atk = (int)dmg_stupor,
                            diy_atk_str = "blunt",
                        };
                        target.hurt(null, atk,out var dmg);
                        target.applied_outlier(null, atk.diy_atk_str, dmg.dmg);
                    }
                });
            }
        }

        private void instantiate_thunder(Vector2 thunder_pos)
        {
            Mission.instance.try_get_mgr("Weather",out WeatherMgr wmgr);
            wmgr.InstantiateVfx("vfx_weather_lightning", thunder_pos, delay+3f);
        }
    }
}
