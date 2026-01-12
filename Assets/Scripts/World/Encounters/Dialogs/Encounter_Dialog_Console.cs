using Foundations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog_Console
    {
        static string[] ui_type_array = new[] 
        {
            "btn",
            "dialog",
            "title",
        };

        //==================================================================================================

        public static void decode_console(Encounter_Dialog_Window window, string raw, string uname)
        {
            if (string.IsNullOrEmpty(raw)) return;

            var commands = raw.Split(';');
            foreach (var _command in commands.Where(t => t != ""))
            {
                var command = Regex.Replace(_command, @"[\s\n]+", "");
                var infos = command.Split("+=");
                var target_info = infos[0];

                var cpn_infos = infos[1].Split(new[] { '(', ')' });
                var cpn_name_info = cpn_infos[0];
                var cpn_name = cpn_name_info.Split('$')[0];
                var cpn_prms = EX_Utility.string_safe_split(cpn_infos[1], ",");

                foreach (var ui_type in ui_type_array)
                {
                    if (target_info.Contains(ui_type))
                    {
                        var ui = (IEncounter_Dialog_Window_UI)typeof(Encounter_Dialog_Console).GetMethod($"{ui_type}_attacher")
                            .Invoke(null, new object[] { window, target_info });

                        var cpn_type = Assembly.Load("World").GetType($"World.Encounters.Dialogs.{cpn_name}");
                        var cpn = ui.gameObject.AddComponent(cpn_type);

                        var icpn = cpn as IEncounter_Dialog_CPN;
                        icpn.key_name = $"{uname}&{target_info}&{cpn_name_info}";
                        icpn.@do(ui, cpn_prms);

                        break;
                    }
                }
            }
        }


        public static IEncounter_Dialog_Window_UI btn_attacher(Encounter_Dialog_Window window, string target_info)
        {
            var option_index = int.Parse(target_info["btn".Length..]);
            var option = window.btn_options[option_index];

            return option;
        }


        public static IEncounter_Dialog_Window_UI dialog_attacher(Encounter_Dialog_Window window, string target_info)
        {
            return window.dialog;
        }


        public static IEncounter_Dialog_Window_UI title_attacher(Encounter_Dialog_Window window, string target_info)
        {
            return window.title;
        }
    }
}

