using UnityEngine;
using World.Helpers;


namespace World.Devices.Device_AI
{
    public static class _Device_AI_Common_Action
    {
        public static Vector2 Get_Default_Dir_By_Slot(Device d)
        {
            switch (Device_Slot_Helper.GetSlot(d))
            {
                case "slot_top":
                    return Vector2.up;
                case "slot_back_top":
                    return new Vector2(-1, 1);
                case "slot_back":
                    return Vector2.left;
                case "slot_front_top":
                    return new Vector2(1, 1);
                case "slot_front":
                default:
                    return Vector2.right;
            }
        }

    }
}