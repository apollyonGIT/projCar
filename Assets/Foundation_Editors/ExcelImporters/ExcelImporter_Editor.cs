using Foundations.Excels;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Foundation_Editors.ExcelImporters
{
    [CustomEditor(typeof(ExcelImporter))]
    public class ExcelImporter_Editor : ScriptedImporterEditor
    {

        //==================================================================================================

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var asset = (ExcelFileAsset)assetTarget;

            foreach (var e in asset.binarys)
            {
                var sheet_name = e.name;

                GUILayout.Label($"【{sheet_name}】");

                //按钮区
                {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("生成代码"))
                    {
                        ExcelImporter.create_autocode(sheet_name, e.fields, e.type_strs);
                        AssetDatabase.Refresh();
                    }

                    GUILayout.EndHorizontal();
                }
                


                GUILayout.Label("");
            }
        }

    }
}

