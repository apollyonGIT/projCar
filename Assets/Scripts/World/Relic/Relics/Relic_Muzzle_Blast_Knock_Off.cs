
using Commons;
using Foundations;
using UnityEngine;
using World.Helpers;
using World.VFXs;

namespace World.Relic.Relics
{
    public class Relic_Muzzle_Blast_Knock_Off : Relic
    {
        public override void Get()
        {
            BattleContext.instance.fire_event += knock_off;
        }

        private void knock_off(Transform fire_port,Vector2 fire_direction)
        {

            var half_angle = desc.parm_int[0];
            var radius = desc.parm_int[1]/1000f;
            float knock_off = desc.parm_int[2];

            var targets = BattleUtility.select_all_target_in_circle(fire_port.position,radius,WorldEnum.Faction.player,check_angle);

            var vfx = Vfx_Helper.InstantiateVfx(desc.parm_string[0], (int)(desc.parm_int[4]/1000f * Config.PHYSICS_TICKS_PER_SECOND), fire_port, true);

            foreach(var target in targets)
            {
                target.impact(WorldEnum.impact_source_type.melee, fire_port.position, BattleUtility.get_target_colllider_pos(target),knock_off);
            }

            bool check_angle(ITarget t)
            {
                Vector2 dir = t.Position - (Vector2)fire_port.position;

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
