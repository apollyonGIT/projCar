using AutoCodes;

namespace World.Characters.CharacterProperties
{
    public class CharacterProperty
    {
        public properties desc;
        public Character owner;

        public CharacterProperty(Character c,properties p)
        {
            desc = p;
            owner = c;
        }

        public virtual void Init()
        {

        }

        public virtual void Start()
        {

        }
    }
}
