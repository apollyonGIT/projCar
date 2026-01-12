using Foundations;
using Foundations.DialogGraphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class DialogGraphEditor_Utility
    {
        public static LinkedList<string> open_graphName_list = new();

        //==================================================================================================

        public static Port create_port(Node node, Direction dir, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(Orientation.Horizontal, dir, capacity, typeof(float));
        }


        public static Edge create_edge(DialogGraphView view, Port output, Port input)
        {
            Edge edge = new()
            {
                output = output,
                input = input
            };

            edge.input.Connect(edge);
            edge.output.Connect(edge);

            view.Add(edge);

            return edge;
        }


        public static void menu(ContextualMenuPopulateEvent evt, DialogGraphView view)
        {
            var menu = evt.menu;

            menu.AppendAction("DialogWindow Node", (ac) => { view.AddElement(DialogWindowNode.create_node_init("DialogWindow", ac.eventInfo.mousePosition, view)); });
            menu.AppendAction("Destroy Node", (ac) => { view.AddElement(DestroyNode.create_node_init("Destroy", ac.eventInfo.mousePosition, view)); });
            menu.AppendAction("Jump Node", (ac) => { view.AddElement(JumpNode.create_node_init("Jump", ac.eventInfo.mousePosition, view)); });
            menu.AppendAction("Parallel Node", (ac) => { view.AddElement(ParallelNode.create_node_init("Parallel", ac.eventInfo.mousePosition, view)); });
        }


        public static DialogGraphView new_graph(DialogGraphAsset asset)
        {
            DialogGraphView view = new()
            {
                asset = asset
            };

            var g = UnityEngine.ScriptableObject.CreateInstance<DialogGraph>();
            g.init(asset.name, view);

            return view;
        }


        public static void load_graph(DialogGraphAsset asset)
        {
            var view = new_graph(asset);
            var asset_nodes = (Dictionary<string, DialogNode_Data>)EX_Utility.byte2object(asset.nodes_data);
            var asset_edges = (Dictionary<string, Edge_Data>)EX_Utility.byte2object(asset.edges_data);

            foreach (var (_ ,asset_node) in asset_nodes)
            {
                var node_type = Assembly.Load("Foundation_Editors").GetType(asset_node.node_type_str);

                node_type.GetMethod("create_node", new Type[] { typeof(DialogNode_Data) , typeof(DialogGraphView) })
                    ?.Invoke(null, new object[] { asset_node.clone(), view });
            }

            foreach (var (_ ,asset_edge) in asset_edges)
            {
                Port i_port = default;
                Port o_port = default;

                if (asset_edge.is_entry)
                    o_port = (Port)view.nodes.Where(t => t is EntryNode).First().outputContainer[0];

                foreach (var _view_node in view.nodes)
                {
                    if (_view_node is not DialogNode view_node) continue;
                    var _desc = view_node._desc;

                    if (_desc.GUID == asset_edge.i_node_GUID)
                    {
                        i_port = (Port)view_node.inputContainer[0];
                        continue;
                    } 
                    
                    if (_desc.GUID == asset_edge.o_node_GUID)
                    {
                        o_port = (Port)view_node.outputContainer[asset_edge.o_portIndex];
                        continue;
                    }
                }

                create_edge(view, o_port, i_port);
            }
        }


        public static void save_graph(DialogGraphAsset asset, DialogGraphView view)
        {
            Dictionary<string, DialogNode_Data> nodes_data_dic = new();
            Dictionary<string, Edge_Data> edges_data_dic = new();

            foreach (var _view_node in view.nodes)
            {
                if (_view_node is not DialogNode view_node) continue;

                var _desc = view_node._desc;
                var _pos = view_node.GetPosition().position;
                _desc.pos = (_pos.x, _pos.y);

                List<DialogNode_Port_Data> ports = new();
                foreach (var _port in view_node.outputContainer.Children())
                {
                    if (_port is not Port port) continue;
                    ports.Add(new DialogNode_Port_Data()
                    {
                        portName = port.portName
                    });
                }
                _desc.ports = ports.ToArray();

                nodes_data_dic.Add(view_node._desc.GUID, view_node._desc.clone());
            }
            asset.nodes_data = EX_Utility.object2byte(nodes_data_dic);

            foreach (var view_edge in view.edges)
            {
                var o_port = view_edge.output;
                var _o_node = o_port.node;
                var o_port_index = _o_node.outputContainer.IndexOf(o_port);
                var i_node = (DialogNode)view_edge.input.node;

                Edge_Data edge_data = new()
                {
                    GUID = Guid.NewGuid().ToString()
                };

                if (_o_node is EntryNode)
                {
                    edge_data.is_entry = true;
                }
                else if (_o_node is DialogNode o_node)
                {
                    edge_data.o_node_GUID = o_node._desc.GUID;
                    edge_data.o_portName = o_port.portName;
                    edge_data.o_portIndex = o_port_index;
                }

                edge_data.i_node_GUID = i_node._desc.GUID;

                edges_data_dic.Add(edge_data.GUID, edge_data);
            }
            asset.edges_data = EX_Utility.object2byte(edges_data_dic);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        //==================================================================================================

        public static void create_input_prm(DialogNode node, ref TextField res, string prm_name, string field_key)
        {
            VisualElement row = new();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Stretch;

            Label label = new()
            {
                text = prm_name,
            };
            label.style.flexShrink = 0;
            label.style.marginLeft = 10;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;

            TextField text_field = new()
            {
                multiline = true
            };
            text_field.RegisterValueChangedCallback((evt) =>
            {
                node._desc.fields[field_key] = evt.newValue;
            });
            res = text_field;

            text_field.style.flexGrow = 1;
            text_field.style.width = 100;

            row.Add(label);
            row.Add(text_field);
            node.contentContainer.Add(row);
        }


        public static void create_uname_area(DialogNode node)
        {
            TextField uname_content = new();
            uname_content.RegisterValueChangedCallback((evt) =>
            {
                node._desc.uname = evt.newValue;
            });
            node.titleContainer.Add(uname_content);
            node.uname_content = uname_content;
        }
    }
}

