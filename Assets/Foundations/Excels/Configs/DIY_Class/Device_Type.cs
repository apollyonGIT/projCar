
using System;

namespace Foundations.Excels
{
    public class Device_Type
    {
        public enum EN_Device_Type
        {
            Core = 1,
            Wheel,
            Common
        }

        public EN_Device_Type value;


        public Device_Type()
        {
        }

        public Device_Type(object raw)
        {
            var str = (string)raw;
            value = (EN_Device_Type)Enum.Parse(typeof(EN_Device_Type), str);
        }

    }

}


