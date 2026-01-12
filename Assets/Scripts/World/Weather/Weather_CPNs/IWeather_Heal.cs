using Foundations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Enemys;
using World.Helpers;

namespace World.Weather.Weathers
{
    public interface IWeather_Heal
    {
        int interval_tick { get; set; }
        int current_tick { get; set; }

        AutoCodes.weather data { get; }

        //==================================================================================================

        void do_heal()
        {
            if (current_tick++ < interval_tick) return;
            current_tick = 0;

            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                var enemy_tags = cell._desc.enemy_tags;
                if (enemy_tags == null) continue;

                var valid_tag = enemy_tags.Intersect(data.heal_tags).Any();
                if (!valid_tag) continue;

                var monster_hot = data.monster_HOT;
                var heal_coef = !cell._desc.is_elite ? monster_hot.Item1 : monster_hot.Item2;

                Hurt_Calc_Helper.try_heal(cell, new Dmg_Data()
                {
                    is_heal = true,
                    heal_power = Mathf.CeilToInt(cell.hp_max * heal_coef)
                });
            }
        }
    }
}

