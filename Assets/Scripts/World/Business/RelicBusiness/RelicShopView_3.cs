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
    public class RelicShopView_3 : MonoBehaviour,IBusinessView
    {
        public Business owner;
        public List<SingleRelicView_3> relic_views_3;

        public TextMeshProUGUI coin_text;
        public TextMeshProUGUI refresh_text;

        public RelicMgrView prefab;
        public Transform relic_mgr_content;

        void IModelView<Business>.attach(Business owner)
        {
            this.owner = owner;   
        }

        void IModelView<Business>.detach(Business owner)
        {
            if(this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void IBusinessView.init()
        {

        }

        private void init_goods()
        {
            for(int i = 0; i < relic_views_3.Count; i++)
            {
                relic_views_3[i].Clear();
            }

            if (owner.goods.Count < 3)
            {
                for(int i =0;i< owner.goods.Count; i++)
                {
                    var goods = owner.goods[i];
                    relic_views_3[i].Init(goods, owner);
                    relic_views_3[i].gameObject.SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    var goods = owner.goods[i];
                    relic_views_3[i].Init(goods, owner);
                    relic_views_3[i].gameObject.SetActive(true);
                }
            }
        }

        void IBusinessView.update_goods()
        {
            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
            refresh_text.text = ((RelicBusiness)owner).GetRefreshCost().ToString();

            init_goods();
        }

        public void Refresh()
        {
            ((RelicBusiness)owner).Refresh();
        }

        public void Open()
        {
            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();

            if (rview != null)
            {
                rview.gameObject.SetActive(true);
            }
            else
            {
                Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
                if (rmgr != null)
                {
                    rview = Instantiate(prefab, relic_mgr_content, false);
                    rmgr.add_view(rview);
                    rview.gameObject.SetActive(true);
                    rview.Init();
                }
            }
        }

        public void Close() 
        {
            Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
            rmgr.remove_view(rview);
        }

        private RelicMgrView rview;
        


        public void OpenNpcPanel()
        {
            //TBD
        }

        public void OpenDevicePanel()
        {
            //TBD
        }
    }
}
