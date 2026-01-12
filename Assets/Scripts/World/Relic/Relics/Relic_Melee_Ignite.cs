using System.Collections.Generic;
using UnityEngine;
using World.Devices;
using World.Helpers;

namespace World.Relic.Relics
{
    public class Relic_Melee_Ignite : Relic
    {
        public override void Get()
        {
            BattleContext.instance.melee_attack_event += melee_ignite_event;
        }

        private void melee_ignite_event(ITarget source, ITarget target)
        {
            var dmg = desc.parm_int[0];
            var atk_datas = (source as Device).ExecDmg(target, new Dictionary<string, int>() { { "fire", dmg } });
            
            foreach(var atk in atk_datas)
            {
                atk.calc_device_coef(source as IAttack_Device);
                target.hurt(source, atk, out var dmg_data);
                target.applied_outlier(source, atk.diy_atk_str, dmg_data.dmg);

                BattleContext.instance.melee_damage_event?.Invoke(dmg_data,source, target);
            }

            var r = Random.Range(0,1000);
            if (r < desc.parm_int[1])
            {
                source.applied_outlier(source, "fire", (int)(desc.parm_int[2]/1000f));
            }

        }

        public override void Drop()
        {
            BattleContext.instance.melee_attack_event -= melee_ignite_event;
        }
    }
}

