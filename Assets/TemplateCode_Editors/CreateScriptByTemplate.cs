using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Editor.AutoCreators
{
    public class CreateScriptByTemplate
    {
        static string assembly_name;
        static string folder_name;
        static string file_name;

        static event System.Func<string,string> m_create_file_name;
        static event Handle_RP_Fields m_create_diy_fields;

        public delegate void Handle_RP_Fields(ref string txt, string folder_name);

        //==================================================================================================

        public static void EXE(string template_path, System.Func<string, string> create_file_name = null, Handle_RP_Fields create_diy_fields = null)
        {
            m_create_file_name = create_file_name;
            m_create_diy_fields = create_diy_fields;

            string locationPath = GetSelectedPathOrFallBack();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateScriptAsset>(),
                locationPath + $"/{file_name}.cs", null, template_path);
        }


        static string GetSelectedPathOrFallBack()
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }

            string[] strs = path.Split('/');

            assembly_name = strs[strs.Length - 2];
            folder_name = strs[strs.Length - 1];

            file_name = m_create_file_name?.Invoke(folder_name);
            file_name ??= string.Empty;

            return path;
        }


        class CreateScriptAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                Object o = CreateScriptByTemp(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(o);
                AssetDatabase.Refresh();
            }


            static Object CreateScriptByTemp(string pathName, string resourceFile)
            {
                string fullPath = Path.GetFullPath(pathName);
                StreamReader streamReader = new StreamReader(resourceFile);
                string txt = streamReader.ReadToEnd();
                streamReader.Close();

                txt = Regex.Replace(txt, "#name#", file_name);
                txt = Regex.Replace(txt, "#namespace#", $"{assembly_name}.{folder_name}");
                m_create_diy_fields?.Invoke(ref txt, folder_name);

                bool encoderShouldEmitUTF8Identifier = true;
                bool throwOnInvalidBytes = false;
                UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
                bool append = false;
                StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
                streamWriter.Write(txt);
                streamWriter.Close();
                AssetDatabase.ImportAsset(txt);
                return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
            }
        }
    }
}
