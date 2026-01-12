using Foundations;

namespace World.Weather
{
    public class WeatherPD : Producer
    {
        WeatherMgr mgr;
        public WeatherMgrView wmv;
        public WeatherMgrUiView wmuv;
        public override IMgr imgr => mgr;

        public override void call()
        {
            
        }

        public override void init(int priority)
        {
            mgr = new WeatherMgr("Weather", priority);
            mgr.add_view(wmv);
            mgr.add_view(wmuv);
            mgr.Init(WorldContext.instance.r_scene.id);
        }
    }
}
