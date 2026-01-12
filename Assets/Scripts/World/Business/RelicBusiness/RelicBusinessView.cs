using Addrs;
using Commons;
using Foundations;
using Foundations.MVVM;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using World.Helpers;
using World.Relic;

namespace World.Business
{
    //商店全展示版
    public class RelicBusinessView : MonoBehaviour, IBusinessView
    {
        public Business owner;

        public Transform business_content;

        public SingleRelicView relic_prefab;

        public List<SingleRelicView> relic_list;

        public TextMeshProUGUI coin_text;

        public TextMeshProUGUI refresh_text;

        void IModelView<Business>.attach(Business owner)
        {
            this.owner = owner;
        }

        void IModelView<Business>.detach(Business owner)
        {
            if(this.owner != null)
            {
                this.owner = null;
            }   
            Destroy(gameObject);
        }

        void IBusinessView.init()
        {
            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
            refresh_text.text = ((RelicBusiness)owner).GetRefreshCost().ToString();
        }

        void IBusinessView.update_goods()
        {
            foreach (var relic in relic_list)
            {
                Destroy(relic.gameObject);
            }

            relic_list.Clear();

            foreach (var gd in owner.goods)
            {
                var sr = Instantiate(relic_prefab, business_content);
                sr.Init(gd, owner);
                sr.gameObject.SetActive(true);
                relic_list.Add(sr);
            }

            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
            refresh_text.text = ((RelicBusiness)owner).GetRefreshCost().ToString();
        }


        public void Refresh()
        {
            (owner as RelicBusiness).Refresh();
        }

        private RelicMgrView rview;

        public void OpenUiPanel()
        {
            if (rview != null)
            {
                rview.gameObject.SetActive(true);
            }
            else
            {
                Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
                if (rmgr != null)
                {
                    Addressable_Utility.try_load_asset<RelicMgrView>("RelicInfoWindow", out var rv);
                    rview = Instantiate(rv, transform, false);
                    rmgr.add_view(rview);
                    rview.gameObject.SetActive(true);
                    rview.Init();
                }
            }
        }

        public void Open()
        {
            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
        }
    }
}
