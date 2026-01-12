using Commons;
using Foundations;
using System.Collections.Generic;
using World.Devices;
using World.Devices.Equip;
using World.Helpers;

namespace World.Business
{
    public class DeviceBusiness : Business
    {
        public List<GoodsData> purchase_devices = new();

        //购买玩家手上的设备
        public void PurchaseDevice(Device device,int price_id,int price_cnt)
        {
            purchase_devices.Add(new GoodsData(device,GoodsType.device,device.desc.id,price_id,price_cnt,1));

            foreach(var view in views)
            {
                view.update_goods();
            }
        }

        //还给玩家
        public void RePurchaseDevice(GoodsData data)
        {
            var coin_id = (int)Config.current.coin_id;
            if (data.GetPriceAfterDiscount() <= Safe_Area_Helper.GetLootCount(coin_id))
            {
                purchase_devices.Remove(data);
                Safe_Area_Helper.SpendLootCount(coin_id, data.GetPriceAfterDiscount());
                GetGood(data);
                foreach (var view in views)
                {
                    view.update_goods();
                }
            }
        }

        public override void GetGood(GoodsData data)
        {
            Mission.instance.try_get_mgr("EquipMgr", out EquipmentMgr emgr);
            var device = (data.obj as Device);
            emgr.AddDevice(device);
        }
    }
}
