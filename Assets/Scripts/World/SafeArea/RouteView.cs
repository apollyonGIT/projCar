using Addrs;
using AutoCodes;
using Commons;
using Commons.Levels;
using Foundations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Helpers;

namespace World.SafeArea
{
    public class RouteView : MonoBehaviour
    {
        public Transform content;
        public RouteInfoView prefab;

        public RouteData select;

        public RouteInfoView select_view;

        public List<RouteInfoView> infos = new();

        public GameObject choose_panel, confirm_panel;

        public Image thumbnail,scene_bg_image;

        public TextMeshProUGUI scene_text;

        public void Init(List<RouteData> rd)
        {
            foreach (var info in infos)
            {
                Destroy(info.gameObject);
            }
            infos.Clear();

            if (rd != null)
            {
                foreach(var r in rd)
                {
                    var info = Instantiate(prefab, content, false);
                    info.Init(r,this);
                    info.gameObject.SetActive(true);
                    infos.Add(info);
                }
            }
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_info, out (uint scene_id, Game_Frame_Mgr.EN_scene_type scene_type, RouteData route_data) scene_info);
            scenes.TryGetValue(scene_info.scene_id.ToString(), out var scene_r);
            scene_text.text = Localization_Utility.get_localization(scene_r.name);
            Addressable_Utility.try_load_asset<Sprite>(scene_r.RS_icon, out var t_sprite);
            thumbnail.sprite = t_sprite;
            game_worlds.TryGetValue(WorldContext.instance.world_id.ToString(), out var world_r);
            Addressable_Utility.try_load_asset<Sprite>(world_r.UI_RS_title_bg, out var bg_sprite);
            scene_bg_image.sprite = bg_sprite;
        }

        public void Select(RouteData select)
        {
            this.select = select;

            choose_panel.SetActive(false);
            confirm_panel.SetActive(true);

            select_view.Init(select, this);
        }

        public void ReSelect()
        {
           choose_panel.SetActive(true);
           confirm_panel.SetActive(false);

           select = null;
        }

        public void Go()
        {
            Safe_Area_Helper.select_route(select);
            WorldSceneRoot.instance.ExitSafeArea();
        }
    }
}
