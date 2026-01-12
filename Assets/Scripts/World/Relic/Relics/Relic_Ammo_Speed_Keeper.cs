using Commons;
using Foundations;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_Speed_Keeper :Relic
    {
        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.rebound_event += speed_keeper_event;
        }

        private void speed_keeper_event(Projectile p)
        {
            if (p.faction != WorldEnum.Faction.player)
                return;
            p.velocity = p.direction * p.init_speed;
        }

        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.rebound_event -= speed_keeper_event;
        }
    }
}
