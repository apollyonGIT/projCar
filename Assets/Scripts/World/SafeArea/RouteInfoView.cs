using Addrs;
using AutoCodes;
using Commons;
using Commons.Levels;
using Foundations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.SafeArea
{
    public class RouteInfoView : MonoBehaviour,IPointerClickHandler
    {

        public Transform shop_content;

        public SingleShopInfoView shop_prefab;

        public TextMeshProUGUI event_text;

        public TextMeshProUGUI weather_text;

        public TextMeshProUGUI enemy_text;

        public List<SingleShopInfoView> shops = new();

        [HideInInspector]
        public RouteView owner;

        public RouteData data;

        public void Init(RouteData r,RouteView rv)
        {
            foreach (var g in shops)
            {
                Destroy(g.gameObject);
            }
            shops.Clear();
            owner = rv;
            data = r;

            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_info, out (uint scene_id, Game_Frame_Mgr.EN_scene_type scene_type, RouteData route_data) scene_info);
            scenes.TryGetValue(scene_info.scene_id.ToString(), out var scene_r);

/*            nameText.text = Localization_Utility.get_localization(scene_r.name);
            Addressable_Utility.try_load_asset<Sprite>(scene_r.RS_icon,out var sprite);
            thumbnail.sprite = sprite;*/

            if (scene_info.scene_type == Game_Frame_Mgr.EN_scene_type.boss)
            {
                var obj = Instantiate(shop_prefab, shop_content, false);
                obj.Init("icon_store[namecard-npc_4]","???");
                obj.gameObject.SetActive(true);
                shops.Add(obj);
            }
            else
            {
                scene_shop_groups.TryGetValue(r.shop_group_id.ToString(), out var shop_group_r);
                foreach (var shop_name in shop_group_r.shop_list)
                {
                    var obj = Instantiate(shop_prefab, shop_content, false);

                    shop_uis.TryGetValue(shop_name, out var shop_ui_data);
                    obj.Init(shop_ui_data);
                    obj.gameObject.SetActive(true);
                    shops.Add(obj);
                }

                if (weather_text != null)
                    weather_text.text = Localization_Utility.get_localization(r.weather_data.Item3);


                if (r.safe_zone_event_list != null && r.safe_zone_event_list.Count!=0)
                {
                    var event_id = r.safe_zone_event_list[0];
                    safe_zone_events.TryGetValue(event_id.ToString(), out var r_event);
                    if (r_event != null && r_event.rs_desc != null)
                        event_text.text = Localization_Utility.get_localization(r_event.rs_desc);
                }

                if (enemy_text != null)
                {
                    var _enemy_text = Localization_Utility.get_localization(r.mg_context);
                    enemy_text.text = string.IsNullOrEmpty(_enemy_text) ? "？？？" : _enemy_text;
                }
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            owner.Select(data);
            /*Safe_Area_Helper.select_route(data);
            WorldSceneRoot.instance.ExitSafeArea();*/
        }
    }
}
