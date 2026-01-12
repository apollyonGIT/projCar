using AutoCodes;
using Commons;
using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using UnityEngine;
using World.VFXs;
using World.Weather.Weathers;

namespace World.Weather
{
    public interface IWeatherMgrView : IModelView<WeatherMgr>
    {
        void notify_init();
        void notify_weather_instantiated(Weather weather);
        void notify_weather_end(Weather weather);
        void notify_weather_vfx_instantiate(string vfx, Vector2 pos, float duration);
        void tick();
    }

    public class WeatherMgr : Model<WeatherMgr, IWeatherMgrView>, IMgr
    {
        private readonly string m_mgr_name;
        private readonly int m_mgr_priority;

        string IMgr.name => m_mgr_name;
        int IMgr.priority => m_mgr_priority;

        public WeatherMgr(string mgr_name, int mgr_priority, params object[] args)
        {
            m_mgr_name = mgr_name;
            m_mgr_priority = mgr_priority;
            (this as IMgr).init(args);
        }
        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);
        }
        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }


        //===============================================================

        public Weather current_weather;
        public weather_group current_group;
        public float current_weather_remain_length;


        public void Init(uint scene_id)
        {
            scenes.TryGetValue(scene_id.ToString(), out var scene);

            /*var total_weight = 0;
            foreach (var weather_groups in scene.scene_weather)
            {
                total_weight += weather_groups.Item3;
            }

            var rnd_weight = Random.Range(0, total_weight + 1);
            foreach (var wg in scene.scene_weather)
            {
                rnd_weight -= wg.Item3;
                if (rnd_weight <= 0)
                {
                    weather_groups.TryGetValue($"{wg.Item1},{wg.Item2}", out current_group);                //确认天气组
                    break;
                }
            }*/

            //开局sceneroot没有active,不走协程 直接启动
            //WorldSceneRoot.instance.StartCoroutine("IInit_Weather");

            var init_weather =WorldContext.instance.routeData.weather_data;
            weather_groups.TryGetValue($"{init_weather.Item1},{init_weather.Item2}", out var init_group);
            current_group = init_group;

            TryToInstantiateWeather((int)init_group.weather, out current_weather);

            current_weather.start();
            foreach (var view in views)
            {
                view.notify_weather_instantiated(current_weather);
            }
            current_weather_remain_length = Random.Range(current_group.length.Item1, current_group.length.Item2);
        }
        
        private void go_next_weather()
        {
            if (current_group == null)
                return;

            var rnd = Random.Range(0, 1000001);
            foreach(var next in current_group.next)
            {
                rnd -= next.Item2;
                if (rnd <= 0)
                {
                    weather_groups.TryGetValue($"{current_group.group_id},{next.Item1}", out current_group);      //切换到下一个天气组
                    break;
                }
            }

            var ret = TryToInstantiateWeather((int)current_group.weather, out var next_weather);   //实例化当前天气           
            if (ret)
            {
                //不应该存在为空的情况
                //此时逻辑已经切换到下一个天气 但是外观还是上一个 等待out协程结束才回销毁上一个天气的外观
                    
                end_weather_request(current_weather);
                
                current_weather = next_weather;
                
                start_weather_request(next_weather);

                Debug.Log($"group_id:{current_group.group_id}   sub_id:{current_group.sub_id}   weather_id:{current_weather.data.id}");
            }

            current_weather_remain_length = Random.Range(current_group.length.Item1, current_group.length.Item2);
        
        }


        private void end_weather_request(Weather current_weather)
        {
            Request req = new("WeatherEndRequest",
                (req) => { return current_weather.transition_factor <= 0; },
                (req) => { current_weather.transition_factor = 1; },
                (req) => {
                    foreach (var view in views)
                    {
                        view.notify_weather_end(current_weather);
                    }
                },
                (req) => {
                    current_weather.transition_factor -= Config.PHYSICS_TICK_DELTA_TIME / Config.current.weather_view_switch_duration;
                    if (current_weather.transition_factor < 0)
                        current_weather.transition_factor = 0;
                }
                );
        }

        private void start_weather_request(Weather next_weather)
        {
            Request req = new("WeatherStartRequest",
                (req) => { return next_weather.transition_factor >= 1; },
                (req) => {
                    next_weather.transition_factor = 0;
                    foreach (var view in views)
                    {
                        view.notify_weather_instantiated(next_weather);
                    }
                    next_weather.start();
                    current_weather_remain_length = Random.Range(current_group.length.Item1, current_group.length.Item2);
                },
                (req) => { },
                (req) =>
                {
                    next_weather.transition_factor += Config.PHYSICS_TICK_DELTA_TIME/Config.current.weather_view_switch_duration;
                    if (next_weather.transition_factor > 1)
                        next_weather.transition_factor = 1;
                }
                );
        }

        public void tick()
        {
            current_weather_remain_length -= WorldContext.instance.caravan_velocity.x * Config.PHYSICS_TICK_DELTA_TIME;

            if (current_weather != null)
            {
                current_weather.tick();
            }

            if(current_weather_remain_length <= 0)              //当前天气走完了
            {
                current_weather?.end();

                go_next_weather();
            }

            foreach(var view in views)
            {
                view.tick();
            }

        }

        public void tick1()
        {

        }


        public void notify_init()
        {
            foreach (var view in views)
            {
                view.notify_init();
            }
        }


        public void ChangeWeather(int weather_id)
        {
            var ret = TryToInstantiateWeather(weather_id, out var next_weather);   //实例化当前天气           
            if (ret)
            {
                //不应该存在为空的情况
                //此时逻辑已经切换到下一个天气 但是外观还是上一个 等待out协程结束才回销毁上一个天气的外观
                end_weather_request(current_weather);
                current_weather = next_weather;
                start_weather_request(next_weather);
            }

            current_weather_remain_length = Random.Range(current_group.length.Item1, current_group.length.Item2);
        }

        public bool TryToInstantiateWeather(int weather_id,out Weather w)
        {
            w = null;
            if (current_weather != null && current_weather.data.id == weather_id)
            {
                return false;
            }
            else
            {
                w = instantiate_weather(weather_id);
            }

            return true;
        }


        private Weather instantiate_weather(int weather_id)
        {
            weathers.TryGetValue(weather_id.ToString(), out var weather_data);
            var w = new Weather(weather_data);
            switch(weather_data.behaviour)
            {
                case "ThunderStorm":
                    w = new ThunderStorm(weather_data);
                    break;
                case "Rain":
                    w = new Rain(weather_data);
                    break;
                case "Sunny":
                    w = new Sunny(weather_data);
                    break;
                default:
                    w = new Weather(weather_data);
                    break;
            }
            return w;
        }

        public void InstantiateVfx(string vfx, Vector2 pos, float duration)
        {

            Mission.instance.try_get_mgr("VFX", out VFXMgr vmgr);
            vmgr.AddVFX(vfx,(int)(duration * Config.PHYSICS_TICKS_PER_SECOND) ,pos);

           /* foreach (var view in views)
            {
                view.notify_weather_vfx_instantiate(vfx, pos, duration);
            }*/
        }
    }
}
