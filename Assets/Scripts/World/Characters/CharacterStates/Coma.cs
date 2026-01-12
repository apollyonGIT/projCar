namespace World.Characters.CharacterStates
{
    public class Coma : CharacterState
    {
        public override void trigger()
        {
            duration -= 600;
        }
    }
}
