using Commons;
using UnityEngine;

namespace Camp
{
    public class CampSceneInput : MonoBehaviour
    {

        //==================================================================================================

        public void OnZoom_in()
        {
            var size = CampSceneRoot.instance.mainCamera.orthographicSize;
            size -= Config.current.camp_camera_zoom_speed;

            CampSceneRoot.instance.mainCamera.orthographicSize = Mathf.Max(Config.current.camp_camera_size_range.x, size);
        }


        public void OnZoom_out()
        {
            var size = CampSceneRoot.instance.mainCamera.orthographicSize;
            size += Config.current.camp_camera_zoom_speed;

            CampSceneRoot.instance.mainCamera.orthographicSize = Mathf.Min(Config.current.camp_camera_size_range.y, size);
        }
    }
}

