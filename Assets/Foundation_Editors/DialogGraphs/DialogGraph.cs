using Foundations.DialogGraphs;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine.UIElements;

namespace Foundation_Editors.DialogGraphs
{
    public class DialogGraph : EditorWindow
    {
        DialogGraphView m_view;
        string graph_name;

        //==================================================================================================

        public void init(string graph_name, DialogGraphView view)
        {
            var window = GetWindow<DialogGraph>();
            window.titleContent = new UnityEngine.GUIContent(graph_name);
            window.minSize = new(960, 540);

            m_view = view;
            m_view.StretchToParentSize();
            rootVisualElement.Add(m_view);

            this.graph_name = graph_name;
        }


        private void OnEnable()
        {
            
        }


        private void OnDisable()
        {
            //保存提示
            //HandleCloseEvent();

            rootVisualElement.Remove(m_view);
            DialogGraphEditor_Utility.open_graphName_list.Remove(graph_name);
        }


        private void HandleCloseEvent()
        {
            bool need_save = EditorUtility.DisplayDialog(
                "保存确认",
                "需要保存数据吗？",
                "是",
                "否"
            );

            if (need_save)
            {
                DialogGraphEditor_Utility.save_graph(m_view.asset, m_view);
            }
        }


        [UnityEditor.Callbacks.OnOpenAsset(999)]
        private static bool on_open(int instance_id, int line)
        {
            if (EditorUtility.InstanceIDToObject(instance_id) is DialogGraphAsset asset)
            {
                if (DialogGraphEditor_Utility.open_graphName_list.Contains(asset.name)) return false;
                DialogGraphEditor_Utility.open_graphName_list.AddLast(asset.name);

                if (asset.nodes_data == null || !asset.nodes_data.Any())
                    DialogGraphEditor_Utility.new_graph(asset);
                else
                    DialogGraphEditor_Utility.load_graph(asset);

                return true;
            }

            return false;
        }
    }
}

