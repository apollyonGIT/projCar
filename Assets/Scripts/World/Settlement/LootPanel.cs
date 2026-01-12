using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.BackPack;

namespace World.Settlement
{
    public class LootPanel : MonoBehaviour
    {
        public SingleLootCampCoin prefab;
        public Transform content;

        public SingleCampCoin camp_coin_prefab;
        public Transform camp_coin_content;


        public SingleCampCoin camp_coin_prefab_2;    //营地里有的所有卡普空
        public Transform camp_content;

        public Dictionary<int, int> camp_coins = new Dictionary<int, int>();

        public void Init(bool success)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            var data = bmgr.Settlement(success);

            foreach (var item in data.loots)
            {
                var loot = Instantiate(prefab, content,false);
                loot.Init(item);

                loot.gameObject.SetActive(true);
            }

            foreach(var item in data.loots)
            {
                if (!item.is_broken)
                {
                    foreach (var cp in item.camp_coins)
                    {
                        if (camp_coins.ContainsKey(cp.Item1))
                        {
                            camp_coins[cp.Item1] += cp.Item2;
                        }
                        else
                        {
                            camp_coins[cp.Item1] = cp.Item2;
                        }
                    }
                }
            }

            instantiate_camp_coin_add();

            instantiate_camp_coin_sum();
        }


        private void instantiate_camp_coin_add()
        {
            foreach (var kv in camp_coins)
            {
                var camp_coin = Instantiate(camp_coin_prefab, camp_coin_content, false);
                camp_coin.Init((kv.Key, kv.Value));
                camp_coin.gameObject.SetActive(true);
            }
        }

        private void instantiate_camp_coin_sum()
        {
            foreach(var cc in Commons.CommonContext.instance.camp_coins)
            {
                var camp_coin = Instantiate(camp_coin_prefab_2, camp_content, false);
                camp_coin.Init((cc.Key, cc.Value));
                camp_coin.gameObject.SetActive(true);
            }
        }
    }
}
