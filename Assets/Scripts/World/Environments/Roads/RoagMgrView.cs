using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Environments.Roads
{
    public class RoadMgrView : MonoBehaviour, IRoadMgrView
    {
        [Range(1, 100)]
        public int division = 20;
        public RoadMgr owner;
        public RoadView roadPrefab;
        public List<RoadView> road_views = new();

        void IRoadMgrView.add_main_curve(Curve curve)
        {
            var g = Instantiate(roadPrefab, transform, false);
            g.init(curve);
            g.transform.SetParent(transform);
            var v = curve.sprite_position + curve.start_pos;
            g.transform.localPosition = new Vector3(v.x, v.y, 10);
            g.transform.localScale *= (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / RoadMgr.default_road_z);
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, 1);
            g.gameObject.SetActive(true);
            road_views.Add(g);
        }

        void IRoadMgrView.add_vice_curve(ViceCurve curve)
        {
            var g = Instantiate(roadPrefab, transform, false);
            g.GetComponent<SpriteRenderer>().sortingOrder = -11;
            g.init(curve);
            g.transform.SetParent(transform);
            var v = curve.sprite_position + curve.start_pos;
            g.transform.localPosition = new Vector3(v.x, v.y, 10);
            g.transform.localScale *= (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / RoadMgr.default_road_z);
            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y, 1);
            g.gameObject.SetActive(!curve.hidden);
            road_views.Add(g);
        }

        void IModelView<RoadMgr>.attach(RoadMgr owner)
        {
            this.owner = owner;
        }


        void IModelView<RoadMgr>.detach(RoadMgr owner)
        {
            if (this.owner != null)
            {
                this.owner = null;
            }
            Destroy(gameObject);
        }

        void IRoadMgrView.init()
        {
            
        }

        void IRoadMgrView.remove_main_curve(Curve curve)
        {
            for (int i = road_views.Count - 1; i >= 0; i--)
            {
                if (road_views[i].curve == curve)
                {
                    Destroy(road_views[i].gameObject);
                    road_views.RemoveAt(i);
                }
            }
        }

        void IRoadMgrView.remove_vice_curve(ViceCurve curve)
        {
            for (int i = road_views.Count - 1; i >= 0; i--)
            {
                if (road_views[i].curve == curve)
                {
                    Destroy(road_views[i].gameObject);
                    road_views.RemoveAt(i);
                }
            }
        }

        void IRoadMgrView.reset_curve(float delta)
        {
            foreach (var road_view in road_views)
            {
                var curve = road_view.curve;
                var v = curve.sprite_position + curve.start_pos;
                road_view.transform.localPosition = new Vector3(v.x, v.y, 10);
            }
        }
        private void Update()
        {
#if UNITY_EDITOR
            //foreach (var curve in owner.main_curves)
            //{
            //    drawCurve(curve);
            //}
#endif
        }
        private void drawCurve(Curve curve)
        {
            var sp = curve.start_pos;

            for (int i = 0; i < curve.points.Count - 1; i++)
            {
                var v1 = curve.points[i].position + sp;
                var v2 = curve.points[i].right_position + sp;
                var v3 = curve.points[i + 1].left_position + sp;
                var v4 = curve.points[i + 1].position + sp;
                CubicBezier(new Vector3(v1.x, v1.y, 10f), new Vector3(v2.x, v2.y, 10f), new Vector3(v3.x, v3.y, 10f), new Vector3(v4.x, v4.y, 10f));
            }
        }
        private void CubicBezier(Vector3 start, Vector3 mid1, Vector3 mid2, Vector3 end)
        {
            Vector3 pos1 = start;
            Vector3 pos2;
            int d = 100 / division;
            for (int t = 0; ; t += d)
            {
                if (t >= 100)
                {
                    Debug.DrawLine(pos1, end, Color.green);
                    return;
                }
                pos2 = start * Mathf.Pow(1 - t / 100f, 3) + 3 * mid1 * t / 100f * Mathf.Pow(1 - t / 100f, 2) + 3 * mid2 * t / 100f * t / 100f * (1 - t / 100f) + end * Mathf.Pow(t / 100f, 3);
                Debug.DrawLine(pos1, pos2, Color.green);
                pos1 = pos2;
            }
        }
    }
}

