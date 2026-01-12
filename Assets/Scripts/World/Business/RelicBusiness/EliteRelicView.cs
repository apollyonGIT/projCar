using Addrs;
using AutoCodes;
using Foundations;
using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;
using World.Relic;
using World.Relic.RelicStores;

namespace World.Business
{
    public class EliteRelicView : MonoBehaviour,IBusinessView
    {

        public Business owner;
        public RelicStoreSingleView prefab;
        public Transform content;

        List<RelicStoreSingleView> m_btns = new();
        public List<RelicStoreSingleView> btns => m_btns;

        private bool selected = false;

        public RelicMgrView rview;

        public void Init(List<uint> relics)
        {
            foreach(var relic_id in relics)
            {
                var relic = get_relic(relic_id);
                var view = Instantiate(prefab, content);
                view.Init(relic);

                view.gameObject.SetActive(true);
                m_btns.Add(view);
            }
        }


        private Relic.Relic get_relic(uint relic_id)
        {
            relics.records.TryGetValue(relic_id.ToString(), out var relic_value);
            Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
            var relic  = rmgr.GetRelic(relic_value.behaviour_script);
            relic.desc = relic_value;
            return relic;
        }

        void IBusinessView.update_goods()
        {
            
        }

        void IBusinessView.init()
        {
            foreach(var good in owner.goods)
            {
                var relic_id = good.goods_id;
                var relic = get_relic(relic_id);
                var view = Instantiate(prefab, content);
                view.Init(relic);
                view.elite_relic = this;


                view.gameObject.SetActive(true);
                m_btns.Add(view);
            }

            Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
            if (rmgr != null)
            {
                rmgr.add_view(rview);
               // rview.gameObject.SetActive(true);
                rview.Init();
            }
        }

        void IModelView<Business>.attach(Business owner)
        {
            this.owner = owner;
        }

        void IModelView<Business>.detach(Business owner)
        {
            if (this.owner != null)
                this.owner = null;
            Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
            rmgr.remove_view(rview);

            Destroy(gameObject);
        }


        public void Open()
        {
            if(!selected)
                gameObject.SetActive(true);
        }

        public void SetSelect()
        {
            selected = true;
        }
    }
}
