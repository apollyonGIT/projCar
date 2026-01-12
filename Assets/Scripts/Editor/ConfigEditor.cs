
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Editor {

    [CustomEditor(typeof(Commons.Config))]
    public class ConfigEdtor : UnityEditor.Editor {

        private void OnEnable()
        {
            propertys.Clear();
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();

            m_filter = GUILayout.TextField(m_filter);

            if (string.IsNullOrEmpty(m_filter)) {

                for(int i = propertys.Count - 1; i >= 0; i--)
                {
                    propertys[i].name_list.Clear();
                }

                var sp = serializedObject.GetIterator();
                if (sp.NextVisible(true)) {
                    while (sp.NextVisible(false))
                    {
                        var fieldInfo = sp.serializedObject.targetObject.GetType().GetField(sp.name);
                        if (fieldInfo != null)
                        {
                            var foldoutAttr = fieldInfo.GetCustomAttributes(typeof(FoldoutAttribute), false).FirstOrDefault() as FoldoutAttribute;
                            if (foldoutAttr != null)
                            {
                                var had = false;
                                foreach(var pros in propertys)
                                {
                                    if(pros.attr_name == foldoutAttr.name)
                                    {
                                        had = true;

                                        pros.name_list.Add(sp.name);

                                        break;
                                    }
                                }
                                if (!had)
                                {
                                    propertys.Add(new SerializedPropertyList()
                                    {
                                        attr_name = foldoutAttr.name,
                                        name_list = new List<string>(),
                                        active = false,
                                    });
                                }
                            }
                            else
                            {
                                var had = false;
                                foreach (var pros in propertys)
                                {
                                    if (pros.attr_name == "无分类")
                                    {
                                        had = true;

                                        pros.name_list.Add(sp.name);

                                        break;
                                    }
                                }
                                if (!had)
                                {
                                    propertys.Add(new SerializedPropertyList()
                                    {
                                        attr_name = "无分类",
                                        name_list = new List<string>(),
                                        active = false,
                                    });
                                }
                            }
                        }
                    }

                    foreach (var p in propertys)
                    {
                        p.active = EditorGUILayout.Foldout(p.active, p.attr_name);
                        if (p.active)
                        {
                            foreach (var _name in p.name_list)
                            {
                                var pro = serializedObject.FindProperty(_name);
                                    
                                EditorGUILayout.PropertyField(pro);
                            }
                        }
                    }
                }
            } else {
                var sp = serializedObject.GetIterator();
                if (sp.NextVisible(true) && sp.NextVisible(false)) {
                    do {
                        if (filter(sp.name)) {
                            EditorGUILayout.PropertyField(sp);
                        }

                    } while (sp.NextVisible(false));
                }
            }


            serializedObject.ApplyModifiedProperties();
        }

        private string m_filter;

        private bool filter(string content) {
            int last = 0;
            foreach (var c in m_filter) {
                var idx = content.IndexOf(c, last);
                if (idx < last) {
                    return false;
                }
                last = idx + 1;
            }
            return true;
        }

        [SerializeField]
        private List<SerializedPropertyList> propertys = new();
    }

    public class SerializedPropertyList
    {
        public string attr_name;
        public List<string> name_list = new();
        public bool active = true;
    }
}