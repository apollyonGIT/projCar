using AutoCodes;

namespace World.Characters.CharacterProperties
{
    public class CP_Mood_After_Working : CharacterProperty
    {
        public CP_Mood_After_Working(Character c, properties p) : base(c, p)
        {
        }
        /*
        public override void Init()
        {
            owner.enter_safe_area += EnterSafeArea;
        }


        private void EnterSafeArea()
        {
            if (owner.work_tick / owner.tick_time > desc.float_parms[0])
            {
                owner.UpdateMood(desc.int_parms[0]);
            }
        }
        */
    }
}
