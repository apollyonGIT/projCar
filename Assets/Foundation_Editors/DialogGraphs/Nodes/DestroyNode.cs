using Foundations;
using Foundations.DialogGraphs;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class DestroyNode : DialogNode
    {
        public TextField target_uname_content;

        public override Type node_type => typeof(DestroyNode);
        public override Type coder_type => typeof(DestroyNode_Coder);

        //==================================================================================================

        public static DestroyNode create_node(DialogNode_Data data, DialogGraphView view)
        {
            var node = create_node_init(data.node_name, new(data.pos.Item1, data.pos.Item2), view);
            node._desc = data;
            node.uname_content.value = data.uname;
            node._desc.node_type_str = node.node_type.ToString();

            node.target_uname_content.value = (string)EX_Utility.dic_safe_getValue(ref data.fields, "target_uname", "");

            view.AddElement(node);

            return node;
        }


        public static DestroyNode create_node_init(string nodeName, Vector2 pos, DialogGraphView view)
        {
            DestroyNode node = new()
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

            DialogGraphEditor_Utility.create_input_prm(node, ref node.target_uname_content, "【下次节点】", "target_uname");

            node.RefreshExpandedState();
            node.RefreshPorts();

            pos = view.contentViewContainer.WorldToLocal(pos);
            node.SetPosition(new Rect(pos, node_size));

            return node;
        }
    }
}

