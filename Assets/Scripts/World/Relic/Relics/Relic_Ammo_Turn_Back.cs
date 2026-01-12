using Commons;
using Foundations;
using System.Collections.Generic;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_Turn_Back : Relic
    {
        public List<Projectile> triggered_projectiles = new();
        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event += retrace_event;
        }

        public override void Tick()
        {
            for (int i = triggered_projectiles.Count - 1; i >= 0; i--)
            {
                if (triggered_projectiles[i] == null || triggered_projectiles[i].validate == false)
                {
                    triggered_projectiles.RemoveAt(i);
                }
            }
        }

        private void retrace_event(Projectile p)
        {
            if (triggered_projectiles.Contains(p)||p.faction!= WorldEnum.Faction.player)
                return;
            var retrace_tick = desc.parm_int[0];
            if(p.GetLifeTick() > retrace_tick)
            {
                p.velocity *= -1;
                p.direction *= -1;

                triggered_projectiles.Add(p);
            }
        }

        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event -= retrace_event;
        }
    }
}
