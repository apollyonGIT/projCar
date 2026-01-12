using AutoCodes;
using Commons;
using Foundations;
using Foundations.Excels;
using Foundations.MVVM;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Characters;
using World.Devices;
using World.Helpers;
using World.Relic;

namespace World.Business
{
    public interface IBusinessView : IModelView<Business>
    {
        void update_goods();
        void init();
    }

    public class Business : Model<Business, IBusinessView>
    {
        public List<GoodsData> goods = new();
        public virtual void Init(uint business_id)
        {
            shops.TryGetValue(business_id.ToString(), out shop record);
            var total_weight = 0;

            var shop_count = Random.Range(record.goods_count_rnd.Item1, record.goods_count_rnd.Item2 + 1);

            List<(uint, int)> goods_record_list = new();


            if (record.shop_type.value == Shop_Type.EN_Shop_Type.角色)
            {
                Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
                foreach (var (goods_id, goods_weight) in record.goods_rnd_pool)
                {
                    if (cmgr.characters.Any(c => c == null || c.desc.id == goods_id))
                    {
                        //已经有的角色不要加入随机池了
                        continue;
                    }
                    total_weight += goods_weight;

                    goods_record_list.Add((goods_id, goods_weight));
                }
            }
            else if (record.shop_type.value == Shop_Type.EN_Shop_Type.遗物商人 || record.shop_type.value == Shop_Type.EN_Shop_Type.遗物掉落)
            {
                Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
                //遗物商人目前没有角色,所以不需要去重
                foreach (var (goods_id, goods_weight) in record.goods_rnd_pool)
                {
                    if (rmgr.relic_museum.CheckRelic(goods_id))
                    {
                        total_weight += goods_weight;
                        goods_record_list.Add((goods_id, goods_weight));
                    }
                }
            }
            else
            {
                //角色要去重,其他货物目前没发现要去重的必要

                foreach (var (goods_id, goods_weight) in record.goods_rnd_pool)
                {
                    total_weight += goods_weight;

                    goods_record_list.Add((goods_id, goods_weight));
                }
            }

            var discount = record.discount;
            var max_discount_count = discount.Item1;
            var discount_probability = discount.Item2;
            var discount_rate = discount.Item3;


            int loop_time = 0;      //防止死循环

            var current_discount_count = 0;

            while (shop_count > 0 && loop_time++ <= 50)
            {
                var rnd_weight = Random.Range(0, total_weight);
                for (int i = goods_record_list.Count - 1; i >= 0; i--)
                {
                    var goods_rec = goods_record_list[i];
                    if (goods_rec.Item2 > rnd_weight)
                    {
                        var price_weight_total = 0;

                        foreach (var (price, price_weight) in record.shop_goods_price)
                        {
                            price_weight_total += price_weight;
                        }

                        var rnd_price_weight = Random.Range(0, price_weight_total);

                        foreach (var (price_rate, price_weight) in record.shop_goods_price)
                        {
                            rnd_price_weight -= price_weight;
                            if (rnd_price_weight <= 0)
                            {
                                var _discount = 1f;
                                var rnd_dicount = Random.Range(0, 1f);
                                if(rnd_dicount < discount_probability && current_discount_count < max_discount_count)
                                {
                                    current_discount_count++;
                                    _discount = discount_rate;
                                }

                                var basic_price = GetBasicValue((int)goods_rec.Item1, record.shop_type);
                                var gd = new GoodsData(GetGoodsType(record.shop_type.value), goods_rec.Item1, (int)Config.current.coin_id, (int)((basic_price * price_rate / 1000f)), _discount);

                                goods.Add(gd);

                                break;
                            }
                        }
                        total_weight -= goods_record_list[i].Item2;
                        goods_record_list.RemoveAt(i);
                        shop_count--;
                        break;
                    }
                    else
                    {
                        rnd_weight -= goods_rec.Item2;
                    }
                }
            }

            foreach (var view in views)
            {
                view.init();
            }

            foreach (var view in views)
            {
                view.update_goods();
            }
        }

        protected int GetBasicValue(int id, Shop_Type shop_type)
        {
            switch (shop_type.value)
            {
                case Shop_Type.EN_Shop_Type.设备:
                    device_alls.TryGetValue($"{id},0", out var device_record);
                    return device_record.base_value;
                case Shop_Type.EN_Shop_Type.角色:
                    roles.TryGetValue(id.ToString(), out var character_record);
                    return character_record.base_value;
                case Shop_Type.EN_Shop_Type.遗物商人:
                    relics.TryGetValue(id.ToString(), out var relic_record);
                    return relic_record.base_value;
                case Shop_Type.EN_Shop_Type.遗物掉落:
                    relics.TryGetValue(id.ToString(), out var relic_record2);
                    return relic_record2.base_value;
                default:
                    return 0;
            }
        }

        public int purchase_limitation = 99;

        public void AddGoods(object obj, GoodsType goods_type, uint goods_id, int price_id, int price_count)
        {
            goods.Add(new GoodsData(obj, goods_type, goods_id, price_id, price_count, 1));

            foreach (var view in views)
            {
                view.update_goods();
            }
        }

        protected GoodsType GetGoodsType(Shop_Type.EN_Shop_Type st)
        {
            switch (st)
            {
                case Shop_Type.EN_Shop_Type.设备:
                    return GoodsType.device;
                case Shop_Type.EN_Shop_Type.角色:
                    return GoodsType.role;
                case Shop_Type.EN_Shop_Type.遗物掉落:
                    return GoodsType.drops;
                case Shop_Type.EN_Shop_Type.遗物商人:
                    return GoodsType.relic;
                default:
                    return GoodsType.device;
            }
        }

        public void RemoveGoods(uint goods_id)
        {
            for (int i = goods.Count - 1; i >= 0; i--)
            {
                if (goods[i].goods_id == goods_id)
                {
                    goods.RemoveAt(i);
                    break;
                }
            }

            foreach (var view in views)
            {
                view.update_goods();
            }
        }

        public virtual void GetGood(GoodsData data)
        {

        }

        public bool BuyGood(GoodsData single_good)
        {
            var coin_id = (int)Config.current.coin_id;
            if (single_good.GetPriceAfterDiscount() <= Safe_Area_Helper.GetLootCount(coin_id))
            {
                if (purchase_limitation > 0)
                {
                    purchase_limitation--;

                    Safe_Area_Helper.SpendLootCount(coin_id, single_good.GetPriceAfterDiscount());

                    GetGood(single_good);

                    var index = goods.IndexOf(single_good);


                    goods[index] = null;

                    foreach (var view in views)
                    {
                        view.update_goods();
                    }
                    return true;
                }

#if UNITY_EDITOR
                Debug.LogError("购买物品失败,商店没有该物品");
#endif
            }
            return false;
        }


        //把一个随机商品变成免费
        public void SetRndFree()
        {
            var rnd = Random.Range(0, goods.Count);
            goods[rnd].discount = 0;

            foreach (var view in views)
            {
                view.update_goods();
            }
        }
    }

    public class GoodsData
    {
        public object obj;

        public GoodsType goods_type;
        public uint goods_id;
        public int price_id;
        private int price_count;
        public float discount;

        public GoodsData(GoodsType goods_type,uint goods_id,int price_id,int price_count,float discount)
        {
            this.goods_type = goods_type;
            this.goods_id = goods_id;
            this.price_id = price_id;
            this.price_count = price_count;
            this.discount = discount;

            init_obj();
        }

        public GoodsData(object obj,GoodsType goods_type, uint goods_id, int price_id, int price_count, float discount)
        {
            this.obj = obj;
            this.goods_type = goods_type;
            this.goods_id = goods_id;
            this.price_id = price_id;
            this.price_count = price_count;
            this.discount = discount;
        }

        private void init_obj()
        {
            switch (goods_type)
            {
                case GoodsType.device:
                    Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
                    device_alls.TryGetValue($"{goods_id},0", out var device_record);
                    var device = dmgr.GetDevice(device_record.behavior_script);
                    device.InitData(device_record);
                    obj = device;
                    break;
                case GoodsType.role:
                    roles.TryGetValue(goods_id.ToString(), out var character_record);
                    var character = Safe_Area_Helper.CreateCharacter(character_record);
                    obj = character;
                    break;
                case GoodsType.relic:
                    Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
                    var relic = rmgr.CreateRelic(goods_id.ToString());
                    obj = relic;
                    break;
                case GoodsType.drops:
                    Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr relic_mgr);
                    var drop_relic = relic_mgr.CreateRelic(goods_id.ToString());
                    obj = drop_relic;
                    break;
                default:
                    break;
            }
        }

        public int GetPrice()
        {
            return price_count;
        }

        public int GetPriceAfterDiscount()
        {
            return (int)(price_count * discount);
        }
    }

    public enum GoodsType
    {
        device,
        role,
        relic,
        drops,
    }
}
