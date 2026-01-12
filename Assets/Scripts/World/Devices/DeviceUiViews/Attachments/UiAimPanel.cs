using UnityEngine;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class UiAimPanel : MonoBehaviour
    {
        public DeviceUiView duv;
        public UiAim ui_aim;

        public void SetTarget(ITarget t)
        {
            if(!duv.owner.target_list.Contains(t))
                duv.owner.target_list.Add(t);
        }
    }
}
