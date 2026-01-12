
using System;

namespace Foundations.Excels
{
    public class Site_Type
    {
        public enum EN_Site_Type
        {
            Dialog = 0,
            Monster,
            Loot
        }

        public EN_Site_Type value;


        public Site_Type()
        {
        }

        public Site_Type(object raw)
        {
            var str = (string)raw;
            value = (EN_Site_Type)Enum.Parse(typeof(EN_Site_Type), str);
        }

    }

}


