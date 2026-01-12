
using System;

namespace Foundations.Excels
{
    public class Slot_Type
    {
        public enum EN_Slot_Type
        {
            顶部,
            底部,
            前,
            后,
            后上,
            前上,
            车轮,
            核心
        }

        public EN_Slot_Type value;


        public Slot_Type()
        {
        }

        public Slot_Type(object raw)
        {
            var str = (string)raw;
            value = (EN_Slot_Type)Enum.Parse(typeof(EN_Slot_Type), str);
        }

    }

}


