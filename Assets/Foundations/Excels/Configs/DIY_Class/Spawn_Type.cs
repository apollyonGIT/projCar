
using System;

namespace Foundations.Excels
{
    public class Spawn_Type
    {
        public enum EN_Spawn_Type
        {
            RndPosition,
            FixedDistance,
            SubsceneRes
        }

        public EN_Spawn_Type value;


        public Spawn_Type()
        {
        }

        public Spawn_Type(object raw)
        {
            var str = (string)raw;
            value = (EN_Spawn_Type)Enum.Parse(typeof(EN_Spawn_Type), str);
        }

    }

}


