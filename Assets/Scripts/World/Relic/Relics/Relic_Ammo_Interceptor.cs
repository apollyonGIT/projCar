using Commons;
using Foundations;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_Interceptor : Relic
    {
        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event += hit_projectile_event;
        }

        public void hit_projectile_event(Projectile p)
        {

        }

        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event -= hit_projectile_event;
        }
    }
}
