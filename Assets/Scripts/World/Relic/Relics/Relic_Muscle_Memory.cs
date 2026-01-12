namespace World.Relic.Relics
{
    public class Relic_Muscle_Memory : Relic
    {
        public override void Get()
        {
            BattleContext.instance.coma_can_work = true;
        }

        public override void Drop()
        {
            BattleContext.instance.coma_can_work = false;
        }
    }
}
