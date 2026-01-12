using System.Reflection;

namespace World.Caravans.Mover
{
    public class CaravanMover_Jump
    {
        public static void @do()
        {
            var move_type = typeof(CaravanMover).GetField("m_move_type", BindingFlags.NonPublic | BindingFlags.Static);
            move_type.SetValue(null, EN_caravan_move_type.Flying);
            CaravanMover.move();
        }
    }
}

