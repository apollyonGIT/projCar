using Addrs;
using Foundations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Relic;

namespace World.Business
{
    public class SingleRelicView : RelicView,IPointerClickHandler
    {
        public Business b;

        public GoodsData gd;
        public TextMeshProUGUI price_text;
        public void Init(GoodsData gd,Business b)
        {
            this.b = b;
            this.gd = gd;
            data = (gd.obj as Relic.Relic);

            if (data.desc.portrait != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(data.desc.portrait, out var s);
                if(s!=null)
                    icon.sprite = s;
            }

            if(price_text != null)
                price_text.text = gd.GetPriceAfterDiscount().ToString();
        }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (b.BuyGood(gd))
            {
                Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
                rmgr.CreateRelicAndAdd(data.desc.id.ToString());

                Destroy(gameObject);
            }
        }
    }
}
