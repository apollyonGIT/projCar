using Commons;
using System;
using UnityEngine;
using World.Helpers;

namespace World.Caravans.Mover
{
    public class CaravanMover_None
    {
        public static void @do()
        {
            var mgr = CaravanMover.mgr;
            var ctx = mgr.ctx;

            ref var pos = ref ctx.caravan_pos;
            pos.y = Road_Info_Helper.try_get_altitude(pos.x);
        }
    }
}

