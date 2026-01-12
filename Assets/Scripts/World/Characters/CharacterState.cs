using System;

namespace World.Characters
{
    public class CharacterState
    {
        public int duration;
        public Character owner;

        public virtual void start()
        {

        }

        public virtual void end()
        {

        }

        public virtual void tick()
        {

        }

        public virtual void trigger()
        {
            
        }
    }
}
