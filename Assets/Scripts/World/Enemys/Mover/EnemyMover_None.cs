using UnityEngine;
using World.Helpers;

namespace World.Enemys.Mover
{
    public class EnemyMover_None : MonoBehaviour
    {
        public static void @do(Enemy cell)
        {
            ref var position = ref cell.pos;
            cell.velocity = new();

            position.y = Mathf.Max(position.y, Road_Info_Helper.try_get_altitude(position.x));
        }
    }
}

