using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Relic;

namespace World.Business
{
    public class SingleRelicView_3 : RelicView,IPointerEnterHandler, IPointerExitHandler
    {
        public Business b;
        public GoodsData gd;

        public GoodPriceUi price_ui;
        public TextMeshProUGUI relic_name_display;

        public GameObject view_content;

        public void Init(GoodsData goodsData, Business owner)
        {
            b = owner;
            gd = goodsData;

            if(goodsData == null)
            {
                view_content.SetActive(false);
                return;
            }
            else
            {
                view_content.SetActive(true);
            }

            data = (gd.obj as Relic.Relic);

            if (data.desc.portrait != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(data.desc.portrait, out var s);
                if (s != null)
                {
                    icon.color = new Color(255, 255, 255, 255);
                    icon.sprite = s;
                }

                relic_ranks.TryGetValue(data.desc.rank.ToString(), out var rank_record);
                if (shop_bg != null)
                {
                    Addressable_Utility.try_load_asset<Sprite>(rank_record.UI_shop_bg, out var bg_s);
                    shop_bg.sprite = bg_s;
                }

                if (bag_bg != null)
                {
                    Addressable_Utility.try_load_asset<Sprite>(rank_record.UI_bag_bg, out var bg_s);
                    bag_bg.sprite = bg_s;
                }
            }
            relic_name_display.text = Localization_Utility.get_localization(data.desc.name);
            price_ui.Init(gd.GetPrice(),gd.discount);
        }

        public void Clear()
        {
            icon.color = new Color(255, 255, 255, 0);
            icon.sprite = null;
            b = null;
            gd = null;
            data = null;
            relic_name_display.text = "";
            description_obj.gameObject.SetActive(false);
            price_ui.Init(0, 1);
        }

        public void Buy()
        {
            if (b == null)
                return;
            if (b.BuyGood(gd))
            {
                /*Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
                rmgr.CreateRelicAndAdd(data.desc.id.ToString());*/
            }
        }
    }
}
