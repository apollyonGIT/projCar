using Foundations;
using UnityEditor;
using UnityEngine;
using World;
using World.Caravans;
using World.Enemys;

namespace GameEditorWindows
{
    public class Enemy_GEW : EditorWindow
    {
        int m_id;
        int m_count;

        Vector2 m_init_pos => new(m_init_pos_x, m_init_pos_y);
        int m_init_pos_x;
        int m_init_pos_y;

        string m_init_state = "Default";

        //================================================================================================

        [MenuItem("GameEditorWindow/Enemy _F6")]
        public static void ShowWindow()
        {
            GetWindow(typeof(Enemy_GEW));
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
            if (!Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr mgr)) return;

            GUILayout.BeginVertical();
            {
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label("");
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("ID");
                    {
                        m_id = EditorGUILayout.IntField(m_id, GUILayout.Width(60));
                    }

                    GUILayout.Label("数量");
                    {
                        m_count = EditorGUILayout.IntField(m_count, GUILayout.Width(60));
                    }

                    GUILayout.Label("初始状态");
                    {
                        m_init_state = EditorGUILayout.TextArea(m_init_state, GUILayout.Width(60));
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("初始位置_X");
                    {
                        m_init_pos_x = EditorGUILayout.IntField(m_init_pos_x, GUILayout.Width(50));
                    }

                    GUILayout.Label("初始位置_Y");
                    {
                        m_init_pos_y = EditorGUILayout.IntField(m_init_pos_y, GUILayout.Width(50));
                    }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("添加怪物"))
                {
                    if (m_count == 1)
                    {
                        mgr.pd.add_enemy_directly_req(0, (uint)m_id, m_init_pos, m_init_state);
                    }
                    else
                    {
                        for (int i = 0; i < m_count; i++)
                        {
                            var _pos = m_init_pos + Random.insideUnitCircle * 1f;
                            mgr.pd.add_enemy_directly_req(0, (uint)m_id, _pos, m_init_state);
                        }
                    }
                }

            }
            GUILayout.EndVertical();
        }
    }
}

