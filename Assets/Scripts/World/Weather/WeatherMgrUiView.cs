using Addrs;
using AutoCodes;
using Commons;
using Foundations.MVVM;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Weather
{
    public class WeatherMgrUiView : MonoBehaviour, IWeatherMgrView,IPointerEnterHandler,IPointerExitHandler
    {
        public WeatherMgr owner;

        public Image weather_icon;
        public TextMeshProUGUI weather_name;
        public TextMeshProUGUI weather_desc;

        public GameObject weather_info;
        public GameObject weather_panel;

        public Transform show_pos, hide_pos;
        private const float anim_seconds = 0.2f;

        Coroutine anim_coroutine;


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
        }

        void IWeatherMgrView.notify_weather_instantiated(Weather weather)
        {
            anim_coroutine = StartCoroutine("change_weather",weather);
        }

        void IWeatherMgrView.notify_weather_vfx_instantiate(string vfx, Vector2 pos, float duration)
        {

        }

        void IWeatherMgrView.tick()
        {

        }


        IEnumerator change_weather(Weather weather)
        {
            var ticks = anim_seconds * Config.PHYSICS_TICKS_PER_SECOND;

            for (int i = 0; i < ticks; i++)
            {
                weather_panel.transform.position = Vector3.Lerp(show_pos.position, hide_pos.position, MathContext.instance.Curve_01_In_Out(i / ticks));
                yield return new WaitForSeconds(Config.PHYSICS_TICK_DELTA_TIME);
            }

            Addressable_Utility.try_load_asset<Sprite>(weather.data.weather_icon_path, out var sprite);
            weather_icon.sprite = sprite;
            weather_name.text = Localization_Utility.get_localization(weather.data.name);
            weather_desc.text = Localization_Utility.get_localization(weather.data.desc);
            LayoutRebuilder.ForceRebuildLayoutImmediate(weather_info.GetComponent<RectTransform>());

            for (int i = 0; i < ticks; i++)
            {
                weather_panel.transform.position = Vector3.Lerp(hide_pos.position, show_pos.position, MathContext.instance.Curve_01_In_Out(i / ticks));
                yield return new WaitForSeconds(Config.PHYSICS_TICK_DELTA_TIME);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            weather_info.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(weather_info.GetComponent<RectTransform>());
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            weather_info.SetActive(false);
        }
    }
}
