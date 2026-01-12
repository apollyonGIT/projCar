using UnityEngine;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class OperateButton : MonoBehaviour
    {
        public DeviceUiView uiview;

        public void StartControl()
        {
            if (uiview != null)
            {
                InputController.instance.SetDeviceControl(uiview.owner);
            }
        }
    }
}
