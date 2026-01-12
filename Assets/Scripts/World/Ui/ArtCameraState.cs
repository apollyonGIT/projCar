using Commons;
using TMPro;
using UnityEngine;

namespace World.Ui
{
    public class ArtCameraState :MonoBehaviour
    {
        public TextMeshProUGUI cameraText;

        public void ChangeCameraState()
        {
            Config.current.free_camera = !Config.current.free_camera;

            if (Config.current.free_camera)
            {
                cameraText.text = "FreeCamera";
            }
            else
            {
                cameraText.text = "Focus";
            }
        }

        public void Awake()
        {
            if (Config.current.free_camera)
            {
                cameraText.text = "FreeCamera";
            }
            else
            {
                cameraText.text = "Focus";
            }
        }
    }
}
