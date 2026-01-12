using Commons;
using Foundations;
using System.Collections.Generic;
using System.Linq;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_Knock_Ball : Relic
    {
        public Dictionary<Projectile,int> knock_projectiles = new();
        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.rebound_event += knock_ball_event;
        }

        private void knock_ball_event(Projectile p)
        {
            if (p.faction != WorldEnum.Faction.player)
                return;
            if (knock_projectiles.ContainsKey(p)&& knock_projectiles[p] >= desc.parm_int[1])
                return;

            if (knock_projectiles.ContainsKey(p))
            {
                knock_projectiles[p]++;
            }
            else
            {
                knock_projectiles.Add(p, 1);
            }
                

            p.scale *=  (1 + desc.parm_int[0] / 1000f);

            for(int i = 0; i < p.attack_datas.Count; i++)
            {
                var atk = p.attack_datas[i];
                atk.atk = (int)(atk.atk * (1 + desc.parm_int[0] / 1000f));

                p.attack_datas[i] = atk;
            }
        }

        public override void Tick()
        {
            var p2remove = knock_projectiles.Keys.Where(k => k.validate == false || k == null).ToList();
            foreach(var p in p2remove)
                knock_projectiles.Remove(p);
        }

        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.rebound_event -= knock_ball_event;
        }
    }
}
