using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using World;
using World.Business;
using World.Relic;

namespace GameEditorWindows
{
    public class Relic_GEW : EditorWindow
    {
        string relic_id;

        [MenuItem("GameEditorWindow/Relic")]

        public static void ShowWindow()
        {
            GetWindow(typeof(Relic_GEW));
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                GUILayout.Label("请先运行游戏");
                return;
            }

            GUILayout.Label("relic_id");
            {
                relic_id = GUILayout.TextField(relic_id);
            }

            if (GUILayout.Button("AddRelic"))
            {
                Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
                rmgr.CreateRelicAndAdd(relic_id);
            }

            if (GUILayout.Button("RelicStore"))
            {

                Addressable_Utility.try_load_asset<EliteRelicView>("RelicDropView", out var rdv_view);
                var view = Instantiate(rdv_view,WorldSceneRoot.instance.uiRoot.transform,false);
                view.Init(new List<uint> { 4, 6, 7, 8 });


                /* List<string> relics_id = new() {"4","6","7","8" };
                 Mission.instance.try_get_mgr(Config.RelicMgr_Name, out RelicMgr rmgr);
                 rmgr.InstantiateRelicStore(relics_id);*/
            }
        }
    }
}
