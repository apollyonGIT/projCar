using Foundations.DialogGraphs;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class DialogNode : Node
    {
        public TextField uname_content;

        public DialogNode_Data _desc = new();

        public virtual Type node_type => null;
        public virtual Type coder_type => null;

        public static Vector2 node_size = new(150, 200);

        //==================================================================================================

    }
}

