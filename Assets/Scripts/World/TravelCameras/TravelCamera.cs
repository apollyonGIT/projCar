
using UnityEngine;

namespace World.TravelCameras
{
    public class TravelCamera 
    {
        public Vector2 focus_pos;   //摄像机汇聚点
        public void after_tick()
        {
            var v = focus_pos;
            var config = Commons.Config.current;
            var fx = v.x + config.travel_scene_camera_offset_x;
            var fy = v.y;
        }
    }
}
