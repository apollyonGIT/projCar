using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog_Window : MonoBehaviour
    {
        public Encounter_Dialog_Window_Title title;
        public Encounter_Dialog_Window_Dialog dialog;

        public Encounter_Dialog_Window_Btn_Option btn_option_model;

        List<Encounter_Dialog_Window_Btn_Option> m_btn_options = new();
        public List<Encounter_Dialog_Window_Btn_Option> btn_options => m_btn_options;

        public Encounter_Dialog_Window_Btn_Option[] diy_btn_options;
        public Encounter_Dialog_Outter outter;

        public GameObject[] outter_go_array;

        //==================================================================================================

        public void init(params object[] args)
        {
            var uname = (string)args[0];
            AutoCodes.encounter_dialogs.TryGetValue(uname, out var r_encounter_dialog);

            if (title != null)
                //title.content.text = Localization_Utility.get_localization_dialog((string)args[1]);
                title.content.text = Localization_Utility.get_localization_dialog(r_encounter_dialog.title);

            if (dialog != null)
                //dialog.content.text = Localization_Utility.get_localization_dialog((string)args[2]);
                dialog.content.text = Localization_Utility.get_localization_dialog(r_encounter_dialog.content);

            var btn_option_ac_list = (List<(string, Func<object>)>)args[3];

            var is_auto = !diy_btn_options.Any();
            if (is_auto) //自动：按配置生成按钮
            {
                for (int i = 0; i < btn_option_ac_list.Count; i++)
                {
                    var btn_option = Instantiate(btn_option_model, btn_option_model.transform.parent);
                    btn_option.gameObject.SetActive(true);
                    m_btn_options.Add(btn_option);
                }
            }
            else //手动：用户指定按钮
            {
                foreach (var diy_btn_option in diy_btn_options)
                {
                    if (diy_btn_option.gameObject.TryGetComponent(out Button btn))
                    {
                        diy_btn_option.btn_option = btn;
                    }
                    else
                    {
                        var _btn = diy_btn_option.gameObject.AddComponent<Button>();
                        diy_btn_option.btn_option = _btn;
                    }

                    m_btn_options.Add(diy_btn_option);
                }
            }

            int option_index = 0;
            foreach (var (option_name, option_ac) in btn_option_ac_list)
            {
                var btn_info = m_btn_options[option_index];

                if (btn_info.btn_option_contents!= null)
                    //btn_info.btn_option_name.text = Localization_Utility.get_localization_dialog(option_name);
                    //btn_info.btn_option_contents.text = Localization_Utility.get_localization_dialog(r_encounter_dialog.btn_content[option_index]);
                {
                    var _text = Localization_Utility.get_localization_dialog(r_encounter_dialog.btn_content[option_index]).Split('|');
                    for (int i = 0; i < _text.Length; i++)
                    {
                        btn_info.btn_option_contents[i].text = _text[i];
                    }
                }

                btn_info.btn_option.onClick.AddListener(
                    () =>
                    {
                        foreach (var btn_click_ac in btn_info.btn_click_ac_list)
                        {
                            btn_click_ac?.Invoke();
                        }

                        fini();
                        option_ac?.Invoke();
                    });

                option_index++;
            }

            Encounter_Dialog.instance.opening_window = this;

            var console = (string)args[4];
            Encounter_Dialog_Console.decode_console(this, console, uname);

            //加载外部模块
            if (Encounter_Dialog.instance.cache_dic.TryGetValue("outter_go_infos", out var _outter_go_infos))
            {
                var outter_go_infos = (Dictionary<string, bool>)_outter_go_infos;

                foreach (var (go_name, go_active) in outter_go_infos)
                {
                    outter_go_array.Where(t => t.name == go_name).First().SetActive(go_active);
                }
            }
            else
            {
                Encounter_Dialog.instance.cache_dic.Add("outter_go_infos", new Dictionary<string, bool>());
            }
        }


        public void fini()
        {
            DestroyImmediate(gameObject);

            var e = Encounter_Dialog.instance;
            e.opening_window = null;
        }
    }
}

