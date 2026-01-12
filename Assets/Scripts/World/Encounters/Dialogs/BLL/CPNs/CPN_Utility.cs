using Commons;
using System.Linq;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class CPN_Utility
    {
        public static bool try_get_loot_id(string pool_loot_id, out uint loot_id)
        {
            loot_id = default;

            AutoCodes.pool_loots.TryGetValue(pool_loot_id, out var r_pool_root);
            var pool_loot_dic = r_pool_root.pool_loot_dic;
            var odds_sum = pool_loot_dic.Select(t => t.Value).Sum();
            var odds = Random.Range(0, odds_sum);

            var current_odds = 0;
            foreach (var (_loot_id, _odds) in pool_loot_dic)
            {
                current_odds += _odds;

                if (odds < current_odds)
                {
                    loot_id = _loot_id;
                    return true;
                }
            }

            Debug.LogError("掉落物ID配置错误，无法通过掉落池ID找到掉落物");
            return false;
        }


        public static bool try_get_loot_name(string loot_id, out string loot_name)
        {
            loot_name = default;
            if (!AutoCodes.loots.TryGetValue(loot_id, out var r_loot)) return false;

            loot_name = Localization_Utility.get_localization(r_loot.name);
            return true;
        }


        public static bool try_get_relic_id(string pool_relic_id, out uint relic_id)
        {
            relic_id = default;

            AutoCodes.pool_relics.TryGetValue(pool_relic_id, out var r_pool_root);
            var pool_relic_dic = r_pool_root.pool_relic_dic;
            var odds_sum = pool_relic_dic.Select(t => t.Value).Sum();
            var odds = Random.Range(0, odds_sum);

            var current_odds = 0;
            foreach (var (_relic_id, _odds) in pool_relic_dic)
            {
                current_odds += _odds;

                if (odds < current_odds)
                {
                    relic_id = _relic_id;
                    return true;
                }
            }

            Debug.LogError("遗物ID配置错误，无法通过掉落池ID找到遗物");
            return false;
        }


        public static void get_replace_strs_quick(string[] args, out string[] replace_strs)
        {
            get_replace_strs(args[0], out replace_strs);
        }


        public static void get_replace_strs(string str, out string[] replace_strs)
        {
            replace_strs = str.Split("//&");
        }


        public static string get_cache_str_value(string ori_str)
        {
            var new_str = ori_str;

            if (ori_str.Contains("@"))
            {
                Encounter_Dialog.instance.cache_dic.TryGetValue(ori_str, out var _new_str_cell);
                new_str = (string)_new_str_cell;
            }

            return new_str;
        }


        public static string get_random_cell(string[] strs)
        {
            var index = Random.Range(0, strs.Length);
            return strs[index];
        }
    }
}

