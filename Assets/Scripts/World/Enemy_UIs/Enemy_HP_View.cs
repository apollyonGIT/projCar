using UnityEngine;
using World.Enemy_Cars;
using World.Enemys;

namespace World.Enemy_UIs
{
    public class Enemy_HP_View : MonoBehaviour
    {
        public SpriteRenderer current_hp;

        const float max_length = 20;        

        //==================================================================================================

        public void init(Enemy cell)
        {
            cell.outter_tick += tick;
            cell.outter_fini += fini;
        }


        public void tick(object[] prms)
        {
            if (prms == null) return;
            if (prms[0] is not Enemy_Car cell) return;

            var ctx = cell.ctx;
            var pos = ctx.caravan_pos;

            pos.y += 2.5f;
            transform.localPosition = pos;

            //赋值血量
            var size = current_hp.size;
            size.x = calc_hp_length((float)cell.hp / (float)cell.hp_max);
            current_hp.size = size;
        }


        public void fini(object[] prms)
        {
            if (prms == null) return;
            if (prms[0] is not Enemy_Car cell) return;

            cell.outter_tick -= tick;
            cell.outter_fini -= fini;

            DestroyImmediate(gameObject);
        }


        float calc_hp_length(float hp_ratio)
        {
            return hp_ratio * max_length;
        }
    }
}

