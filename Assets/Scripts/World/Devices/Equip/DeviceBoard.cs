
using System.Collections.Generic;
using UnityEngine;
using World.Helpers;

namespace World.Devices.Equip
{
    public class DeviceBoard :MonoBehaviour
    {
        public List<DeviceBoardSlot> slots = new();

        public void Init()
        {
            foreach(var slot in slots)
            {
                slot.Init();
            }   
        }
    }
}
