
using System;

namespace Foundations.Excels
{
    public class Shop_Type
    {
        public enum EN_Shop_Type
        {
            设备 = 1,
            角色 = 2,
            遗物掉落 = 3,
            遗物商人 = 4,
        }

        public EN_Shop_Type value;


        public Shop_Type()
        {
        }

        public Shop_Type(object raw)
        {
            var str = (string)raw;
            value = (EN_Shop_Type)Enum.Parse(typeof(EN_Shop_Type), str);
        }

    }

}


