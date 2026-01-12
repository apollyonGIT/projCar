using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;
using System.Linq;
using Foundations;

namespace Foundation_Editors.DialogGraphs
{
    public class CopyManipulator : Manipulator
    {
        Vector2 m_mouse_pos;

        const string key_copy_nodes = "DialogGraph_CopyManipulator_copy_nodes";
        const string key_copy_edges = "DialogGraph_CopyManipulator_copy_edges";

        //==================================================================================================

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }


        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }


        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (target is not DialogGraphView graphView) return;

            var localMousePosition = evt.localMousePosition;
            Vector2 contentMousePosition = graphView.contentViewContainer.WorldToLocal(graphView.LocalToWorld(localMousePosition));

            m_mouse_pos = contentMousePosition;
        }


        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.C && evt.modifiers == EventModifiers.Control)
            {
                CopySelection();

                evt.StopPropagation();
                evt.PreventDefault();

                return;
            }

            if (evt.keyCode == KeyCode.V && evt.modifiers == EventModifiers.Control)
            {
                PasteSelection();

                evt.StopPropagation();
                evt.PreventDefault();

                return;
            }
        }


        void CopySelection()
        {
            if (target is not DialogGraphView graphView) return;

            List<(DialogNode copy_node, Vector2 pos_offset)> copy_nodes = new();
            List<(string i_node_guid, string o_node_guid, int o_port_index)> copy_edges = new();

            #region nodes
            var default_pos = Vector2.zero;
            var pos_offset = Vector2.zero;

            var selected_nodes = graphView.selection.Where(t => t is DialogNode).Cast<DialogNode>();
            selected_nodes = selected_nodes.OrderBy(t => t.GetPosition().position.y).ThenBy(t => t.GetPosition().position.x);

            int i = 0;
            foreach (var selected_node in selected_nodes)
            {
                if (i == 0)
                {
                    default_pos = selected_node.GetPosition().position;
                    pos_offset = Vector2.zero;
                }
                else
                {
                    pos_offset = selected_node.GetPosition().position - default_pos;
                }

                copy_nodes.Add((selected_node, pos_offset));
                i++;
            }
            #endregion

            #region edges
            var selected_edges = graphView.selection.Where(t => t is Edge).Cast<Edge>();

            foreach (var selected_edge in selected_edges)
            {
                var i_port = selected_edge.input;
                var o_port = selected_edge.output;

                if (i_port.node is not DialogNode i_node || o_port.node is not DialogNode o_node) continue;
                if (!selected_nodes.Contains(i_node) || !selected_nodes.Contains(o_node)) continue;

                var i_node_guid = i_node._desc.GUID;
                var o_node_guid = o_node._desc.GUID;
                var o_port_index = o_port.parent.IndexOf(o_port);

                copy_edges.Add((i_node_guid, o_node_guid, o_port_index));
            }
            #endregion

            var share = Share_DS.instance;
            share.add(key_copy_nodes, copy_nodes);
            share.add(key_copy_edges, copy_edges);
        }


        private void PasteSelection()
        {
            if (target is not DialogGraphView graphView) return;

            var share = Share_DS.instance;
            share.try_get_value(key_copy_nodes, out List<(DialogNode copy_node, Vector2 pos_offset)> copy_nodes);
            share.try_get_value(key_copy_edges, out List<(string i_node_guid, string o_node_guid, int o_port_index)> copy_edges);
            if (copy_nodes == null || copy_nodes.Count == 0) return;

            List<ISelectable> new_selections = new();
            Dictionary<string, DialogNode> node_guid_mapping = new();

            #region nodes
            foreach ((DialogNode selected_node, Vector2 pos_offset) in copy_nodes)
            {
                var node = (DialogNode)selected_node.GetType().GetMethod("create_node").Invoke(null, new object[] { selected_node._desc.clone(), graphView });

                node._desc.GUID = Guid.NewGuid().ToString();
                node.SetPosition(new Rect(m_mouse_pos + pos_offset, DialogNode.node_size));

                new_selections.Add(node);
                node_guid_mapping.Add(selected_node._desc.GUID, node);
            }
            #endregion

            #region edges
            foreach ((string _i_node_guid, string _o_node_guid, int o_port_index) in copy_edges)
            {
                node_guid_mapping.TryGetValue(_i_node_guid, out var i_node);
                node_guid_mapping.TryGetValue(_o_node_guid, out var o_node);

                var i_port = (Port)i_node.inputContainer.Children().ElementAt(0);
                var o_port = (Port)o_node.outputContainer.Children().ElementAt(o_port_index);

                DialogGraphEditor_Utility.create_edge(graphView, o_port, i_port);
            }
            #endregion

            graphView.ClearSelection();
            foreach (var new_selection in new_selections)
            {
                graphView.AddToSelection(new_selection);
            }
        }

    }
}
