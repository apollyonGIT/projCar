using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Relic.Relics
{
    public class Relic_Muzzle_Blast : Relic
    {
        public override void Get()
        {
            BattleContext.instance.fire_event += knock_off;
        }

        private void knock_off(Transform fire_port, Vector2 fire_direction)
        {
            Vector2 fire_position = fire_port.position;

            var half_angle = desc.parm_int[0];
            var radius = desc.parm_int[1] / 1000f;
            float damage = desc.parm_int[2];
            string dmg_type = desc.parm_string[0];

            var targets = BattleUtility.select_all_target_in_circle(fire_position, radius, WorldEnum.Faction.player, check_angle);

            Attack_Data attack_data = new Attack_Data
            {
                atk = (int)damage,
                diy_atk_str = dmg_type,
            };

            foreach (var target in targets)
            {
                target.hurt(null,attack_data,out var dmg_data);
                target.applied_outlier(null,dmg_type,dmg_data.dmg);
            }

            var vfx = Vfx_Helper.InstantiateVfx(desc.parm_string[0], (int)(desc.parm_int[4] / 1000f * Config.PHYSICS_TICKS_PER_SECOND), fire_port, true);

            bool check_angle(ITarget t)
            {
                Vector2 dir = t.Position - fire_position;

                var angle = Vector2.Angle(dir, fire_direction);
                if (angle <= half_angle)
                {
                    return true;
                }
                return false;
            }
        }
        public override void Drop()
        {
            BattleContext.instance.fire_event -= knock_off;
        }
    }
}
