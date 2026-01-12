using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Environments
{
    public class EnvironmentMgrView : MonoBehaviour, IEnvironmentMgrView
    {
        EnvironmentMgr owner;

        public Dictionary<float, EnvironmentObjsView> objs_dic = new();

        public List<(float,EncounterObjView)> encounter_objs = new();

        public EnvironmentObjsView objs_prefab;

        void IEnvironmentMgrView.add_obj(float depth, EnvironmentSingleObj obj)
        {
            if (obj == null)
                return;

            if (objs_dic.ContainsKey(depth))
            {
                objs_dic[depth].AddObj(obj);
                return;
            }
            instantiate_objs(depth);
            objs_dic[depth].AddObj(obj);
        }

        void IModelView<EnvironmentMgr>.attach(EnvironmentMgr owner)
        {
            this.owner = owner;
        }

        void IModelView<EnvironmentMgr>.detach(EnvironmentMgr owner)
        {
            if (this.owner != null)
            {
                this.owner = null;
            }
            Destroy(gameObject);
        }

        void IEnvironmentMgrView.init_environment()
        {
            foreach (var objs in owner.objs_dic)
            {
                var _objs = instantiate_objs(objs.Key);
                foreach (var obj in objs.Value.objList)
                {
                    _objs.Item2.AddObj(obj);
                }
            }
        }

        void IEnvironmentMgrView.reinit_environment()
        {
            foreach (var objs in owner.objs_dic)
            {
                if (objs_dic.TryGetValue(objs.Key, out var v))
                {
                    foreach (var obj in objs.Value.objList)
                    {
                        bool need_add = true;
                        foreach (var o in v.obj_list)
                        {
                            if (o.data == obj)
                            {
                                need_add = false;
                                break;
                            }
                        }
                        if (need_add)
                        {
                            v.AddObj(obj);
                        }
                        need_add = true;
                    }
                }
                else
                {
                    var _obj = instantiate_objs(objs.Key);
                    foreach (var obj in objs.Value.objList)
                    {
                        _obj.Item2.AddObj(obj);
                    }
                }
            }
        }

        void IEnvironmentMgrView.remove_obj(float depth, EnvironmentSingleObj obj)
        {
            if (objs_dic.ContainsKey(depth))
            {
                objs_dic[depth].RemoveObj(obj);
                if (objs_dic[depth].obj_list.Count == 0)
                {
                    Destroy(objs_dic[depth].gameObject);
                    objs_dic.Remove(depth);
                }
                return;
            }
            Debug.LogError($"试图删除一个不存在的Objs中的物体   ---- objs {depth}");
        }

        void IEnvironmentMgrView.reset_environment()
        {
            foreach (var objs in objs_dic)
            {
                objs.Value.SynObjPositon();
            }

            foreach(var (f,e) in encounter_objs)
            {
                var v = e.data.position;
                var vd = e.default_pos;
                e.transform.position = new Vector3(v.x + vd.x, v.y + vd.y,f);
            }

        }

        private (float, EnvironmentObjsView) instantiate_objs(float depth)
        {
            if (objs_dic.ContainsKey(depth))
            {
                Debug.LogError($"已经存在{depth}.Objs 等仍然试图创建");
                return (-1, null);
            }
            var t_layer = Instantiate(objs_prefab, transform, false);
            t_layer.name = $"{depth} Objs";
            t_layer.distance = depth;
            objs_dic.Add(depth, t_layer);
            t_layer.gameObject.SetActive(true);
            return (depth, t_layer);
        }

        void IEnvironmentMgrView.add_encoutner_obj(float depth, EnvironmentEncounterObj obj)
        {
            var g = Instantiate(obj.obj, transform, false);
            var e_view = g.AddComponent<EncounterObjView>();
            e_view.default_pos = obj.default_pos;
            e_view.data = obj;

            e_view.transform.position = e_view.default_pos + new Vector3(obj.position.x,obj.position.y,depth);
            e_view.transform.localScale *= depth / 10;
            e_view.transform.localScale = new Vector3(e_view.transform.localScale.x, e_view.transform.localScale.y, 1); 

            encounter_objs.Add((depth, e_view));
        }

        void IEnvironmentMgrView.remove_encoutner_obj(float depth, EnvironmentEncounterObj obj)
        {
            for (int i = encounter_objs.Count - 1; i >= 0; i--)
            {
                if (encounter_objs[i].Item1 == depth && encounter_objs[i].Item2.data == obj)
                {
                    Destroy(encounter_objs[i].Item2.gameObject);
                    encounter_objs.RemoveAt(i);
                }
            }
        }

        void IEnvironmentMgrView.update_y(string path,float depth,float y)
        {
            if (objs_dic.ContainsKey(depth))
            {
                foreach (var obj in objs_dic[depth].obj_list)
                {
                    if (obj.data.path == path)
                    {
                        obj.transform.GetChild(0).localPosition -= new Vector3(0, obj.default_pos.y, 0);
                        obj.transform.GetChild(0).localPosition += new Vector3(0, y, 0);
                        obj.default_pos = new Vector3(obj.default_pos.x, y, obj.default_pos.z);
                    }
                }
            }
            else
            {
                Debug.LogError($"{depth}layer并不存在,是否修改了没有随机生成出来物体的layer,请检查原因");
            }
        }

        void IEnvironmentMgrView.update_encounter_obj()
        {
            foreach (var (f, e) in encounter_objs)
            {
                var v = e.data.position;
                var vd = e.data.default_pos;
                e.transform.position = new Vector3(v.x + vd.x, v.y + vd.y, f);
            }
        }
    }
}

