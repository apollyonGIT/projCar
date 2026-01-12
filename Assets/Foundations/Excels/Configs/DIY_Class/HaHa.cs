
namespace Foundations.Excels
{
    public class HaHa
    {
        public uint id;
        public string desc;


        public HaHa()
        { 
        }

        public HaHa(object raw)
        {
            var strs = ((string)raw).Split(',');

            id = uint.Parse(strs[0]);
            desc = strs[1];
        }

    }
}

