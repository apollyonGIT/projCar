using System.Collections.Generic;
using UnityEngine;

namespace World.Business
{
    public class LootBusinessView : MonoBehaviour
    {
        public Transform loot_content;
        public SingleLootView loot_prefab;

        public List<SingleLootView> loot_list = new List<SingleLootView>();

        public Business business;

        public void Init(Business business)
        {
            this.business = business;

            List<int> price_list = new();

            foreach (var goodsdata in business.goods)
            {
                if (price_list.Contains(goodsdata.price_id))
                {
                    continue;
                }
                else
                {
                    price_list.Add(goodsdata.price_id);
                }
            }

            foreach (var price in price_list)
            {
                var slv = Instantiate(loot_prefab, loot_content, false);
                slv.gameObject.SetActive(true);
                slv.Init(price);
                loot_list.Add(slv);
            }
        }

        public void UpdateLoot()
        {
            foreach(var slv in loot_list)
            {
                //slv.Update();
            }
        }
    }
}
