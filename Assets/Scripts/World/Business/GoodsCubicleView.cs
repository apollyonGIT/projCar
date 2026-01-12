using Addrs;
using AutoCodes;
using UnityEngine;
using UnityEngine.UI;

namespace World.Business
{
    public class GoodsCubicleView : MonoBehaviour
    {
        public Image cubicle_icon;

        public void Init(uint cubicle_id)
        {
            device_cubicless.TryGetValue(cubicle_id.ToString(), out var cubicle);
            Addressable_Utility.try_load_asset<Sprite>(cubicle.icon_idle, out var icon);
            cubicle_icon.sprite = icon;
        }
    }
}
