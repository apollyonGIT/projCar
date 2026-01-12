using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;
using Foundations.CurveSimulators;

namespace Foundation_Editors.CurveSimulators
{
    [CustomEditor(typeof(CurveSimulator))]
    public class CurveSimulator_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var owner = (CurveSimulator)target;

            CurveSimulator_Helper graph = new("模拟图像")
            {
                minX = owner.x_range.x,
                maxX = owner.x_range.y,
                minY = owner.y_range.x,
                maxY = owner.y_range.y,
            };
            graph.Draw(owner.function);
        }
    }


    public class CurveSimulator_Helper
    {
        const float m_width = 128f;
        const float m_height = 192f;

        public float minX = 0;
        public float maxX = 10;
        public float minY = 0;
        public float maxY = 10;

        Rect m_rect;

        public GraphColors Colors;
        public struct GraphColors
        {
            public Color Background;
            public Color Outline;
            public Color GridLine;
            public Color Function;
            public Color CustomLine;
        }

        public string Title;
        Vector3[] m_lineVertices = new Vector3[2];

        //==================================================================================================

        public CurveSimulator_Helper(string _title = "")
        {
            Title = _title;

            Colors = new GraphColors
            {
                Background = new Color(0.15f, 0.15f, 0.15f, 1f),
                Outline = new Color(0.15f, 0.15f, 0.15f, 1f),
                GridLine = new Color(0.5f, 0.5f, 0.5f),
                Function = Color.red,
                CustomLine = Color.white
            };
        }


        public void Draw(Func<float, float> func = null)
        {
            //标题
            if (!string.IsNullOrEmpty(Title))
            {
                using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
                    GUILayout.Label(Title);
            }

            //组装rect
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                m_rect = GUILayoutUtility.GetRect(m_width, m_height);
            }

            if (Event.current.type != EventType.Repaint)
                return;

            //绘制背景
            List<Vector3> _v = new()
        {
            UnitToGraph(minX, minY),
            UnitToGraph(maxX, minY),
            UnitToGraph(maxX, maxY),
            UnitToGraph(minX, maxY),
        };
            Handles.DrawSolidRectangleWithOutline(_v.ToArray(), Colors.Background, Colors.Outline);

            //辅助线
            var t_x = minX;
            while (++t_x < maxX)
            {
                DrawLine(new(t_x, minY), new(t_x, maxY), Colors.GridLine, 1f);
            }

            t_x = minY;
            while (++t_x < maxY)
            {
                DrawLine(new(minX, t_x), new(maxX, t_x), Colors.GridLine, 1f);
            }

            if (func == null) return;


            //函数线
            t_x = minX;
            while (t_x < maxX)
            {
                var _start_y = func.Invoke(t_x);
                var _end_y = func.Invoke(t_x + 0.1f);

                DrawLine(new(t_x, _start_y), new(t_x + 0.1f, _end_y), Colors.Function, 2f);
                t_x += 0.1f;
            }
        }


        Vector3 UnitToGraph(float x, float y)
        {
            x = Mathf.Lerp(m_rect.x, m_rect.xMax, (x - minX) / (maxX - minX));
            y = Mathf.Lerp(m_rect.yMax, m_rect.y, (y - minY) / (maxY - minY));

            return new Vector3(x, y, 0);
        }


        Vector3 UnitToGraph(Vector2 pos)
        {
            return UnitToGraph(pos.x, pos.y);
        }


        void DrawLine(Vector2 start, Vector2 end, Color color, float _width = 1)
        {
            m_lineVertices[0] = UnitToGraph(start);
            m_lineVertices[1] = UnitToGraph(end);

            Handles.color = color;

            if (end.y > maxY || end.y < minY)
                _width = 0;

            if (end.x == 0 || end.y == 0)
            {
                Handles.color = Color.yellow;
                _width = 2;
            }

            Handles.DrawAAPolyLine(_width, m_lineVertices);
        }

    }
}

