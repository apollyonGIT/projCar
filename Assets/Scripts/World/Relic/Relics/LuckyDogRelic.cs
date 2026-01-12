namespace World.Relic.Relics
{
    public class LuckyDogRelic : Relic
    {
        public override void Get()
        {
            BattleContext.instance.drop_loot_delta += 0.8f;
        }
        public override void Drop()
        {
            BattleContext.instance.drop_loot_delta -= 0.8f;
        }
    }
}
