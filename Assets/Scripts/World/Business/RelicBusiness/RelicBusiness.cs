using AutoCodes;
using Commons;
using Foundations;
using Foundations.Excels;
using System.Collections.Generic;
using UnityEngine;
using World.Helpers;
using World.Relic;

namespace World.Business
{
    public class RelicBusiness : Business
    {
        public int refresh_cnt = 0;

        public shop record;

        //relic需要把商店记录
        public override void Init(uint business_id)
        {
            shops.TryGetValue(business_id.ToString(), out record);
            base.Init(business_id);
        }


        public bool Refresh()
        {
            var coin_id = (int)Config.current.coin_id;
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);

            scene_index = scene_index + 1;

            var refresh_cost = Mathf.Ceil(Config.current.shop_relic_f5_cost_base + Config.current.shop_relic_f5_cost_scene_parm * scene_index) * refresh_cnt;

            if (refresh_cost <= Safe_Area_Helper.GetLootCount(coin_id))
            {
                Safe_Area_Helper.SpendLootCount(coin_id, (int)refresh_cost);
                refresh_cnt++;

                goods.Clear();
                Reinit();
            }

            return false;
        }

        public void Reinit()
        {
            var total_weight = 0;

            var shop_count = Random.Range(record.goods_count_rnd.Item1, record.goods_count_rnd.Item2 + 1);

            List<(uint, int)> goods_record_list = new();

            if (record.shop_type.value == Shop_Type.EN_Shop_Type.遗物商人 || record.shop_type.value == Shop_Type.EN_Shop_Type.遗物掉落)
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
                                if (rnd_dicount < discount_probability && current_discount_count < max_discount_count)
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

        public override void GetGood(GoodsData data)
        {
            Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
            rmgr.CreateRelicAndAdd((data.obj as Relic.Relic).desc.id.ToString());
        }

        public int GetRefreshCost()
        {
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);

            scene_index = scene_index + 1;

            return (int)(Mathf.Ceil(Config.current.shop_relic_f5_cost_base + Config.current.shop_relic_f5_cost_scene_parm * scene_index) * refresh_cnt);
        }
    }
}
