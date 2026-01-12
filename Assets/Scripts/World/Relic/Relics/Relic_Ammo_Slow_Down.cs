using Commons;
using Foundations;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_Slow_Down : Relic
    {
        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event += slow_down_event;
        }

        private void slow_down_event(Projectile p)
        {

            switch (desc.parm_string[0])
            {
                case "all":
                    p.velocity *= desc.parm_int[0]/1000f;
                    break;
                case "player":
                    if(p.faction == WorldEnum.Faction.player)
                        p.velocity *= desc.parm_int[0] / 1000f;
                    break;
                default:
                    break;
            }
            
        }

        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event -= slow_down_event;
        }
    }
}
