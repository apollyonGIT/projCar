using UnityEngine;
using World.Devices;
using World.Helpers;

namespace World.Relic.Relics
{
    public class Relic_Melee_Assassinate : Relic
    {
        public override void Get()
        {
            BattleContext.instance.melee_damage_event += melee_assassinate_event;
        }

        private void melee_assassinate_event(Dmg_Data data, ITarget source, ITarget target)
        {
            if (data.is_critical)
            {
                var pro = desc.parm_int[0];
                var r = Random.Range(0, 1000);
                if (r <= pro)
                {
                    if (target.hp < (source as Device).desc.basic_damage * desc.parm_int[0] / 1000f)
                    {
                        target.hurt(null, new Attack_Data()
                        {
                            atk = 99999,
                            armor_piercing = 1000,
                            ts = 1000,
                        }, out var _);
                    }
                }
            }
        }

        public override void Drop()
        {
            BattleContext.instance.melee_damage_event -= melee_assassinate_event;
        }
    }
}

