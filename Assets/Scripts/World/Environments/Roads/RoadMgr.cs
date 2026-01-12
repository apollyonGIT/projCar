using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;
using AutoCodes;
using Commons;
using Addrs;
using System.Linq;

namespace World.Environments.Roads
{
    public interface IRoadMgrView : IModelView<RoadMgr>
    {
        void init();
        void add_main_curve(Curve curve);
        void add_vice_curve(ViceCurve curve);
        void remove_main_curve(Curve curve);
        void remove_vice_curve(ViceCurve curve);
        void reset_curve(float delta);
    }


    public class RoadMgr : Model<RoadMgr, IRoadMgrView>
    {
        public const float default_road_z = 10;
        public Vector2 main_end_pos;
        public Vector2 vice_end_pos;
        public List<Curve> main_curves = new();
        public List<ViceCurve> vice_curves = new();
        public List<scene_road> main_road_records = new();
        public List<scene_road> vice_road_records = new();

        public Dictionary<uint, float> generate_distance = new();            //记录每种路段最新的x

        private bool need_vice = false;
        private (float, float) main_h, vice_h;

        public void Init(uint scene_id, EnvironmentMgr owner)
        {
            scenes.TryGetValue(scene_id.ToString(), out var record);
            var group_id = record.scene_road;

            main_h = (group_id[0].Item3, group_id[0].Item4);

            if (group_id.Count > 1)
            {
                need_vice = true;
                vice_h = (group_id[1].Item3, group_id[1].Item4);
            }

            foreach (var road in scene_roads.records)
            {
                if (road.Value.group_id == group_id[0].Item1)           //记录合理的路面数据到路面随机池内
                {
                    main_road_records.Add(road.Value);
                }

                if (need_vice && road.Value.group_id == group_id[1].Item1)
                {
                    vice_road_records.Add(road.Value);
                }
            }

            main_end_pos = owner.focus_pos - new Vector2(owner.dis, 0) * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z) + new Vector2(0, group_id[0].Item2);
            main_end_pos.y = group_id[0].Item2;

            while (main_end_pos.x < 0)
            {
                add_main_curve();
            }

            if (need_vice)
            {
                vice_end_pos = owner.focus_pos - new Vector2(owner.dis, 0) * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z) + new Vector2(0, group_id[1].Item2);
                while (vice_end_pos.x < 0)
                {
                    add_vice_curve();
                }
            }

            foreach (var view in views)
            {
                view.init();
            }
        }
        public void Reinit(uint scene_id, EnvironmentMgr owner)
        {
            foreach (var curve in main_curves)
            {
                foreach (var view in views)
                {
                    view.remove_main_curve(curve);
                }
            }
            main_curves.Clear();
            main_road_records.Clear();

            if (need_vice)          //如果原本有副相关
            {
                foreach (var curve in vice_curves)
                {
                    foreach (var view in views)
                    {
                        view.remove_vice_curve(curve);
                    }
                }
                vice_curves.Clear();
                vice_road_records.Clear();
            }

            scenes.TryGetValue(scene_id.ToString(), out var rec);
            var group_id = rec.scene_road;

            main_h = (group_id[0].Item3, group_id[0].Item4);

            if (group_id.Count > 1)
            {
                need_vice = true;
                vice_h = (group_id[1].Item3, group_id[1].Item4);
            }

            foreach (var (_, record) in scene_roads.records)
            {
                if (record.group_id == group_id[0].Item1)
                {
                    main_road_records.Add(record);
                }

                if (need_vice && record.group_id == group_id[1].Item1)
                {
                    vice_road_records.Add(record);
                }
            }

            var pre_main_pos = main_end_pos;
            main_end_pos = -new Vector2(owner.dis, 0) * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z);
            main_end_pos.y = group_id[0].Item2;

            while (main_end_pos.x < pre_main_pos.x)
            {
                add_main_curve();
            }

            if (need_vice)
            {
                var pre_vice_pos = vice_end_pos;
                vice_end_pos = owner.focus_pos - new Vector2(owner.dis, 0) * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z) + new Vector2(0, group_id[1].Item2);
                while (vice_end_pos.x < pre_vice_pos.x)
                {
                    add_vice_curve();
                }
            }
        }
        public void ResetRoad(float delta)
        {
            main_end_pos.x -= delta;
            foreach (var curve in main_curves)
            {
                curve.start_pos.x -= delta;
            }

            vice_end_pos.x -= delta;
            foreach (var curve in vice_curves)
            {
                curve.start_pos.x -= delta;
            }

            for (int i = 0; i < generate_distance.Keys.Count; i++)
            {
                var key = generate_distance.Keys.ElementAt(i);
                generate_distance[key] -= delta;
            }

            foreach (var view in views)
            {
                view.reset_curve(delta);
            }
        }
        public void AddRoad(Vector2 focus_pos, float dis)
        {
            if ((main_end_pos.x - focus_pos.x) <= dis * 2)
            {
                add_main_curve();
            }

            if (need_vice)
            {
                if ((vice_end_pos.x - focus_pos.x) <= dis)
                {
                    add_vice_curve();
                }
            }
        }
        public void RemoveRoad(Vector2 focus_pos, float dis)
        {
            if (Config.current.free_camera)
                return;
            List<Curve> remove_curves = new();
            foreach (var curve in main_curves)
            {
                if (focus_pos.x - (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x) > dis)
                {
                    remove_curves.Add(curve);
                }
            }
            foreach (var curve in remove_curves)
            {
                main_curves.Remove(curve);

                foreach (var view in views)
                {
                    view.remove_main_curve(curve);
                }
            }

            remove_curves.Clear();

            if (need_vice)
            {
                foreach (var curve in vice_curves)
                {
                    if (focus_pos.x - (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x) > dis)
                    {
                        remove_curves.Add(curve);
                    }
                }

                foreach (var curve in remove_curves)
                {
                    vice_curves.Remove((ViceCurve)curve);

                    foreach (var view in views)
                    {
                        view.remove_vice_curve((ViceCurve)curve);
                    }
                }
            }
        }
        private scene_road get_random_main_record()
        {
            if (main_road_records.Count == 0)
            {
                return null;
            }
            int sum_weight = 0;
            foreach (var mr_record in main_road_records)
            {
                sum_weight += mr_record.generate_weight;
            }

            int rnd_weight = Random.Range(0, sum_weight);

            foreach (var mr_reocrd in main_road_records)
            {
                if (rnd_weight < mr_reocrd.generate_weight)
                {
                    return mr_reocrd;
                }
                else
                {
                    rnd_weight -= mr_reocrd.generate_weight;
                }
            }

            return null;
        }
        private scene_road get_random_vice_record()
        {
            if (vice_road_records.Count == 0 || !need_vice)
            {
                return null;
            }

            int index = Random.Range(0, vice_road_records.Count);

            return vice_road_records[index];
        }
        private void add_main_curve()
        {
            var rc = get_random_main_record();
            if (rc == null)
                return;
            Addressable_Utility.try_load_asset<RoadData>($"{rc.road_data_path}", out var road_data);
            if (road_data == null)
            {
                Debug.LogError("获取road数据失败");
            }

            if (generate_distance.ContainsKey(rc.sub_id) && main_end_pos.x - generate_distance[rc.sub_id] < rc.generate_distance)
            {
                add_main_curve();
                return;
            }

            var h = main_end_pos.y + road_data.points[road_data.points.Count - 1].position.y * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z);
            if (h < main_h.Item1 || h > main_h.Item2)
            {
                add_main_curve();                //此处有一个注意问题,如果所有的路面都不能满足高度差需求的话,游戏会卡死
                return;
            }

            if (generate_distance.ContainsKey(rc.sub_id))
            {
                generate_distance[rc.sub_id] = main_end_pos.x;
            }
            else
            {
                generate_distance.Add(rc.sub_id, main_end_pos.x);
            }

            var curve = new Curve()
            {
                start_pos = main_end_pos,
                points = road_data.mul(1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z),
                curve_sprite = road_data.road_sprite,
                sprite_position = road_data.road_sprite_position,
            };

            main_curves.Add(curve);

            main_end_pos += road_data.points[road_data.points.Count - 1].position * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z);           //把路段的最后位置后移

            foreach (var view in views)
            {
                view.add_main_curve(curve);
            }
        }
        private void add_vice_curve()
        {
            var rc = get_random_vice_record();
            if (rc == null)
                return;
            Addressable_Utility.try_load_asset<RoadData>($"{rc.road_data_path}", out var road_data);
            if (road_data == null)
            {
                Debug.LogError("获取road数据失败");
            }
            var h = vice_end_pos.y + road_data.points[road_data.points.Count - 1].position.y * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z);
            if (h < vice_h.Item1 || h > vice_h.Item2)
            {
                add_vice_curve();                //此处有一个注意问题,如果所有的路面都不能满足高度差需求的话,游戏会卡死
                return;
            }
            var curve = new ViceCurve()
            {
                start_pos = vice_end_pos,
                points = road_data.mul(1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z),
                curve_sprite = road_data.road_sprite,
                sprite_position = road_data.road_sprite_position,
                hidden = rc.hidden,
            };
            vice_curves.Add(curve);

            vice_end_pos += road_data.points[road_data.points.Count - 1].position * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / default_road_z);           //把路段的最后位置后移

            foreach (var view in views)
            {
                view.add_vice_curve(curve);
            }
        }
        /// <summary>
        /// 输入当前的x,输出这个x的主路段的y
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float road_height(float x, int road_type = 0)
        {
            if (road_type == 0)
            {
                foreach (var curve in main_curves)
                {
                    if (x >= curve.start_pos.x && x <= (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x))
                    {
                        for (int i = 0; i < curve.points.Count - 1; i++)
                        {
                            var p1 = curve.points[i].position + curve.start_pos;
                            var p2 = curve.points[i].right_position + curve.start_pos;
                            var p3 = curve.points[i + 1].left_position + curve.start_pos;
                            var p4 = curve.points[i + 1].position + curve.start_pos;
                            if (x >= p1.x && x <= p4.x)
                            {
                                var t = (x - p1.x) / (p4.x - p1.x);
                                var y = Mathf.Pow((1 - t), 3) * p1.y + 3 * Mathf.Pow((1 - t), 2) * t * p2.y + 3 * (1 - t) * t * t * p3.y + Mathf.Pow(t, 3) * p4.y;
                                return y;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var curve in vice_curves)
                {
                    if (x >= curve.start_pos.x && x <= (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x))
                    {
                        for (int i = 0; i < curve.points.Count - 1; i++)
                        {
                            var p1 = curve.points[i].position + curve.start_pos;
                            var p2 = curve.points[i].right_position + curve.start_pos;
                            var p3 = curve.points[i + 1].left_position + curve.start_pos;
                            var p4 = curve.points[i + 1].position + curve.start_pos;
                            if (x >= p1.x && x <= p4.x)
                            {
                                var t = (x - p1.x) / (p4.x - p1.x);
                                var y = Mathf.Pow((1 - t), 3) * p1.y + 3 * Mathf.Pow((1 - t), 2) * t * p2.y + 3 * (1 - t) * t * t * p3.y + Mathf.Pow(t, 3) * p4.y;
                                return y;
                            }
                        }
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 返回x位置的主路段的斜率
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float road_slope(float x, int road_type = 0)
        {
            if (road_type == 0)
            {
                foreach (var curve in main_curves)
                {
                    if (x >= curve.start_pos.x && x <= (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x))
                    {
                        for (int i = 0; i < curve.points.Count - 1; i++)
                        {
                            var p1 = curve.points[i].position + curve.start_pos;
                            var p2 = curve.points[i].right_position + curve.start_pos;
                            var p3 = curve.points[i + 1].left_position + curve.start_pos;
                            var p4 = curve.points[i + 1].position + curve.start_pos;
                            if (x >= p1.x && x <= p4.x)
                            {
                                var t = (x - p1.x) / (p4.x - p1.x);
                                var s = 1 - t;
                                var y1 = -3 * Mathf.Pow(s, 2) * p1.y + 3 * s * (1 - 3 * t) * p2.y + 3 * (2 - 3 * t) * t * p3.y + 3 * t * t * p4.y;
                                var result = y1 / (p4.x - p1.x);
                                return result;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var curve in vice_curves)
                {
                    if (x >= curve.start_pos.x && x <= (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x))
                    {
                        for (int i = 0; i < curve.points.Count - 1; i++)
                        {
                            var p1 = curve.points[i].position + curve.start_pos;
                            var p2 = curve.points[i].right_position + curve.start_pos;
                            var p3 = curve.points[i + 1].left_position + curve.start_pos;
                            var p4 = curve.points[i + 1].position + curve.start_pos;
                            if (x >= p1.x && x <= p4.x)
                            {
                                var t = (x - p1.x) / (p4.x - p1.x);
                                var s = 1 - t;
                                var y1 = -3 * Mathf.Pow(s, 2) * p1.y + 3 * s * (1 - 3 * t) * p2.y + 3 * (2 - 3 * t) * t * p3.y + 3 * t * t * p4.y;
                                var result = y1 / (p4.x - p1.x);
                                return result;
                            }
                        }
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 返回路段x位置的主路段的凹凸性,>0下凹,<0 凸,=0 平
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float road_bump(float x, int road_type = 0)
        {
            if (road_type == 0)
            {
                foreach (var curve in main_curves)
                {
                    if (x >= curve.start_pos.x && x <= (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x))
                    {
                        for (int i = 0; i < curve.points.Count - 1; i++)
                        {
                            var p1 = curve.points[i].position + curve.start_pos;
                            var p2 = curve.points[i].right_position + curve.start_pos;
                            var p3 = curve.points[i + 1].left_position + curve.start_pos;
                            var p4 = curve.points[i + 1].position + curve.start_pos;
                            if (x >= p1.x && x <= p4.x)
                            {
                                var t = (x - p1.x) / (p4.x - p1.x);
                                var s = 1 - t;
                                var y2 = 6 * s * p1.y + 6 * (3 * t - 2) * p2.y + 6 * (1 - 3 * t) * p3.y + 6 * t * p4.y;
                                var d = (p4.x - p1.x);
                                var result = y2 / (Mathf.Pow(d, 2));
                                return result;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var curve in vice_curves)
                {
                    if (x >= curve.start_pos.x && x <= (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x))
                    {
                        for (int i = 0; i < curve.points.Count - 1; i++)
                        {
                            var p1 = curve.points[i].position + curve.start_pos;
                            var p2 = curve.points[i].right_position + curve.start_pos;
                            var p3 = curve.points[i + 1].left_position + curve.start_pos;
                            var p4 = curve.points[i + 1].position + curve.start_pos;
                            if (x >= p1.x && x <= p4.x)
                            {
                                var t = (x - p1.x) / (p4.x - p1.x);
                                var s = 1 - t;
                                var y2 = 6 * s * p1.y + 6 * (3 * t - 2) * p2.y + 6 * (1 - 3 * t) * p3.y + 6 * t * p4.y;
                                var d = (p4.x - p1.x);
                                var result = y2 / (Mathf.Pow(d, 2));
                                return result;
                            }
                        }
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 返回路段位置x的主路段的曲率半径
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float road_radius(float x, int road_type = 0)
        {
            return Mathf.Pow(1 + Mathf.Pow(road_slope(x, road_type), 2), 1.5f) / Mathf.Abs(road_bump(x, road_type));
        }
        /// <summary>
        /// 返回路段位置x的副路段是否隐藏,road_type = 0时无意义,主路面一定不隐藏,x无意义时返回true
        /// </summary>
        /// <param name="x"></param>
        /// <param name="road_type"></param>
        /// <returns></returns>
        public bool road_hidden(float x, int road_type = 1)
        {
            if (road_type == 0)
            {
                return false;
            }
            else
            {
                foreach (var curve in vice_curves)
                {
                    if (x >= curve.start_pos.x && x <= (curve.start_pos.x + curve.points[curve.points.Count - 1].position.x))
                    {
                        return curve.hidden;
                    }
                }
            }
            return true;
        }
    }

    public class Curve
    {
        public Sprite curve_sprite;
        public Vector2 sprite_position;
        public Vector2 start_pos;
        public List<PointData> points = new();
    }

    public class ViceCurve : Curve
    {
        public bool hidden;
    }
}





