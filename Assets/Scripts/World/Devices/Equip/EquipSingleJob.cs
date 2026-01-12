using System;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.Equip
{
    public class EquipSingleJob : MonoBehaviour
    {
        public Image highlight;
        public void SetHighlight()
        {
            highlight.gameObject.SetActive(true);
        }
    }
}
