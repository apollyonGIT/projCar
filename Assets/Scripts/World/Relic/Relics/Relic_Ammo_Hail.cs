using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_Hail : Relic
    {
        public List<Projectile> triggered_projectiles = new();

        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event += add_event;
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


        private void add_event(Projectile p)
        {
            if (triggered_projectiles.Contains(p) || p.faction != WorldEnum.Faction.player)      //这里应该加一个 确认是冰雹就直接return
                return;

            var relic_upgrade_id = desc.parm_int[3];
            Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
            var ret = rmgr.relic_list.Any(r => r.desc.id == relic_upgrade_id);

            if (ret)
            {
                if(p.position.x >= WorldContext.instance.caravan_pos.x + 12.8f || p.position.x <= WorldContext.instance.caravan_pos.x - 12.8f)
                {
                    Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
                    projectiles.TryGetValue($"{desc.parm_int[0]}", out var projectile_record);

                    var salvo = desc.parm_int[1];
                    var half_angle = desc.parm_int[2];

                    for (int i = 0; i < salvo; i++)
                    {
                        var angle = 2 * half_angle;
                        var ave_a = angle / salvo;
                        var angle_1 = -half_angle + (salvo - i - 1) * ave_a;
                        var angle_2 = half_angle - i * ave_a;

                        var new_projectile = BattleUtility.GetProjectile(projectile_record.ammo_type);

                        var shoot_dir = p.position.x >= WorldContext.instance.caravan_pos.x + 12.8f?  Vector2.left:Vector2.right;

                        var position_offset = Random.insideUnitCircle;

                        new_projectile.Init(projectile_record, shoot_dir, p.position + position_offset, Vector2.zero, angle_1, angle_2, p.init_speed, p.init_speed, p.faction, 999, p.emitter, p.attack_datas, 0);
                        pmgr.AddProjectile(new_projectile);

                        triggered_projectiles.Add(new_projectile);
                    }
                    triggered_projectiles.Add(p);
                }
            }

            if (p.position.y >= 12.8)
            {
                Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
                projectiles.TryGetValue($"{desc.parm_int[0]}", out var projectile_record);

                var salvo = desc.parm_int[1];
                var half_angle = desc.parm_int[2];

                for (int i = 0; i < salvo; i++)
                {
                    var angle = 2 * half_angle;
                    var ave_a = angle / salvo;
                    var angle_1 = -half_angle + (salvo - i - 1) * ave_a;
                    var angle_2 = half_angle - i * ave_a;

                    var new_projectile = BattleUtility.GetProjectile(projectile_record.ammo_type);
                    var shoot_dir = Vector3.down;

                    var position_offset = Random.insideUnitCircle;

                    new_projectile.Init(projectile_record, shoot_dir, p.position + position_offset, Vector2.zero, angle_1, angle_2, p.init_speed, p.init_speed, p.faction, 999, p.emitter, p.attack_datas, 0);
                    pmgr.AddProjectile(new_projectile);

                    triggered_projectiles.Add(new_projectile);
                }
                triggered_projectiles.Add(p);
            }
        }

        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event -= add_event;
        }
    }
}
