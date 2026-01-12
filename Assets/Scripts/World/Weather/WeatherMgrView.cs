using Addrs;
using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;
using World.Weather.WeatherViews;

namespace World.Weather
{
    public class WeatherMgrView : MonoBehaviour, IWeatherMgrView
    {
        public WeatherMgr owner;

        public Transform content;
        public Transform ui_content;

        public List<WeatherView> weather_objs = new List<WeatherView>();

        public WeatherMask weather_mask;
        void IModelView<WeatherMgr>.attach(WeatherMgr owner)
        {
            this.owner = owner;
        }

        void IModelView<WeatherMgr>.detach(WeatherMgr owner)
        {
            if (this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void IWeatherMgrView.notify_init()
        {
            
        }

        void IWeatherMgrView.notify_weather_end(Weather weather)
        {
            for(int i = 0; i < weather_objs.Count; i++)
            {
                if (weather_objs[i].weather == weather)
                {
                    Destroy(weather_objs[i].gameObject);
                    weather_objs.RemoveAt(i);
                    break;
                }
            }
        }

        void IWeatherMgrView.notify_weather_instantiated(Weather weather)
        {
            if (owner.current_weather.data.view_prefab != null && owner.current_weather.data.view_prefab.Length > 0)
            {
                Addressable_Utility.try_load_asset<WeatherView>(owner.current_weather.data.view_prefab, out var obj);
                if (obj != null)
                {
                    var weather_obj = Instantiate(obj, content, false);
                    weather_obj.init(owner,owner.current_weather);

                    weather_objs.Add(weather_obj);
                }
            }

            /*if(owner.current_weather.data.mask_prefab != null && owner.current_weather.data.mask_prefab.Length > 0)
            {
                Addressable_Utility.try_load_asset<WeatherMask>(owner.current_weather.data.mask_prefab, out var obj);
                if (obj != null)
                {
                    weather_mask = Instantiate(obj, content, false);
                    weather_mask.init(owner);
                }
            }*/            // 暂时注释
        }

        void IWeatherMgrView.notify_weather_vfx_instantiate(string vfx, Vector2 pos, float duration)
        {
            Addressable_Utility.try_load_asset<GameObject>(vfx, out var obj);
            var vfx_obj = Instantiate(obj,content,false);
            vfx_obj.transform.position = new Vector3(pos.x, pos.y, 10);
            Destroy(vfx_obj, duration);
        }

        void IWeatherMgrView.tick()
        {
            for (int i = 0; i < weather_objs.Count; i++)
            {
                weather_objs[i].tick();
            }
        }
    }
}
