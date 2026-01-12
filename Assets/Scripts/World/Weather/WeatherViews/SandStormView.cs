using Addrs;
using UnityEngine;

namespace World.Weather.WeatherViews
{
    public class SandStormView : WeatherView
    {
        [Header("沙暴")]
        public bool sandstorm_active = false;
        public string sandstorm_path;
        private GameObject sandstorm_obj;

        [Header("沙砾")]
        public bool grit_active = false;
        public string grit_path;
        private GameObject grit_obj;    

        public override void init(WeatherMgr owner, Weather weather)
        {
            base.init(owner, weather);

            if (sandstorm_active && sandstorm_path != null)
            {
                Addressable_Utility.try_load_asset<GameObject>(sandstorm_path, out var sandstorm_prefab);
                sandstorm_obj = Instantiate(sandstorm_prefab, transform);
                sandstorm_obj.GetComponent<SpriteRenderer>().material.SetFloat("_Arg1", 0);
            }

            if (grit_active && grit_path != null)
            {
                Addressable_Utility.try_load_asset<GameObject>(grit_path, out var grit_prefab);
                grit_obj = Instantiate(grit_prefab, transform);
                grit_obj.GetComponent<SpriteRenderer>().material.SetFloat("_Arg1", 0);
            }
        }

        public override void tick()
        {
            var pos = WorldSceneRoot.instance.mainCamera.transform.position;
            if(sandstorm_active && sandstorm_obj != null)
            {
                sandstorm_obj.transform.position = new Vector3(pos.x, pos.y, 10);
                sandstorm_obj.GetComponent<SpriteRenderer>().material.SetFloat("_Arg1", weather.transition_factor);
            }

            if (grit_active && grit_obj != null)
            {
                grit_obj.transform.position = new Vector3(pos.x, pos.y, 10);
                grit_obj.GetComponent<SpriteRenderer>().material.SetFloat("_Arg1", weather.transition_factor);
            }
        }
    }
}
