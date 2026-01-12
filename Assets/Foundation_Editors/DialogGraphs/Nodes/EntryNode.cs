using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Foundation_Editors.DialogGraphs
{
    public class EntryNode : Node
    {

        //==================================================================================================

        public static EntryNode create_entryNode()
        {
            EntryNode node = new()
            {
                title = "Entry",
            };

            var port = DialogGraphEditor_Utility.create_port(node, Direction.Output);
            port.portName = "Next";
            node.outputContainer.Add(port);

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }
    }
}

