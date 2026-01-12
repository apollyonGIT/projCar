using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TitanBuff))]
    public class TitanBuffEditor : UnityEditor.Editor 
    {
        TitanBuff self;
        public void OnEnable()
        {
            self = (TitanBuff)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("ChangeScale"))
            {
                self.ChangeScale();
            }
        }
    }
}
