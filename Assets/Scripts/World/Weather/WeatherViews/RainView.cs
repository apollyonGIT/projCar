using Addrs;
using Commons;
using UnityEngine;
using World.Helpers;
using World.Weather.Weathers;
using static UnityEngine.ParticleSystem;

namespace World.Weather.WeatherViews
{
    public class RainView : WeatherView
    {
        public Vector2 GAP_RANGE = new Vector2(0.07f, 0.2f);     //刷新的时间间隔
        readonly (float left, float right) DIS_RANGE = (-12f, 12f);   //刷新的水平距离

        private int spawn_gap = 0;
        private float record_emission;
        private MinMaxCurve record_start_speed;
        public float max_alpha = 0.3f;

        private GameObject rain_particle;
        private GameObject scene_mask;
        private GameObject fog;


        public string particle_path = "weather_rain_base_vfx";
        public string scene_mask_path = "weather_rain_window_vfx";
        public string fog_path = "weather_rainfog_base_vfx";
        public string splash_path = "weather_splash_vfx";


        public override void init(WeatherMgr owner, Weather weather)
        {
            base.init(owner,weather);

            //生成雨粒子
            if(particle_path!=null && particle_path.Length > 0)
            {
                Addressable_Utility.try_load_asset<GameObject>(particle_path, out var rain_particle_system);
                rain_particle = Instantiate(rain_particle_system, transform);
                record_emission = rain_particle.GetComponent<ParticleSystem>().emission.rateOverTime.constant;
                record_start_speed = rain_particle.GetComponent<ParticleSystem>().main.startSpeed;
            }

            //生成场景遮罩
            if(scene_mask_path!=null && scene_mask_path.Length > 0)
            {
                Addressable_Utility.try_load_asset<GameObject>(scene_mask_path, out var scene_mask_obj);
                scene_mask = Instantiate(scene_mask_obj, transform);
            }
            

            //生成雾气
            if(fog_path!=null && fog_path.Length > 0)
            {
                Addressable_Utility.try_load_asset<GameObject>(fog_path, out var fog_obj);
                fog = Instantiate(fog_obj, transform);
            }
        }

        public override void tick()
        {
            var pos = WorldSceneRoot.instance.mainCamera.transform.position;
            if (rain_particle != null)
            {
                rain_particle.transform.position = new Vector3(pos.x, pos.y, 10);
                var emission = rain_particle.GetComponent<ParticleSystem>().emission;
                emission.rateOverTime = new MinMaxCurve(record_emission * weather.transition_factor);
                /*var main = GetComponent<ParticleSystem>().main;           //可以后续做
                  main.startSpeed = new MinMaxCurve(record_start_speed.constantMin *owner.transition_factor,record_start_speed.constantMax * owner.transition_factor);*/
            }

            if(scene_mask != null)
            {
                scene_mask.transform.position = new Vector3(pos.x, pos.y, 10);
                var color = scene_mask.GetComponent<SpriteRenderer>().color;
                scene_mask.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, weather.transition_factor * max_alpha);
            }

            if(fog != null)
            {
                fog.transform.position = new Vector3(pos.x, pos.y, 10);
                fog.GetComponent<SpriteRenderer>().material.SetFloat("_Arg1", weather.transition_factor);
            }
                

            spawn_gap--;

            if (spawn_gap <= 0)
            {
                Vector2 t = ((GAP_RANGE - Vector2.one ) * weather.transition_factor + Vector2.one ) * Config.PHYSICS_TICKS_PER_SECOND;
                spawn_gap = Random.Range((int)t.x, (int)t.y);

                instantiate_splash();
            }
        }


        private void instantiate_splash()
        {
            if(splash_path == null || splash_path.Length == 0)
                return;
            Addressable_Utility.try_load_asset<GameObject>(splash_path, out var splash_obj);
            var splash = Instantiate(splash_obj, transform);             //不能依附在rainview上,rainview要跟车移动
            var pos = WorldContext.instance.caravan_pos;
            var offset = Random.Range(DIS_RANGE.left, DIS_RANGE.right);

            var road_height = Road_Info_Helper.try_get_altitude(pos.x + offset);
            splash.transform.position = new Vector3(pos.x + offset, road_height, 10);

            splash.GetComponent<Animator>().Play("AnimationClip_RainFlower");
            Destroy(splash,0.5f);
        }
    }
}
