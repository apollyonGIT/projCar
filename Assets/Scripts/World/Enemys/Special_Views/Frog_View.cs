using Foundations.MVVM;
using UnityEngine;
using World.Enemys.BT;

namespace World.Enemys
{
    public class Frog_View : MonoBehaviour, IEnemyView
    {
        public LineRenderer line;

        public Enemy owner;
        public BT_Lake_Frog bt;

        //==================================================================================================

        void IModelView<Enemy>.attach(Enemy owner)
        {
            this.owner = owner;
            bt = owner.bt as BT_Lake_Frog;
        }


        void IModelView<Enemy>.detach(Enemy owner)
        {
        }


        void IEnemyView.notify_on_aim(bool ret)
        {
        }


        void IEnemyView.notify_on_change_color(Color color)
        {
        }


        void IEnemyView.notify_on_hurt()
        {
        }


        void IEnemyView.notify_on_outrange_aim(bool ret)
        {
        }


        void IEnemyView.notify_on_pre_aim(bool ret)
        {
        }


        void IEnemyView.notify_on_tick1()
        {
            line.SetPosition(0, new(bt.tongue_start_pos.x, bt.tongue_start_pos.y, 10));
            line.SetPosition(1, new(bt.tongue_end_pos.x, bt.tongue_end_pos.y, 10));
        }
    }
}

