using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_Division :Relic
    {
        public List<Projectile> triggered_projectiles = new();

        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            switch (desc.parm_string[0])
            {
                case "first_hit":
                    pmgr.hit_enemy_event += division_event;
                    break;
                case "per_kill":
                    pmgr.kill_enemy_event += division_event;
                    break;
                case "on_destroyed":
                    pmgr.destroying_event += division_event_2;
                    break;
                default:
                    break;
            }
        }

        private void division_event(Projectile p)
        {
            if (triggered_projectiles.Contains(p) || p.faction!= WorldEnum.Faction.player)      
                return;

            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);

            var rate = desc.parm_int[0];

            if(rate != -1)
            {
                var rnd = Random.Range(0, 1000);
                if(rate < rnd)
                {
                    triggered_projectiles.Add(p);
                    return;
                }
            }

            division_projectile(p);
        }

        private void division_event_2(Projectile p)
        {
            if (triggered_projectiles.Contains(p))      
                return;
            division_projectile(p,true);
        }

        private void division_projectile(Projectile p,bool reset = false)
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            projectiles.TryGetValue(p.desc.projectile_id.ToString(), out var projectile_record);

            for (int i = 0; i < 2; i++)     //先固定是2次吧
            {
                var new_projectile = BattleUtility.GetProjectile(projectile_record.ammo_type);
                var shoot_dir = Random.insideUnitCircle.normalized;

                var position_offset = Random.insideUnitCircle;

                new_projectile.Init(projectile_record, shoot_dir, p.position + position_offset,Vector2.zero, 0, 0, p.velocity.magnitude, 0, p.faction, p.life_ticks, p.emitter, p.attack_datas, 0);
                pmgr.AddProjectile(new_projectile);

                if (reset)
                {
                    new_projectile.ResetProjectile(new_projectile.velocity,new_projectile.direction,new_projectile.faction,new_projectile.movement_status);
                }


                triggered_projectiles.Add(new_projectile);
            }
            triggered_projectiles.Add(p);
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

        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            switch (desc.parm_string[0])
            {
                case "first_hit":
                    pmgr.hit_enemy_event -= division_event;
                    break;
                case "per_kill":
                    pmgr.kill_enemy_event -= division_event;
                    break;
                case "on_destroyed":
                    pmgr.destroying_event -= division_event_2;
                    break;
                default:
                    break;
            }
        }
    }
}
