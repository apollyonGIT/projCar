using AutoCodes;
using Commons;
using Foundations;
using Foundations.Excels;
using Foundations.MVVM;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Environments.Roads;
using World.Helpers;

namespace World.Environments
{
    public interface IEnvironmentMgrView : IModelView<EnvironmentMgr>
    {
        void init_environment();
        void reset_environment();
        void reinit_environment();
        void add_obj(float depth, EnvironmentSingleObj obj);
        void remove_obj(float depth, EnvironmentSingleObj obj);
        void add_encoutner_obj(float depth, EnvironmentEncounterObj obj);
        void remove_encoutner_obj(float depth, EnvironmentEncounterObj obj);
        void update_y(string path, float depth, float y);
        void update_encounter_obj();
    }

    public class EnvironmentMgr : Model<EnvironmentMgr, IEnvironmentMgrView>, IMgr
    {
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;
        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;
        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
        }
        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
        }

        public EnvironmentMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }


        //===============================================================================================================================

        public Vector2 focus_pos;       //场景刷新点

        private float m_dis = Config.current.desiredResolution.x / (float)Config.current.pixelPerUnit;
        public float dis => m_dis;

        public Dictionary<float, EnvironmentObjs> objs_dic = new();
        public List<(float, EnvironmentEncounterObj)> encounter_layers = new();

        public RoadMgr roadMgr = new RoadMgr();

        public void Init(uint scene_id)
        {
            roadMgr.Init(scene_id, this);
            add_all_resource(scene_id);

            foreach (var view in views)
            {
                view.init_environment();
            }
        }


        public void AddEncounterObj(EncounterObjects eg, Vector2 position)
        {
            foreach (var g in eg.objs)
            {
                var g_trans = g.obj.transform;
                var w = g.width;

                EnvironmentEncounterObj e_obj = new EnvironmentEncounterObj()
                {
                    obj = g_trans.gameObject,
                    width = w,
                    position = position + (Vector2)g.obj.transform.localPosition,
                    default_pos = Vector2.zero,
                };
                if(roadMgr.main_end_pos.x < e_obj.position.x + e_obj.default_pos.x)
                {
                    e_obj.default_pos.y = 0;
                    e_obj.y_is_update = true;
                }
                else
                {
                    e_obj.default_pos.y = Road_Info_Helper.try_get_altitude(e_obj.position.x + e_obj.default_pos.x);
                    e_obj.y_is_update = false;
                }
                encounter_layers.Add((g_trans.position.z, e_obj));
                foreach (var view in views)
                {
                    view.add_encoutner_obj(g_trans.position.z, e_obj);
                }
            }
        }

        public void tick()
        {
            /*
            需要camera的相关参数 
            m_dis = (Common.Config.current.desiredResolution.x / (float)Common.Config.current.pixelPerUnit) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / 10);
            var pre_dis = ((Common.Config.current.desiredResolution.x / (float)Common.Config.current.pixelPerUnit) / 2f + camera_offset.x) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / 10);
            var after_dis = ((Common.Config.current.desiredResolution.x / (float)Common.Config.current.pixelPerUnit) / 2f - camera_offset.x) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / 10);
            */

            focus_pos = WorldSceneRoot.instance.mainCamera.transform.position;

            m_dis = (Config.current.desiredResolution.x / (float)Config.current.pixelPerUnit) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / 10);
            var pre_dis = ((Config.current.desiredResolution.x / (float)Config.current.pixelPerUnit) / 2f) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / 10);
            var after_dis = ((Config.current.desiredResolution.x / (float)Config.current.pixelPerUnit) / 2f) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / 10);

            roadMgr.AddRoad(focus_pos, pre_dis);
            roadMgr.RemoveRoad(focus_pos, m_dis);


            foreach (var kp in objs_dic)
            {
                while (need_remove_obj(kp))
                {

                }
                while (need_add_obj(kp))
                {

                }
            }


            List<(float, EnvironmentEncounterObj)> fs = new();          //删除过去的场景
            foreach (var e in encounter_layers)
            {
                var x = e.Item2.position.x + e.Item2.default_pos.x;

                if(e.Item2.y_is_update == true && roadMgr.main_end_pos.x >= x)      //y轴需要更新
                {
                    e.Item2.y_is_update = false;
                    e.Item2.position.y = Road_Info_Helper.try_get_altitude(x);
                }

                if (need_remove_encounter_obj(e))
                {
                    fs.Add(e);
                }
            }
            foreach (var view in views)
            {
                view.update_encounter_obj();
            }

            foreach (var e in fs)
            {
                encounter_layers.Remove(e);
                foreach (var view in views)
                {
                    view.remove_encoutner_obj(e.Item1, e.Item2);
                }
            }


            if (WorldContext.instance.is_need_reset)
            {
                reset_environment();
            }
        }
        private scene add_all_resource(uint scene_id)
        {
            scenes.TryGetValue(scene_id.ToString(), out var record);
            var group_id = record.scene_resource;

            foreach (var rec in scene_resources.records)
            {
                if (rec.Value.group_id == group_id)
                {
                    var resource = rec.Value;
                    var paths = resource.resource_path;

                    int i = UnityEngine.Random.Range(0, paths.Count);
                    var obj = new EnvironmentSingleObj();

                    obj.path = paths[i];

                    if (!objs_dic.ContainsKey(resource.z_distance))
                    {
                        objs_dic.Add(resource.z_distance, new EnvironmentObjs
                        {
                            group_id = group_id,
                            sub_id = resource.sub_id,
                            next_position = focus_pos - new Vector2(dis, 0) * (1 - WorldSceneRoot.instance.mainCamera.transform.position.z / 10),     //待update
                            objList = new(),
                            max_length = 0,
                            last_active = false,
                        });
                    }
                    update_layer(rec.Value, ref obj);
                    if (obj != null)
                        objs_dic[resource.z_distance].objList.Add(obj);
                }
            }

            return record;
        }
        private scene re_add_all_resource(uint scene_id)
        {
            scenes.TryGetValue(scene_id.ToString(), out var record);
            var group_id = record.scene_resource;

            roadMgr.Reinit(scene_id, this);

            foreach (var rec in scene_resources.records)
            {
                if (rec.Value.group_id == group_id)
                {
                    var resource = rec.Value;
                    var paths = resource.resource_path;

                    int i = UnityEngine.Random.Range(0, paths.Count);
                    var obj = new EnvironmentSingleObj();

                    obj.path = paths[i];

                    if (!objs_dic.ContainsKey(resource.z_distance))
                    {
                        objs_dic.Add(resource.z_distance, new EnvironmentObjs
                        {
                            group_id = group_id,
                            sub_id = resource.sub_id,
                            next_position = focus_pos,       //待update
                            objList = new(),
                            max_length = 0,
                            last_active = false,
                        });
                    }
                    update_layer(rec.Value, ref obj);
                    if (obj != null)
                        objs_dic[resource.z_distance].objList.Add(obj);
                }
            }

            return record;
        }
        private void reset_environment()
        {
            var reset_dis = WorldContext.instance.reset_dis;
            roadMgr.ResetRoad(reset_dis);

            foreach (var p in objs_dic)
            {
                p.Value.next_position -= new Vector2(reset_dis, 0);
                foreach (var o in p.Value.objList)
                {
                    o.position.x -= reset_dis;
                }
            }

            foreach (var e in encounter_layers)
            {
                e.Item2.position -= new Vector2(reset_dis, 0);
            }
            foreach (var view in views)
            {
                view.reset_environment();
            }
        }
        private bool update_layer(scene_resource resource, ref EnvironmentSingleObj obj)
        {
            if (resource.rnd_type.value == Spawn_Type.EN_Spawn_Type.RndPosition)
            {
                float new_x, new_y;
                if (resource.rnd_pos_x == default && resource.rnd_pos_y == default)
                {
                    Debug.LogError("rnd_pos_x,rnd_pos_y没有正确的配置");
                    return false;
                }

                obj.width = resource.resource_length * resource.z_distance / 10;
                new_x = objs_dic[resource.z_distance].next_position.x + polarized_rnd_range(resource.rnd_pos_x.Item1, resource.rnd_pos_x.Item2) * resource.z_distance / 10 + obj.width / 2;
                new_y = UnityEngine.Random.Range(resource.rnd_pos_y.Item1, resource.rnd_pos_y.Item2) * resource.z_distance / 10;

                obj.position = objs_dic[resource.z_distance].next_position;
                obj.spawn_type = resource.rnd_type.value;

                objs_dic[resource.z_distance].max_length = resource.resource_length * resource.z_distance / 10;
                objs_dic[resource.z_distance].next_position = new Vector2(new_x, new_y);

                if (resource.rnd_p_active != default && resource.rnd_p_muted != default)
                {
                    float r = UnityEngine.Random.value;

                    if (objs_dic[resource.z_distance].last_active)
                    {
                        if (r < resource.rnd_p_active)
                        {

                        }
                        else
                        {
                            obj = null;
                            objs_dic[resource.z_distance].last_active = false;
                        }
                    }
                    else
                    {
                        if (r < resource.rnd_p_muted)
                        {
                            obj = null;
                        }
                        else
                        {
                            objs_dic[resource.z_distance].last_active = true;
                        }
                    }
                }
            }
            else if (resource.rnd_type.value == Spawn_Type.EN_Spawn_Type.FixedDistance)
            {
                float new_x;
                new_x = objs_dic[resource.z_distance].next_position.x;
                obj.position = objs_dic[resource.z_distance].next_position;
                obj.spawn_type = Spawn_Type.EN_Spawn_Type.FixedDistance;

                objs_dic[resource.z_distance].max_length = resource.fixed_length * resource.z_distance / 10;
                objs_dic[resource.z_distance].next_position = new Vector2(new_x + resource.fixed_length * resource.z_distance / 10, 0f);
            }
            return true;
        }
        private float polarized_rnd_range(float min, float max)
        {
            var value_0 = UnityEngine.Random.Range(min, max);
            var t_0 = (value_0 - min) / (max - min) * 2 - 1;
            var t_1 = MathF.Sqrt(MathF.Abs(t_0)) * MathF.Sign(t_0);
            return (t_1 + 1) * 0.5f * (max - min) + min;
        }
        private bool need_remove_encounter_obj((float, EnvironmentEncounterObj) kp)
        {
            var obj = kp.Item2;
            var layer_dis = Config.current.desiredResolution.x / Config.current.pixelPerUnit * (1 + Math.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / kp.Item1);
            if (focus_pos.x - obj.position.x - obj.width / 2 > layer_dis * kp.Item1 / 10)
                return true;

            return false;
        }
        private bool need_remove_obj(KeyValuePair<float, EnvironmentObjs> kp)
        {
            if (Config.current.free_camera)         //自由相机下不移除物体
                return false;

            var objs = kp.Value;
            var remove_objs = new List<EnvironmentSingleObj>();
            float active_dis = 25.6f;

            //等待camera相关

            active_dis = (Config.current.desiredResolution.x / (float)Config.current.pixelPerUnit) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / kp.Key);

            foreach (var obj in objs.objList)
            {
                if (focus_pos.x - obj.position.x - obj.width / 2 > active_dis * kp.Key / 10)
                {
                    remove_objs.Add(obj);
                }
            }

            if (remove_objs.Count == 0)
            {
                return false;
            }

            foreach (var remove_obj in remove_objs)
            {
                objs.objList.Remove(remove_obj);
                foreach (var view in views)
                {
                    view.remove_obj(kp.Key, remove_obj);
                }
            }
            return true;
        }
        private bool need_add_obj(KeyValuePair<float, EnvironmentObjs> kp)
        {
            bool add = false;
            var objs = kp.Value;

            float active_dis = 25.6f;
            //active_dis = ((Common.Config.current.desiredResolution.x / (float)Common.Config.current.pixelPerUnit) / 2 + camera_offset.x) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / kp.Key);

            if ((float)(objs.next_position.x - focus_pos.x - objs.max_length / 2) <= (float)(active_dis * kp.Key / 10f))
            {
                var d = add_random_resource(objs.group_id, objs.sub_id);
                foreach (var view in views)
                {
                    view.add_obj(kp.Key, d);
                }
                add = true;
            }

            return add;
        }
        /// <summary>
        /// 从编号为group_id的池子里面取sub_id一份资源出来                  2023.10.13   更新随机的物体会根据上一次的active情况判断  
        /// </summary>
        private EnvironmentSingleObj add_random_resource(uint group_id, uint sub_id)
        {
            scene_resources.TryGetValue($"{group_id},{sub_id}", out var record);

            var paths = record.resource_path;
            int index = UnityEngine.Random.Range(0, record.resource_path.Count);
            var obj = new EnvironmentSingleObj();

            obj.path = paths[index];

            if (!objs_dic.ContainsKey(record.z_distance))
            {
                objs_dic.Add(record.z_distance, new EnvironmentObjs
                {
                    group_id = group_id,
                    sub_id = sub_id,
                    next_position = focus_pos,       //待update
                    objList = new(),
                    max_length = 0,
                    last_active = false,
                });
            }
            else if (paths.Count >= 2)
            {
                int index2 = objs_dic[record.z_distance].objList.Count - 1;
                if (index2 >= 0)
                {
                    var new_obj = objs_dic[record.z_distance].objList[index2];

                    if (new_obj.path == obj.path)      //随机都是一样
                    {
                        var rd = 0;
                        int j = index;
                        while (j == index)
                        {
                            rd++;
                            j = UnityEngine.Random.Range(0, paths.Count);
                            if (rd >= 100)
                            {
                                Debug.Log("触发while循环防死机熔断机制,请检查代码");
                                break;
                            }
                        }
                        obj.path = paths[j];
                    }
                }
            }
            update_layer(record, ref obj);
            if (obj != null)
                objs_dic[record.z_distance].objList.Add(obj);
            return obj;
        }


        public void UpdatePrefabY(string path, float depth, float y)
        {
            foreach (var view in views)
            {
                view.update_y(path, depth, y);
            }
        }
    }

    public class EnvironmentSingleObj
    {
        public string path;
        public Spawn_Type.EN_Spawn_Type spawn_type;

        public Vector2 position;
        public float width;
        public float scale;
    }

    public class EnvironmentObjs        //EnvironmentSingleObj的集合体
    {
        public bool last_active;    //上次刷新是否
        public List<EnvironmentSingleObj> objList = new();
        public Vector2 next_position;
        public float max_length;        //上一个生成的实体的长度

        public uint group_id;
        public uint sub_id;
    }

    public class EnvironmentEncounterObj
    {
        public GameObject obj;
        public float width;
        public Vector2 position;
        public Vector3 default_pos;

        public bool y_is_update = true;      //是否需要更新y坐标
    }

}