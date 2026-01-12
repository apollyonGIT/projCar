namespace World.Relic.Relics
{
    public class DifficultModeRelic : Relic
    {
        public override void Get()
        {
            BattleContext.instance.enemy_scale_factor += 0.2f;
            BattleContext.instance.enemy_attack_factor += 0.2f;
            BattleContext.instance.enemy_def_factor -= 0.2f;
            BattleContext.instance.enemy_vel_factor += 0.2f;
            BattleContext.instance.drop_loot_delta += 0.2f;
        }

        public override void Drop()
        {
            BattleContext.instance.enemy_scale_factor -= 0.2f;
            BattleContext.instance.enemy_attack_factor -= 0.2f;
            BattleContext.instance.enemy_def_factor += 0.2f;
            BattleContext.instance.enemy_vel_factor -= 0.2f;
            BattleContext.instance.drop_loot_delta -= 0.2f;
        }
    }
}
