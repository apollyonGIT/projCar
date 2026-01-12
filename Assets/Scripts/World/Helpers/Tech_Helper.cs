using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace World.Helpers
{
    public class Tech_Helper
    {
        public static void load_techs()
        {
            var tech_opr_list = Commons.CommonContext.instance.tech_opr_list;
            foreach (var tech_id in tech_opr_list)
            {
                AutoCodes.techs.TryGetValue($"{tech_id}", out var r_tech);

                List<object> prms = new();
                foreach (var e in r_tech.int_parms)
                {
                    prms.Add(e);
                }
                Assembly.Load("World").GetType($"World.Techs.{r_tech.script}")?.GetMethod("do").Invoke(null, prms.ToArray());
            }
        }
    }
}

