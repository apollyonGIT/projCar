using UnityEngine;
using World.Caravans;
using World.Widgets;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class DeviceStuporView : DeviceEmergencyView,IUiFix
    {
        public DeviceStuporKick kick;

        public override void tick()
        {
            var mousePosition = InputController.instance.GetScreenMousePosition();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(kick.transform.parent.transform as RectTransform, mousePosition, WorldSceneRoot.instance.uiCamera, out var pos);
            kick.GetComponent<RectTransform>().anchoredPosition = pos;
            kick.gameObject.SetActive(!Widget_Fix_Context.instance.player_oper && GetComponent<RectTransform>().rect.Contains(pos));
        }

        public bool Fix()
        {
            (owner as DeviceStupor).Fix();

            return true;
        }
    }
}
