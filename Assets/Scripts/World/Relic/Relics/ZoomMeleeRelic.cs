namespace World.Relic.Relics
{
    public class ZoomMeleeRelic :Relic
    {
        public override void Get()
        {
            BattleContext.instance.melee_scale_factor += 0.2f;
        }

        public override void Drop()
        {
            BattleContext.instance.melee_scale_factor -= 0.2f;
        }
    }
}
