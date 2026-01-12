using Foundations.DialogGraphs;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class DialogGraphView : GraphView
    {
        public DialogGraphAsset asset;

        //==================================================================================================

        public DialogGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogGraph"));
            SetupZoom(0.2f, 2f);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(new SaveManipulator());
            this.AddManipulator(new CopyManipulator());

            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(EntryNode.create_entryNode());
        }


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            DialogGraphEditor_Utility.menu(evt, this);

            //base.BuildContextualMenu(evt);
        }


        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> ret = new();
            ports.ForEach((port) =>
            {
                if (startPort != port && startPort.node != port.node)
                    ret.Add(port);
            });

            return ret;
        }
    }
}

