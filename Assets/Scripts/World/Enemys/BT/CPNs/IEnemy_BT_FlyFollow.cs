using UnityEngine;

namespace World.Enemys.BT
{
    public interface IEnemy_BT_FlyFollow
    {
        public Vector2 flyFollow_pos { get; }

        //==================================================================================================

        void notify_on_tick(Enemy cell, Vector2 target_pos)
        {
            cell.position_expt = target_pos + flyFollow_pos;
        }
    }
}

