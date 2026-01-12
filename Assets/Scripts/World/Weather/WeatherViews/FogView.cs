using Addrs;
using System.Collections.Generic;
using UnityEngine;

namespace World.Weather.WeatherViews
{
    public class CloudObj
    {
        public GameObject obj;
        public float t;
    }

    public class FogView : WeatherView
    {
        [Header("薄雾")]
        public bool mist_active = false;
        public string mist_path;
        public float mist_max_alpha = 0.3f;

        private GameObject mist_obj;

        [Header("浓雾")]
        public bool heavy_fog_active = false;
        public string heavy_fog_path;

        private GameObject heavy_fog_obj;

        [Header("妖雾")]
        public bool demon_fog_active = false;
        public List<Sprite> cloud_sprites = new();
        public int cloud_num = 10;
        public float radius_1;
        public float radius_2;
        public float aspect_ratio = 0.56f;
        public Vector2 angle_radius;
        public float swing_amplitude = 2f;
        public float swing_interval = 2f;



        private List<CloudObj> cloud_objs = new();


        public override void init(WeatherMgr owner, Weather weather)
        {
            base.init(owner, weather);

            if (mist_active && mist_path != null)
            {
                Addressable_Utility.try_load_asset<GameObject>(mist_path, out var mist_prefab);
                mist_obj = Instantiate(mist_prefab, transform);
                var mist_color = mist_obj.GetComponent<SpriteRenderer>().color;
                mist_obj.GetComponent<SpriteRenderer>().color = new Color(mist_color.r, mist_color.g, mist_color.b, 0);
            }

            if (heavy_fog_active && heavy_fog_path != null)
            {
                Addressable_Utility.try_load_asset<GameObject>(heavy_fog_path, out var heavy_fog_prefab);
                heavy_fog_obj = Instantiate(heavy_fog_prefab, transform);
                heavy_fog_obj.GetComponent<SpriteRenderer>().material.SetFloat("_Arg1", 0);
            }

            if (demon_fog_active)
            {
                Vector2 camera_pos = WorldSceneRoot.instance.mainCamera.transform.position;
                var total_angle = angle_radius.y - angle_radius.x;
                var per_angle = total_angle / cloud_num;

                for (int i = 0; i < cloud_num; i++)
                {
                    var cloud_obj = new GameObject($"cloud_{i}");
                    var sr = cloud_obj.AddComponent<SpriteRenderer>();
                    cloud_obj.transform.SetParent(transform);
                    sr.sprite = cloud_sprites[Random.Range(0, cloud_sprites.Count)];
                    var current_angle = per_angle * i;
                    Vector2 q = new Vector2(Mathf.Cos(current_angle * Mathf.Deg2Rad), Mathf.Sin(current_angle * Mathf.Deg2Rad)) * radius_1;
                    Vector2 p = new Vector2(Mathf.Cos(current_angle * Mathf.Deg2Rad), Mathf.Sin(current_angle * Mathf.Deg2Rad)) * radius_2;

                    q.y *= aspect_ratio;
                    p.y *= aspect_ratio;

                    cloud_obj.transform.position = new Vector3(camera_pos.x + p.x, camera_pos.y + p.y, 10);
                    cloud_obj.GetComponent<SpriteRenderer>().sortingLayerName = "Front_Landscape";
                    cloud_obj.GetComponent<SpriteRenderer>().sortingOrder = 10;

                    cloud_objs.Add(new()
                    {
                        obj = cloud_obj,
                        t = Random.Range(0f, swing_interval),
                    });
                }
            }
        }

        public override void tick()
        {
            var pos = WorldSceneRoot.instance.mainCamera.transform.position;
            if (mist_active && mist_path != null)
            {
                mist_obj.transform.position = new Vector3(pos.x, pos.y, 10);
                var mist_color = mist_obj.GetComponent<SpriteRenderer>().color;
                mist_obj.GetComponent<SpriteRenderer>().color = new Color(mist_color.r, mist_color.g, mist_color.b, mist_max_alpha * weather.transition_factor);
            }

            if (heavy_fog_active && heavy_fog_path != null)
            {
                heavy_fog_obj.transform.position = new Vector3(pos.x, pos.y, 10);
                heavy_fog_obj.GetComponent<SpriteRenderer>().material.SetFloat("_Arg1", weather.transition_factor);
            }

            if (demon_fog_active)
            {
                var total_angle = angle_radius.y - angle_radius.x;
                var per_angle = total_angle / cloud_num;
                for (int i = 0; i < cloud_num; i++)
                {
                    var cloud_obj = cloud_objs[i];

                    cloud_obj.t++;

                    var current_angle = per_angle * i;
                    Vector2 q = new Vector2(Mathf.Cos(current_angle * Mathf.Deg2Rad), Mathf.Sin(current_angle * Mathf.Deg2Rad)) * radius_1;
                    Vector2 p = new Vector2(Mathf.Cos(current_angle * Mathf.Deg2Rad), Mathf.Sin(current_angle * Mathf.Deg2Rad)) * radius_2;
                    q.y *= aspect_ratio;
                    p.y *= aspect_ratio;

                    var dir = (p - q).normalized;
                    var x_offset = Mathf.Lerp(0, dir.x * swing_amplitude, MathContext.instance.Curve_010_Smooth(cloud_obj.t / swing_interval % 1));
                    var y_offset = Mathf.Lerp(0, dir.y * swing_amplitude, MathContext.instance.Curve_010_Smooth(cloud_obj.t / swing_interval % 1));

                    var x = Mathf.Lerp(pos.x + p.x, pos.x + q.x, MathContext.instance.Curve_01_In_Out(weather.transition_factor)) + x_offset;
                    var y = Mathf.Lerp(pos.y + p.y, pos.y + q.y, MathContext.instance.Curve_01_In_Out(weather.transition_factor)) + y_offset;

                    cloud_obj.obj.transform.position = new Vector3(x, y, 10);
                }
            }
        }
    }
}
