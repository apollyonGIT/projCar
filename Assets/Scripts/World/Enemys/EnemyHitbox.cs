using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.Enemys
{
    public class EnemyHitbox : MonoBehaviour
    {
        public EnemyView view;

        //==================================================================================================

        private void OnEnable()
        {
            view.tick1_outter += attach_collider_pos;
        }


        private void OnDisable()
        {
            view.tick1_outter -= attach_collider_pos;
        }


        void attach_collider_pos()
        {
            view.owner.collider_pos = transform.position;
        }
    }
}

