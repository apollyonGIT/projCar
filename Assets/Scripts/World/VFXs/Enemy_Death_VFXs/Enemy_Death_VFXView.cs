using Foundations.MVVM;
using UnityEngine;

namespace World.VFXs.Enemy_Death_VFXs
{
    public class Enemy_Death_VFXView : MonoBehaviour, IEnemy_Death_VFXView
    {
        Enemy_Death_VFX owner;

        //==================================================================================================

        void IModelView<Enemy_Death_VFX>.attach(Enemy_Death_VFX owner)
        {
            this.owner = owner;
        }


        void IModelView<Enemy_Death_VFX>.detach(Enemy_Death_VFX owner)
        {
            this.owner = null;

            DestroyImmediate(gameObject);
        }


        void IEnemy_Death_VFXView.notify_on_tick1()
        {
            transform.localPosition = owner.view_pos;
        }
    }
}

