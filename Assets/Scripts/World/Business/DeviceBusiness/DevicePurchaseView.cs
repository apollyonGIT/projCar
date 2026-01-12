using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.Devices.Equip;
using World.Devices;

namespace World.Business
{
    public class DevicePurchaseView :MonoBehaviour
    {
        public DeviceBusiness owner;

        public SingleGoodsView goodsview;

        public Transform content;
        public SinglePurchaseView prefab;

        public List<SinglePurchaseView> goods_list = new();

        public void Init(DeviceBusiness db)
        {
            owner = db;
            for (int i = 0; i < goods_list.Count; i++)
            {
                Destroy(goods_list[i].gameObject);
            }
            goods_list.Clear();
            foreach (var goods in owner.purchase_devices)
            {
                var single_goods = Instantiate(prefab, content, false);
                single_goods.gameObject.SetActive(true);
                single_goods.Init(goods);
                goods_list.Add(single_goods);
            }
        }

        public void SetPurchase(GoodsData data)
        {
            goodsview.Init(data);

            goodsview.gameObject.SetActive(true);
        }

        public void UpdateGoods()
        {
            for (int i = 0; i < goods_list.Count; i++)
            {
                Destroy(goods_list[i].gameObject);
            }
            goods_list.Clear();
            foreach (var goods in owner.purchase_devices)
            {
                var single_goods = Instantiate(prefab, content, false);
                single_goods.gameObject.SetActive(true);
                single_goods.Init(goods);
                goods_list.Add(single_goods);
            }
        }

        public void Buy()
        {
            Mission.instance.try_get_mgr("EquipMgr", out EquipmentMgr emgr);
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);

            owner.RePurchaseDevice(goodsview.goods_record);
            goodsview.gameObject.SetActive(false);
        }

    }
}
