using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.BackPack;
using World.Loots;
using World.Relic;

namespace World.Helpers
{
    public class Drop_Loot_Helper
    {
        public static void drop_loot(uint loot_id, Vector2 pos, Vector2 init_velocity)
        {
            loots.TryGetValue(loot_id.ToString(), out var loot);

            Mission.instance.try_get_mgr("Loot", out LootMgr lmgr);
            lmgr.InstantiateLoot(new Loot()
            {
                pos = pos,
                velocity = init_velocity,
                desc = loot,
            });
        }


        public static void drop_loot(uint loot_id)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            bmgr.AddLoot(loot_id);
        }
    }


    public class Drop_Relic_Helper
    {
        public static void drop_relic(uint relic_id)
        {
            Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr mgr);
            mgr.CreateRelicAndAdd($"{relic_id}");
        }

        public static void drop_relic_by_pool(uint pool_id,int cnt)
        {
            List<uint> results = new();
            List<(uint,int)> relics_in_pool = new();
            Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr mgr);

            int total_weight = 0;

            foreach (var record in relic_pools.records)
            {
                if (record.Key == pool_id.ToString())
                {
                    if (mgr.relic_museum.CheckRelic(record.Value.parm_int))
                    {
                        var rec = record.Value;
                        relics_in_pool.Add((rec.parm_int,rec.weight));
                    }
                }
            }

            for(int i = 0; i < cnt; i++)
            {
                total_weight = 0;

                if (relics_in_pool.Count == 0)
                    break;

                for(int j = 0; j < relics_in_pool.Count; j++)
                {
                    total_weight += relics_in_pool[j].Item2;
                }

                int random_value = UnityEngine.Random.Range(0, total_weight);
                int current_weight = 0;

                for(int j = 0; j < relics_in_pool.Count; j++)
                {
                    current_weight += relics_in_pool[j].Item2;
                    if (random_value < current_weight)
                    {
                        results.Add(relics_in_pool[j].Item1);
                        relics_in_pool.RemoveAt(j);
                        break;
                    }
                }
            }

            if (results.Count > 0)
            {
                Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
                rmgr.InstantiateRelicStore(results);
            }
        }

        public static bool CheckRelic(uint relic_id)
        {
            Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr mgr);
            return mgr.relic_museum.CheckRelic(relic_id);
        }
    }
}
