using Addrs;
using AutoCodes;
using Commons;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices;

namespace World.Business
{
    public class SinglePurchaseView : MonoBehaviour,IPointerClickHandler
    {
        public DevicePurchaseView owner;
        public GoodsData data;

        public Image device_rank_icon;
        public TextMeshProUGUI device_text;
        public void Init(GoodsData data)
        {
            this.data = data;

            var device = (data.obj as Device);
            var rank = device.desc.rank;
            device_ranks.TryGetValue(rank.ToString(), out var rank_db);

            Addressable_Utility.try_load_asset<Sprite>(rank_db.img_name_bar, out var icon);
            device_rank_icon.sprite = icon;

            if (device_text != null)
                device_text.text = Localization_Utility.get_localization(device.desc.name);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            owner.SetPurchase(data);
        }
    }
}
