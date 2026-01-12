using UnityEngine;
using World.Helpers;

namespace World.Enemys
{
    public class Enemy_Hurt_Helper
    {

        //==================================================================================================

        public static Attack_Data create_attack_data(Enemy cell, string diy_atk_str, int atk)
        {
            Attack_Data data = new()
            {
                atk = atk,
                diy_atk_str = diy_atk_str,
                ts = cell._desc.ts
            };

            //规则：骰子超出命中率，设定为miss
            if (cell.hit_rate < Random.Range(0.0001f, 1f))
                data.is_miss = true;

            return data;
        }


        public static void do_hurt(Enemy cell)
        {
            foreach (var (diy_atk_str, atk) in cell._desc.diy_atk)
            {
                var attack_data = create_attack_data(cell, diy_atk_str, atk);

                cell.target.hurt(cell, attack_data, out var dmg_data);
                cell.target.applied_outlier(cell, attack_data.diy_atk_str, dmg_data.dmg);
            }
            
        }


        public static bool do_aoe_hurt_by_dis(Enemy cell, float aoe_radius)
        {
            var ret = false;

            foreach (var (diy_atk_str, atk) in cell._desc.diy_atk)
            {
                var attack_data = create_attack_data(cell, diy_atk_str, atk);

                foreach (var target in SeekTarget_Helper.player_parts())
                {
                    if (Vector2.Distance(cell.pos, target.Position) <= aoe_radius)
                    {
                        target.hurt(cell, attack_data, out _);
                        ret = true;
                    }
                }
            }

            return ret;
        }


        public static void do_aoe_hurt_by_x(Enemy cell, float aoe_radius)
        {
            foreach (var (diy_atk_str, atk) in cell._desc.diy_atk)
            {
                var attack_data = create_attack_data(cell, diy_atk_str, atk);

                foreach (var target in SeekTarget_Helper.player_parts())
                {
                    if (Mathf.Abs(cell.pos.x - target.Position.x) <= aoe_radius)
                        target.hurt(cell, attack_data, out _);
                }
            }
        }
    }
}

