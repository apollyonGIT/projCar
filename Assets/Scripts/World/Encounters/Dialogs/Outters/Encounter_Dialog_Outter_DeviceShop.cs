using Addrs;
using Commons;
using Foundations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Business;
using World.Relic;

namespace World.Encounters.Dialogs
{
    public class Encounter_Dialog_Outter_DeviceShop : Encounter_Dialog_Outter
    {
        public override void @do(params object[] args)
        {
            open_target_window();

            #region 子函数 open_target_window
            void open_target_window()
            {
                //业务代码
                var cache_dic = Encounter_Dialog.instance.cache_dic;
                DeviceBusinessPanel view;

                Addressable_Utility.try_load_asset<DeviceBusinessPanel>("DeviceBusinessPanel", out var dbp);
                view = Instantiate(dbp, WorldSceneRoot.instance.uiRoot.transform);
                view.Init(1000);

                if (cache_dic.TryGetValue("DeviceBusinessPanel", out var _history_goods))
                {
                    view.owner.goods = (List<GoodsData>)_history_goods;

                    foreach (var _view in view.owner.views)
                    {
                        _view.update_goods();
                    }
                }
                else
                {
                    var goods = view.owner.goods;

                    cache_dic.Add("DeviceBusinessPanel", goods);
                }

                //加载window组件
                var window = view.gameObject.AddComponent<Encounter_Dialog_Window>();

                var btn_obj = view.transform.Find("CloseButton").gameObject;
                var btn = btn_obj.AddComponent<Encounter_Dialog_Window_Btn_Option>();
                btn.btn_option = btn_obj.GetComponent<Button>();

                window.diy_btn_options = new Encounter_Dialog_Window_Btn_Option[]{ btn };

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

