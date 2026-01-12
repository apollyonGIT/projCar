using UnityEditor;
using UnityEngine;
using Foundations.SavePrefabs;

namespace Foundation_Editors.SavePrefabs
{
    [CustomEditor(typeof(SavePrefab))]
    public class SavePrefab_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("do"))
            {
                var instance = (SavePrefab)target;
                instance.@do();
            }
        }
    }
}

