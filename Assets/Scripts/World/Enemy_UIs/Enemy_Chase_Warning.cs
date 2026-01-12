using Commons;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace World.Enemy_UIs
{
    public class Enemy_Chase_Warning : MonoBehaviour
    {
        public Image screen_edge_bg;
        Material m_mt;

        public GameObject mark_model;

        public RectTransform mark_area;
        Vector2 m_mark_area_square;

        Dictionary<Enemys.Enemy, (GameObject mark, float randX)> m_chase_marks = new();

        //==================================================================================================

        public void init(Enemys.EnemyMgr mgr)
        {
            gameObject.SetActive(true);
            m_mark_area_square = mark_area.sizeDelta;

            m_mt = screen_edge_bg.material;

            mgr.outter_tick += tick;
        }


        public void tick(Enemys.EnemyMgr mgr)
        {
            if (mgr == null) return;

            var camera = WorldSceneRoot.instance.mainCamera;
            var config = Config.current;

            #region 边缘警告
            var chase_enemy_num = mgr.cells.Where(cell => cell.is_pursuing && !is_in_screen(cell.pos, camera)).Count();
            m_mt.SetFloat("_chase_enemy_num", chase_enemy_num);
            #endregion

            #region 感叹号
            var caravan_pos = WorldContext.instance.caravan_pos;

            var dis_car_edge_left = calc_dis_to_left_screen(caravan_pos, camera);
            if (dis_car_edge_left <= 0) return;

            foreach (var (cell, (mark, randX)) in m_chase_marks)
            {
                //1. 前置计算
                var dis_enemy_car = Mathf.Abs(cell.pos.x - caravan_pos.x);
                var dis_chase_warning_show = config.dis_chase_warning_show;
                var t = (dis_enemy_car - dis_car_edge_left) / (dis_chase_warning_show - dis_car_edge_left);

                //2. 感叹号位置计算
                var width = m_mark_area_square.x;
                var height = m_mark_area_square.y;
                mark.transform.localPosition = new(randX * width, -height / 2 + height * t);

                //3. 感叹号的显隐判断
                mark.SetActive(t >= 0 && t <= 1);
            }
            #endregion
        }


        bool is_in_screen(Vector3 worldPos, Camera camera)
        {
            worldPos.z = 10;
            Vector3 vpos = camera.WorldToViewportPoint(worldPos);

            return vpos.x >= 0 && vpos.x <= 1 && vpos.y >= 0 && vpos.y <= 1;
        }


        float calc_dis_to_left_screen(Vector2 pos, Camera camera)
        {
            var leftEdgeWorld = camera.ScreenToWorldPoint(new(0, 0, 10));

            return pos.x - leftEdgeWorld.x;
        }


        public void add_chase_mark(Enemys.Enemy cell)
        {
            var mark = Instantiate(mark_model, mark_area.transform);
            var randX = Random.Range(-0.5f, 0.5f);
            m_chase_marks.Add(cell, (mark, randX));
        }


        public void remove_chase_mark(Enemys.Enemy cell)
        {
            m_chase_marks.TryGetValue(cell, out var info);
            DestroyImmediate(info.mark);

            m_chase_marks.Remove(cell);
        }
    }
}

