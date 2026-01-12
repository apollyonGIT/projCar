using System.Collections.Generic;
using UnityEngine;
using World.Enemy_Cars;
using World.Enemys;

namespace World.Enemy_UIs
{
    public class Enemy_Elite_HP_View : MonoBehaviour
    {
        public SpriteRenderer current_hp;

        public Transform clips;
        public GameObject clip_model;
        public float interval;

        const float max_length = 20;

        int m_max_clip_count;
        int m_sub_hp;

        LinkedList<GameObject> clip_list = new();

        //==================================================================================================

        public void init(Enemy cell, int max_clip_count, int sub_hp)
        {
            cell.outter_tick += tick;
            cell.outter_fini += fini;

            m_max_clip_count = max_clip_count;
            m_sub_hp = sub_hp;

            for (int i = 0; i < m_max_clip_count; i++)
            {
                var clip = Instantiate(clip_model, clips);
                clip.transform.localPosition = new(i * interval, 0);
                clip.SetActive(true);

                clip_list.AddLast(clip);
            }
        }


        public void tick(object[] prms)
        {
            if (prms == null) return;
            if (prms[0] is not Enemy cell) return;

            var pos = cell.pos;

            pos.y += 2.5f;
            transform.localPosition = pos;

            //赋值血量
            var hp = cell.hp;

            var sub_remain_hp = hp - Mathf.FloorToInt(hp / (float)m_sub_hp) * m_sub_hp;
            sub_remain_hp = sub_remain_hp == 0 ? m_sub_hp : sub_remain_hp;

            //临时规则：如果为最大血量，启用修正
            if (hp == cell.hp_max)
                sub_remain_hp = m_sub_hp;

            var remain_clip_count = Mathf.CeilToInt(hp / (float)m_sub_hp);
            var del_clip_count = clip_list.Count - remain_clip_count;

            if (del_clip_count > 0)
            {
                for (int i = 0; i < del_clip_count; i++)
                {
                    del_clip();
                }
            }
            else if (del_clip_count < 0)
            {
                for (int i = 0; i < -del_clip_count; i++)
                {
                    add_clip();
                }
            }

            var size = current_hp.size;
            size.x = calc_hp_length(sub_remain_hp / (float)m_sub_hp);
            current_hp.size = size;
        }


        public void fini(object[] prms)
        {
            if (prms == null) return;
            if (prms[0] is not Enemy cell) return;

            cell.outter_tick -= tick;
            cell.outter_fini -= fini;

            DestroyImmediate(gameObject);
        }


        float calc_hp_length(float hp_ratio)
        {
            return hp_ratio * max_length;
        }


        void del_clip()
        {
            var t = clip_list.Last.Value;
            t.SetActive(false);

            clip_list.RemoveLast();
        }


        void add_clip()
        {
            if (clip_list.Count == m_max_clip_count) return;

            var clip = Instantiate(clip_model, clips);
            clip.transform.localPosition = new(clip_list.Count * interval, 0);
            clip.SetActive(true);

            clip_list.AddLast(clip);
        }
    }
}

