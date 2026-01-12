using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World.Enemys;

namespace World.Devices
{
    public class Balloon_View : MonoBehaviour
    {
        bool is_alive = true;

        public float pos_offset = 0.75f;

        //==================================================================================================

        public void tick(ITarget target)
        {
            if (!is_alive) return;

            transform.localPosition = target.Position + pos_offset * Vector2.up;
        }


        private void OnDestroy()
        {
            is_alive = false;
        }
    }
}

