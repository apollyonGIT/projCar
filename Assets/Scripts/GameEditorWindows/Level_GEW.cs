using Commons;
using Foundations;
using UnityEditor;
using UnityEngine;
using World;
using World.Enemys;
using World.Tensions;

namespace GameEditorWindows
{
    public class Level_GEW : EditorWindow
    {

        //================================================================================================

        [MenuItem("GameEditorWindow/Level")]
        public static void ShowWindow()
        {
            GetWindow(typeof(Level_GEW));
        }


        void OnInspectorUpdate()
        {
            Repaint();
        }


        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                GUILayout.Label("请先运行游戏");
                return;
            }

            var ctx = WorldContext.instance;

            if (!Mission.instance.try_get_mgr("TensionMgr", out TensionMgr tension_mgr)) return;

            GUILayout.BeginVertical();
            {
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label("【关卡参数】");
                GUILayout.Label($"紧张度: {ctx.area_tension}");
                GUILayout.Label($"紧张度进度: {tension_mgr.tension_progress}/{1e7}");

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("紧张度+1"))
                    {
                        ctx.area_tension++;
                    }

                    if (GUILayout.Button("紧张度-1"))
                    {
                        ctx.area_tension--;
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("");
                GUILayout.Label($"击杀分数: {ctx.kill_score}");
                GUILayout.Label($"压力状态: {ctx.pressure_stage}");
                GUILayout.Label($"压力进度: {ctx.pressure}/{ctx.pressure_threshold}");
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("压力+500"))
                    {
                        ctx.pressure += 500;
                    }

                    if (GUILayout.Button("压力归0"))
                    {
                        ctx.area_tension = 0;
                    }
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();
        }
    }
}

