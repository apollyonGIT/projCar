namespace World.Relic.Relics
{
    public class TitanEnemyRelic : Relic
    {
        public override void Get()
        {
            BattleContext.instance.enemy_scale_factor += 0.2f;
            BattleContext.instance.enemy_mass_factor += 0.2f;
            BattleContext.instance.enemy_hp_factor += 0.2f;
            BattleContext.instance.enemy_vel_factor -= 0.2f;
        }

        public override void Drop()
        {
            BattleContext.instance.enemy_scale_factor -= 0.2f;
            BattleContext.instance.enemy_mass_factor -= 0.2f;
            BattleContext.instance.enemy_hp_factor -= 0.2f;
            BattleContext.instance.enemy_vel_factor += 0.2f;
        }

    }
}
