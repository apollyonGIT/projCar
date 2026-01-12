using UnityEditor;
using UnityEngine;
using World;
using World.Caravans;

namespace GameEditorWindows
{
    public class Caravan_GEW : EditorWindow
    {
        public float jump_input_h = 10;
        public float jump_input_vy = 3;

        //================================================================================================

        [MenuItem("GameEditorWindow/Caravan _F4")]
        public static void ShowWindow()
        {
            GetWindow(typeof(Caravan_GEW));
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
            if (ctx == null) return;

            GUILayout.BeginVertical();
            {
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label("");
                GUILayout.Label("【基础参数】");
                GUILayout.Label($"位置: {ctx.caravan_pos}");
                GUILayout.Label($"速度: {ctx.caravan_velocity}");
                GUILayout.Label($"加速度: {ctx.caravan_acc}");

                GUILayout.Label("");
                GUILayout.Label("【状态】");
                GUILayout.Label($"加速状态: {ctx.caravan_status_acc}");
                GUILayout.Label($"滞空状态: {ctx.caravan_status_liftoff}");
                GUILayout.Label($"运动状态: {CaravanMover.move_type}");

                GUILayout.Label("");
                {
                    if (GUILayout.Button("行驶"))
                    {
                        CaravanMover.do_run();
                    }
                }

                GUILayout.Label("");
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label($"跳跃-高度");
                    {
                        jump_input_h = EditorGUILayout.FloatField(jump_input_h, GUILayout.Width(100));
                    }
                    if (GUILayout.Button("跳跃"))
                    {
                        CaravanMover.do_jump_input_h(jump_input_h);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label($"跳跃-速度");
                    {
                        jump_input_vy = EditorGUILayout.FloatField(jump_input_vy, GUILayout.Width(100));
                    }
                    if (GUILayout.Button("跳跃"))
                    {
                        CaravanMover.do_jump_input_vy(jump_input_vy);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}

