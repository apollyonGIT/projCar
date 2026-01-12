using Foundations.DialogGraphs;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class ParallelNode : DialogNode
    {
        public override Type node_type => typeof(ParallelNode);
        public override Type coder_type => typeof(ParallelNode_Coder);

        //==================================================================================================

        public static ParallelNode create_node(DialogNode_Data data, DialogGraphView view)
        {
            var node = create_node_init(data.node_name, new(data.pos.Item1, data.pos.Item2), view);
            node._desc = data;
            node.uname_content.value = data.uname;
            node._desc.node_type_str = node.node_type.ToString();

            if (data.ports != null && data.ports.Any())
            {
                foreach (var port in data.ports)
                {
                    node.add_option(view, port.portName);
                }
            }

            view.AddElement(node);

            return node;
        }


        public static ParallelNode create_node_init(string nodeName, Vector2 pos, DialogGraphView view)
        {
            ParallelNode node = new()
            {
                title = nodeName,
            };
            node._desc.GUID = Guid.NewGuid().ToString();
            node._desc.node_name = nodeName;
            
            node._desc.coder_type = node.coder_type;
            node._desc.node_type_str = node.node_type.ToString();

            var inputPort = DialogGraphEditor_Utility.create_port(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            #region uname
            DialogGraphEditor_Utility.create_uname_area(node);
            #endregion

            #region Add Option
            Button btn_add_option = new(() => { node.add_option(view); })
            {
                text = "Add Option"
            };
            node.titleContainer.Add(btn_add_option);
            #endregion

            node.RefreshExpandedState();
            node.RefreshPorts();

            pos = view.contentViewContainer.WorldToLocal(pos);
            node.SetPosition(new Rect(pos, node_size));

            return node;
        }


        public void add_option(DialogGraphView view, string overriden_portName = "")
        {
            var port = DialogGraphEditor_Utility.create_port(this, Direction.Output);
            var count = outputContainer.Query("connector").ToList().Count;

            var portName = string.IsNullOrEmpty(overriden_portName) ? $"Option {count}" : overriden_portName;
            port.portName = portName;

            TextField textField = new()
            {
                name = string.Empty,
                value = portName,
                multiline = true
            };

            textField.style.width = 50;
            textField.RegisterValueChangedCallback((evt) =>
            {
                port.portName = evt.newValue;
            });

            Button btn_delete_option = new(() => { remove_option(view, port); })
            {
                text = "X",
            };

            var is_edit = false;
            Button btn_edit_option = new(() => {
                is_edit = !is_edit;
                var _contentContainer = port.contentContainer;

                if (is_edit)
                {
                    _contentContainer.Add(textField);
                    _contentContainer.Add(btn_delete_option);
                }
                else
                {
                    _contentContainer.Remove(textField);
                    _contentContainer.Remove(btn_delete_option);
                }
            })
            {
                text = "Edit"
            };
            port.contentContainer.Add(btn_edit_option);

            outputContainer.Add(port);
            RefreshExpandedState();
            RefreshPorts();
        }


        void remove_option(DialogGraphView view, Port port)
        {
            var target_edge = view.edges.ToList().Where(t => t.output.portName == port.portName && t.output.node == port.node);

            if (target_edge.Any())
            {
                var edge = target_edge.First();
                edge.input.Disconnect(edge);
                view.RemoveElement(target_edge.First());
            }

            outputContainer.Remove(port);
            RefreshPorts();
            RefreshExpandedState();
        }
    }
}

