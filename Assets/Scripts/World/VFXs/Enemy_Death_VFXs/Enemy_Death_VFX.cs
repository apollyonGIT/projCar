using Foundations.MVVM;
using UnityEngine;

namespace World.VFXs.Enemy_Death_VFXs
{
    public interface IEnemy_Death_VFXView : IModelView<Enemy_Death_VFX>
    {
        void notify_on_tick1();
    }


    public class Enemy_Death_VFX : Model<Enemy_Death_VFX, IEnemy_Death_VFXView>
    {
        public Vector2 pos;
        public Vector2 view_pos => pos;

        //==================================================================================================

        public Enemy_Death_VFX(params object[] args)
        {
            pos = (Vector2)args[0];
        }


        public void tick1()
        {
            foreach (var view in views)
            {
                view.notify_on_tick1();
            }
        }
    }
}





