using Addrs;
using Foundations;
using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Relic.RelicStores
{
    public class RelicStoreView : MonoBehaviour, IRelicStoreView
    {
        RelicStore owner;
        public RelicStoreSingleView prefab;
        public Transform content;

        List<RelicStoreSingleView> m_btns = new();
        public List<RelicStoreSingleView> btns => m_btns;

        void IModelView<RelicStore>.attach(RelicStore owner)
        {
            this.owner = owner;
        }

        void IModelView<RelicStore>.detach(RelicStore owner)
        {
            if (this.owner != null)
                this.owner = null;

            if (rview != null)
            {
                Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
                rmgr.remove_view(rview);
            }

            Destroy(gameObject);
        }

        void IRelicStoreView.init()
        {
            foreach (var relic in owner.relic_list)
            {
                var view = Instantiate(prefab, content);
                view.Init(relic);

                view.gameObject.SetActive(true);
                m_btns.Add(view);
            }
        }

        public void SkipButton()
        {
            Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
            rmgr.ClearRelicStore();

            Time.timeScale = 1;
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
                Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
                if (rmgr != null)
                {
                    Addressable_Utility.try_load_asset<RelicMgrView>("RelicInfoWindow", out var rv);
                    rview = Instantiate(rv,transform,false);
                    rmgr.add_view(rview);
                    rview.gameObject.SetActive(true);
                    rview.Init();
                }
            }
        }
    }
}
