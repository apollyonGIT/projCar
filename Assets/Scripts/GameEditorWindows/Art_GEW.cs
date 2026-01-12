using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using World;
using World.Environments;

namespace GameEditorWindows
{
    public class Art_GEW : EditorWindow
    {
        public class ObjData
        {
            public string path;
            public float depth;

            public ObjData(string path, float f_z_distance)
            {
                this.path = path;
                depth = f_z_distance;
            }
        }
        public List<GameObject> prefabs = new();
        public List<float> vs = new();
        public List<ObjData> pd = new();           //path

        private Vector2 scrollPos = Vector2.zero;
        private float f = 0;

        private EnvironmentPD epd;

        private void OnEnable()
        {
            prefabs.Clear();
            vs.Clear();
            pd.Clear();
            epd = GameObject.FindObjectOfType<EnvironmentPD>();
        }
        private void OnDisable()
        {
            prefabs.Clear();
            vs.Clear();
            pd.Clear();
            epd = null;
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0)
            {
                Debug.Log("++");
                f++;
            }
            else if (scroll < 0)
            {
                Debug.Log("--");
                f--;
            }
        }

        [MenuItem("GameEditorWindow/Art")]
        public static void ShowWindow()
        {
            GetWindow(typeof(Art_GEW));
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                GUILayout.Label("请先运行游戏");
            }
            if (Mission.instance == null)
                return;
            if(Mission.instance.try_get_mgr(Config.EnvironmentMgr_Name,out EnvironmentMgr emgr))
            {
                var ctx = WorldContext.instance;

                if (GUILayout.Button("UpdatePrefabsList"))
                {
                    prefabs.Clear();
                    vs.Clear();
                    pd.Clear();

                    //levels.TryGetValue($"{ctx.r_level.id},{ctx.r_level.sub_id}",out var level_record);
                    //scenes.TryGetValue(level_record.scene[0].ToString(), out var sr_record);


                    var sr_record = WorldContext.instance.r_scene;

                    var group_id = sr_record.scene_resource;
                    foreach(var rec in scene_resources.records)
                    {
                        if(rec.Value.group_id == group_id)
                        {
                            var resource = rec.Value;
                            var paths = rec.Value.resource_path;

                            foreach(var path in paths)
                            {
                                Addressable_Utility.try_load_asset(path, out GameObject g);
                                if(g == null)
                                {
                                    Debug.LogError($"存在预制体为null  sub_id:{resource.sub_id} path:{path}");
                                    continue;
                                }
                                prefabs.Add(g);
                                vs.Add(g.transform.GetChild(0).position.y);
                                pd.Add(new ObjData(path, resource.z_distance));
                            }
                        }
                    }
                }
                if (prefabs != null)
                {
                    if (Event.current.type == EventType.ScrollWheel)
                    {
                        float scroll = Event.current.delta.y;

                        if (scroll > 0)
                        {
                            Event.current.delta = Vector2.zero;
                            f++;
                        }
                        else if (scroll < 0)
                        {
                            Event.current.delta = Vector2.zero;
                            f--;
                        }

                        Repaint();
                    }

                    f = GUILayout.HorizontalSlider(f, 0, prefabs.Count);
                    f = Mathf.Clamp(f, 0, prefabs.Count);


                    GUILayout.Space(20f);

                    var start_i = f;

                    for (int i = (int)start_i; i < start_i + 20; i++)
                    {
                        if (i >= prefabs.Count)
                            break;
                        GUILayout.BeginHorizontal();

                        var t = AssetPreview.GetAssetPreview(prefabs[i]) as Texture;
                        GUILayout.Box(t, GUILayout.Width(100), GUILayout.Height(100));

                        EditorGUILayout.ObjectField(prefabs[i], typeof(GameObject));

                        vs[i] = EditorGUILayout.FloatField("PositionY:", vs[i]);

                        if (GUILayout.Button("Regenerate"))
                        {
                            emgr.UpdatePrefabY(pd[i].path, pd[i].depth, vs[i]);
                        }


                        if (GUILayout.Button("Apply"))
                        {
                            var child_transform = prefabs[i].transform.GetChild(0).transform;
                            child_transform.localPosition = new Vector3(child_transform.localPosition.x, vs[i], child_transform.localPosition.z);
                            PrefabUtility.SavePrefabAsset(prefabs[i]);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }

        }
    }
}
