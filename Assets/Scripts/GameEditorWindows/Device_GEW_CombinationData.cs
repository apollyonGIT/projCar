using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEditorWindows
{
    [Serializable]

    [CreateAssetMenu(menuName = "DeviceGEW", fileName = "CombinationData")]
    public class Device_GEW_CombinationData : ScriptableObject
    {
        public List<string> devices_id = new();
        public List<string> slots = new();
    }
}
