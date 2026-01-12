using Commons;
using Foundations.Saves;
using System.Collections.Generic;
using UnityEngine;

namespace World.Helpers
{
    public class SL_Helper
    {
        public static void save_game()
        {
            Dictionary<string, object> save_dic = new()
            {
                { "t1", 4444f },
                { "t2", 555u }
            };

            Save_Helper.save(Config.current.save_00_path, save_dic);
        }


        public static void load_game()
        {
            Save_Helper.load(Config.current.save_00_path, out Dictionary<string, object> save_dic);

            Debug.Log(save_dic["t1"]);
            Debug.Log(save_dic["t2"]);
        }
    }
}

