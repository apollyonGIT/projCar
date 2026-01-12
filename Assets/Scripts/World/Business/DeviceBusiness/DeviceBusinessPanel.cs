using Addrs;
using Commons;
using Foundations;
using Foundations.MVVM;
using UnityEngine;
using World.Devices.Equip;
using World.Helpers;

namespace World.Business { 
    public class DeviceBusinessPanel : MonoBehaviour, IBusinessView
    {
        public EquipmentMgrView ev;
        public DeviceBusinessView dv;
        public EquipmentView eqv;


        public bool inited = false;
        public Business owner;

        public void Init(uint business_id)
        {
            if (inited)
                return;
            Mission.instance.try_get_mgr("EquipMgr", out EquipmentMgr emgr);
            emgr.add_view(ev);
            
            emgr.add_view(eqv);

            emgr.Init(new string[] { });

            Mission.instance.try_get_mgr("BusinessMgr", out BusinessMgr bmgr);
            owner = new DeviceBusiness();
            bmgr.AddBusiness(owner);
            owner.add_view(this);
            owner.Init(business_id);
            inited = true;
        }

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
            dv.Init();
        }

        void IBusinessView.update_goods()
        {
            dv.UpdateGoods();
        }

        public void _Destroy()
        {
            Mission.instance.try_get_mgr("EquipMgr", out EquipmentMgr emgr);

            if(emgr != null)
            {
                emgr.remove_view(ev);
                emgr.remove_view(eqv);
            }

            if(owner!=null)
                owner.remove_view(this);
        }


        private void OnDestroy()
        {
            _Destroy();
        }

        public void Open()
        {
            dv.coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
        }
    }
}
    