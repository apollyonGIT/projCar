using System.Reflection;
using World.Caravans;

namespace World.Enemy_Cars.Mover
{
    public class Enemy_CarMover_Jump
    {
        public static void @do(Enemy_Car cell)
        {
            var mover = cell.car_mover;
            mover.move_type = EN_caravan_move_type.Flying;

            mover.move(cell);
        }
    }
}

