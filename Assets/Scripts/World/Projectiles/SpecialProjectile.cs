using Commons;
using World.Devices;
using static World.WorldEnum;

namespace World.Projectiles
{
    public class SpecialProjectile : Projectile
    {
        public override void tick()
        {
            base.tick();

            var targets = BattleUtility.select_all_target_in_circle(position, desc.explode_radius, faction);
            foreach (var t in targets)
            {
                if(t != null && t.is_interactive)
                {
                    /*Attack_Data attack_data = new()
                    {
                        atk = desc.explode_dmg,
                        ignite = desc.explode_ignite,
                    };
                    t.hurt(this, attack_data, out var dmg_data);*/
                    if (emitter is Device d)
                    {
                        /*BattleContext.instance.ChangeDmg(d, dmg_data.dmg);
                        if (t.hp <= 0)
                        {
                            d.kill_enemy_action?.Invoke(t);
                        }*/
                    }

                    t.impact(impact_source_type.melee, BattleUtility.get_target_colllider_pos(t),position, desc.explode_ft);
                    //吸力而不应该是推力
                    
                    //t.impact(impact_source_type.melee, position, BattleUtility.get_target_colllider_pos(t), desc.explode_ft);
                }
            }
        }

        public override void HitEnemy(ITarget target_hit)
        {
            
        }

        public override void HitGround(float road_height)
        {
            
        }

    }
}
