using AutoCodes;
using Commons;
using System.Collections.Generic;
using UnityEngine;

namespace World.Helpers
{
    public class Hurt_Calc_Helper
    {

        //==================================================================================================

        static Dmg_Data calc_dmg(Attack_Data attack_data, Defense_Data defense_data)
        {
            //特殊规则：如果攻击为绝对落空，直接进入miss
            if (attack_data.is_miss)
                return new()
                {
                    is_miss = true
                };

            //1. 获取攻击者的【攻击】，记为atk，类型为int
            var atk = attack_data.atk;
            if (atk <= 0)
            {
                return new();
            }

            //2. 取受击者的【基础伤害修正系数】，记为ddm_basic
            var ddm_basic = defense_data.k_hurt;
            if (ddm_basic == 0)
            {
                return new()
                {
                    is_immune = true
                };
            }

            if (ddm_basic < 0)
            {
                return new()
                {
                    is_heal = true,
                    heal_power = Mathf.CeilToInt(atk * -ddm_basic)
                };
            }

            //3. 获取受击者的【减伤】，记为def，类型为int
            var def = defense_data.def;
            def = Mathf.CeilToInt(def * (1 - defense_data.fixed_def_cut_rate));

            //4. 获取攻击者的【破甲】，记为ap
            var ap = attack_data.armor_piercing / 1000f;

            //5. 计算受击者在减伤和破甲作用下的【护甲伤害修正系数】
            var D = def - ap;
            var ddm_def = D >= 0 ? 100 / (100 + D) : 1 - 0.01f * D;

            //6. 加总计算受击者在的【综合伤害修正系数】
            var ddm_total = ddm_basic * ddm_def;
            if (ddm_total <= 0)
            {
                return new();
            }

            //7. 获取攻击者的【命中】，记为ts，类型为int
            var ts = attack_data.ts;

            //8. 获取受击者的【闪避】，记为dodge，类型为int
            var dodge = defense_data.dodge;

            //9. 计算本次攻击是否会落空
            var miss = dodge - ts;
            if (Random.Range(0, 100) < miss)
            {
                return new()
                {
                    is_miss = true
                };
            }

            //10. 获取攻击者的【暴击几率】，记为cc
            //   获取攻击者的【暴击倍率】，记为cdr
            var cc = attack_data.critical_chance / 1000f;
            var cdr = attack_data.critical_dmg_rate / 1000f;

            //11. 计算本次攻击是否暴击
            bool is_critical = Random.Range(0f, 1f) < cc;

            //12. 初步计算本次受到的伤害，使其受到受击者【综合伤害修正系数】的修正，将结果记为dmg_0
            var dmg_0 = atk * ddm_total;

            //13. 获取攻击者的【攻击者伤害修正系数】，记为 adm，使其参与伤害修正
            var dmg_1 = dmg_0 * attack_data.adm * Random.Range(0.9f, 1.1f);

            //14. 检查本次是否暴击，并使本次伤害受到暴击效果的修正，记为 dmg_2
            var dmg_2 = Mathf.CeilToInt(dmg_1);

            if (is_critical)
                dmg_2 = Mathf.CeilToInt(dmg_1 * cdr);

            //15. 输出伤害
            Dmg_Data dmg_data = new()
            {
                dmg = dmg_2,
                is_critical = is_critical,
                diy_dmg_str = attack_data.diy_atk_str
            };

            return dmg_data;
        }


        public static void try_heal(ITarget target, Dmg_Data dmg_data)
        {
            if (!dmg_data.is_heal) return;

            var heal_raw = dmg_data.heal_power;
            var heal_power = Mathf.FloorToInt(Mathf.Min(target.hp_max - target.hp, heal_raw));

            dmg_data.heal_power = heal_power;
            target.heal(dmg_data);
        }


        public static Dmg_Data dmg_to_enemy(Attack_Data attack_data, params object[] args)
        {
            var cell = (Enemys.Enemy)args[0];
            var r = cell._desc;

            Defense_Data defense_data = new()
            {
                def = r.def,
                dodge = r.dodge,
                fixed_def_cut_rate = calc_fixed_def_cut_rate(cell),
                k_hurt = calc_k_hurt(attack_data, r.k_hurt)
            };

            var ret = calc_dmg(attack_data, defense_data);
            try_heal(cell, ret);

            return ret;
        }


        public static Dmg_Data dmg_to_caravan(Attack_Data attack_data, params object[] args)
        {
            var cell = (Caravans.Caravan)args[0];
            var r = cell._desc;

            Defense_Data defense_data = new()
            {
                def = r.def,
                fixed_def_cut_rate = calc_fixed_def_cut_rate(cell),
                k_hurt = calc_k_hurt(attack_data, r.k_hurt)
            };

            var ret = calc_dmg(attack_data, defense_data);
            try_heal(cell, ret);

            return ret;
        }


        public static Dmg_Data dmg_to_enemy_caravan(Attack_Data attack_data, params object[] args)
        {
            var cell = (Enemy_Cars.Enemy_Car)args[0];
            var r = cell._caravan_desc;

            Defense_Data defense_data = new()
            {
                def = r.def,
                fixed_def_cut_rate = calc_fixed_def_cut_rate(cell)
            };

            return calc_dmg(attack_data, defense_data);
        }


        public static Dmg_Data dmg_to_device(Attack_Data attack_data, params object[] args)
        {
            var cell = (Devices.Device)args[0];
            var r = cell.desc;

            battle_datas.TryGetValue(r.id.ToString(), out var battle_data);

            Defense_Data defense_data = new()
            {
                def = battle_data.def,
                fixed_def_cut_rate = calc_fixed_def_cut_rate(cell),
                k_hurt = calc_k_hurt(attack_data, cell.battle_data.k_hurt)
            };

            var ret = calc_dmg(attack_data, defense_data);
            try_heal(cell, ret);

            return ret;
        }


        static float calc_fixed_def_cut_rate(ITarget target)
        {
            var def_cut_rate = target.def_cut_rate;
            var ret = def_cut_rate <= 0.5f ? def_cut_rate : 1 - 0.25f / def_cut_rate;

            return ret;
        }


        static float calc_k_hurt(Attack_Data attack_data, Dictionary<string, int> raw)
        {
            var config = Config.current;
            var ret = config.k_hurt_sharp_default;

            var hurt_type = attack_data.diy_atk_str;
            if (string.IsNullOrEmpty(hurt_type)) return ret;

            if (raw == null || !raw.TryGetValue(hurt_type, out var _k_hurt))
            {
                ret = (float)config.GetType().GetField($"k_hurt_{hurt_type}_default").GetValue(config);
                return ret;
            }

            return _k_hurt / 1000f;
        }
    }
}

