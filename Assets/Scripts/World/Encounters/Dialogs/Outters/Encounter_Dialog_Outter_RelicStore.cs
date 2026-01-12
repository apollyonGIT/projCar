using Addrs;
using Commons;
using Foundations;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Relic.RelicStores;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog_Outter_RelicStore : Encounter_Dialog_Outter
    {
        public override void @do(params object[] args)
        {
            Time.timeScale = 1;

            BLL_Utility.set_delay(5, 5, Config.current.trigger_length);
            BLL_Utility.delay(open_target_window);

            #region 子函数 open_target_window
            void open_target_window()
            {
                //业务代码
                Addressable_Utility.try_load_asset<RelicStoreView>("rsv", out var rdv_view);
                var view = Instantiate(rdv_view, WorldSceneRoot.instance.uiRoot.transform, false);
                //这一块relic有修改

                //加载window组件
                var window = view.gameObject.AddComponent<Encounter_Dialog_Window>();

                List<Encounter_Dialog_Window_Btn_Option> window_diy_btns = new();
                foreach (var e in view.btns)
                {
                    var btn = e.gameObject.AddComponent<Encounter_Dialog_Window_Btn_Option>();
                    window_diy_btns.Add(btn);
                }

                window.diy_btn_options = window_diy_btns.ToArray();

                //初始化window数据
                var uname = (string)args[0];
                var title = (string)args[1];
                var content = (string)args[2];
                var output_ac_list = (List<(string, Func<object>)>)args[3];
                var console = (string)args[4];

                window.init(uname, title, content, output_ac_list, console);

                //规则：打开对话时，暂停游戏
                Time.timeScale = 0;
            }
            #endregion
        }
    }
}

