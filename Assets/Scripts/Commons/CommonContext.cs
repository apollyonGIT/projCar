using Foundations;
using System.Collections.Generic;

namespace Commons
{
    public class CommonContext : Singleton<CommonContext>
    {
        public Dictionary<int, int> camp_coins = new();

        public List<int> tech_opr_list = new();

        public List<string> access_world_list = new();
    }
}

