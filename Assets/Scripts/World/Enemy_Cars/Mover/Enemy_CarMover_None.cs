using Commons;
using System;
using UnityEngine;
using World.Helpers;

namespace World.Enemy_Cars.Mover
{
    public class Enemy_CarMover_None
    {
        public static void @do(Enemy_Car cell)
        {
            var ctx = cell.ctx;

            ref var pos = ref ctx.caravan_pos;
            pos.y = Road_Info_Helper.try_get_altitude(pos.x);
        }
    }
}

