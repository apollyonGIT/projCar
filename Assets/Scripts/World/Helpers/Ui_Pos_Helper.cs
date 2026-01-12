using System.Collections.Generic;
using UnityEngine;
using World.Caravans;
using World.Devices;
using World.Devices.DeviceUiViews;
using World.Ui;

namespace World.Helpers
{
    public class Ui_Pos_Helper
    {
        public static List<IUiView> ui_views = new List<IUiView>();

        public static Vector2 get_device_ui_pos(Device d)
        {
            foreach(var ui_view in ui_views)
            {
                if(ui_view is DeviceUiView dv && dv.owner == d)
                {
                    return ui_view.pos;
                }
            }
            return Vector2.zero;
        }

        public static Vector2 get_caravan_ui_pos()
        {
            foreach (var ui_view in ui_views)
            {
                if (ui_view is CaravanUiView cv)
                {
                    return ui_view.pos;
                }
            }
            return Vector2.zero;
        }

    }
}
