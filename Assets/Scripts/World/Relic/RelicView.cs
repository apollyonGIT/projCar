using Addrs;
using AutoCodes;
using Commons;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Relic
{
    public class RelicView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Relic data;
        public Image icon;
        public Image bag_bg;
        public Image shop_bg;

        public GameObject description_obj;
        public TextMeshProUGUI relic_name;
        public TextMeshProUGUI relic_description;

        public bool followed;
        public virtual void Init(Relic r)
        {
            data = r;
            if (data.desc.portrait != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(data.desc.portrait, out var s);
                icon.sprite = s;

                
                relic_ranks.TryGetValue(data.desc.rank.ToString(), out var rank_record);
                if(shop_bg != null)
                {
                    Addressable_Utility.try_load_asset<Sprite>(rank_record.UI_shop_bg, out var bg_s);
                    shop_bg.sprite = bg_s;
                }

                if(bag_bg != null)
                {
                    Addressable_Utility.try_load_asset<Sprite>(rank_record.UI_bag_bg, out var bg_s);
                    bag_bg.sprite = bg_s;
                }
            }
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (data == null)
                return;

            description_obj.gameObject.SetActive(true);
            if(followed)
                description_obj.transform.position = transform.position;
            if (relic_name != null)
                relic_name.text = Localization_Utility.get_localization(data.desc.name);
            if (relic_description != null)
                relic_description.text = Localization_Utility.get_localization(data.desc.description);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            description_obj.gameObject.SetActive(false);
        }
    }
}
