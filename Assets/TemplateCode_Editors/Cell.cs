using System.Text.RegularExpressions;
using UnityEditor;

namespace Editor.AutoCreators
{
    public class Cell
    {
        static string path = "Assets/TemplateCode_Editors/Templates/Cell_Template.cs.txt";

        //==================================================================================================

        [MenuItem("Assets/TemplateCode/Cell", false, -1)]
        public static void EXE()
        {
            CreateScriptByTemplate.EXE(path, create_file_name, create_diy_fields);
        }


        public static string create_file_name(string folder_name)
        {
            return create_name(folder_name);
        }


        public static void create_diy_fields(ref string txt, string folder_name)
        {
            var cell = create_name(folder_name);
            txt = Regex.Replace(txt, "#cell#", cell);
        }


        static string create_name(string str)
        {
            var last_3 = str.Substring(str.Length - 3, 3);
            if (last_3 == "ses")
            {
                return str.Remove(str.Length - 2);
            }
            else
            {
                return str.Remove(str.Length - 1);
            }
        }
    }
}

