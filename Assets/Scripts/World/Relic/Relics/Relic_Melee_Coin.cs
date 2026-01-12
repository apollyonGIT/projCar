using UnityEngine;
using World.Helpers;

namespace World.Relic.Relics
{
    public class Relic_Melee_Coin : Relic
    {
        public override void Get()
        {
            BattleContext.instance.melee_damage_event += melee_coin_event;
        }

        private void melee_coin_event(Dmg_Data data, ITarget source, ITarget target)
        {
            if (data.is_critical)
            {
                var pro = desc.parm_int[0];
                var r = Random.Range(0, 1000);
                if(r<= pro)
                {
                    var dir = target.Position - source.Position;
                    var velocity = dir.normalized * Random.Range(1, 3f);
                    Drop_Loot_Helper.drop_loot((uint)desc.parm_int[1], target.Position, velocity);
                }
            }
        }

        public override void Drop()
        {
            BattleContext.instance.melee_damage_event -= melee_coin_event;
        }
    }
}
