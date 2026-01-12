using Commons;
using Foundations;
using System.Linq;
using UnityEngine;
using World.Enemys;

namespace World.Helpers
{
    public class Enemy_Fini_Helper
    {
        public static void @do(Enemy cell)
        {
            //规则：甩脱时，不会播放音效和掉落物
            if (cell.is_fling_off) return;

            //死亡音效
            Audio.AudioSystem.instance.PlayOneShot(Config.current.SE_monster_die);

            //掉落物
            if (cell._desc.loot_list != null && !cell.is_ban_drop)
            {
                var loot_drop_coef = WorldContext.instance.r_scene.loot_drop_coef; //千分比配置

                foreach (var (drop_loot_id, drop_loot_prob) in cell._desc.loot_list)
                {
                    //var prob = (drop_loot_prob + BattleContext.instance.drop_loot_delta) * 100;
                    //prob = prob * loot_drop_coef / 1000f; //规则：掉落几率受到场景修正

                    //var r = Random.Range(0f, 100f);

                    //if (r < prob)
                    //{
                    //    Drop_Loot_Helper.drop_loot(drop_loot_id, cell.pos, Vector2.zero);
                    //}

                    //实际掉落率
                    var odds = loot_drop_coef * 0.001f * drop_loot_prob;

                    //本次掉落数量
                    var n = 0;
                    if (odds >= 1)
                    {
                        var _odds = Mathf.RoundToInt(odds);
                        n += _odds;
                        odds -= _odds;
                    }

                    var r = Random.Range(0f, 99f) / 100f;
                    if (odds > r)
                    {
                        n += 1;
                    }

                    //掉落
                    if (n == 0) return;
                    for (int i = 0; i < n; i++)
                    {
                        Drop_Loot_Helper.drop_loot(drop_loot_id, cell.pos, Random.insideUnitCircle * 1.5f);
                    }
                }
            }

            //relic
            if (cell.is_drop_relic)
            {
                Drop_Relic_Helper.drop_relic(cell.drop_relic_id);
            }

            //临时规则：结算，记录击杀敌人数（除甩开）
            var ds = Share_DS.instance;
            if (!cell.is_fling_off)
            {
                ds.try_get_value(Game_Mgr.key_kill_enemy_count, out int kill_enemy_count);
                kill_enemy_count++;
                ds.add(Game_Mgr.key_kill_enemy_count, kill_enemy_count);
            }

            //临时规则：结算，记录精英怪击杀数
            if (cell._desc.is_elite)
            {
                ds.try_get_value(Game_Mgr.key_kill_elite_enemy_count, out int kill_elite_enemy_count);
                kill_elite_enemy_count++;
                ds.add(Game_Mgr.key_kill_elite_enemy_count, kill_elite_enemy_count);
            }

            //追兵
            if (cell.is_pursuing)
            {
                WorldSceneRoot.instance.enemy_chase_warning.remove_chase_mark(cell);
            }
        }
    }
}

