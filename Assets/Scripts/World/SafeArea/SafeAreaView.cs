using Addrs;
using AutoCodes;
using Commons;
using Commons.Levels;
using Foundations;
using Foundations.Refs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.BackPack;
using World.Business;
using World.Helpers;
using World.Progresss;

namespace World.SafeArea
{
    public class SafeAreaView : MonoBehaviour
    {
        public Button character_button;
        public Button device_button;
        public Button relic_button;
        public Button fix_button;
        public Button elite_relic_button;


        public Button next_button;
        public Button settlement_button;
        public Button backpack_button;

        public Button encounter_button;


        public RouteView route_view;

        public Transform event_pos;


        public void Init()
        {
            character_button.gameObject.SetActive(false);
            device_button.gameObject.SetActive(false);
            relic_button.gameObject.SetActive(false);
            fix_button.gameObject.SetActive(false);
            elite_relic_button.gameObject.SetActive(false);

            var route = WorldContext.instance.routeData;
            scene_shop_groups.TryGetValue(route.shop_group_id.ToString(), out var route_shop_group);
            if (route_shop_group!= null)
            {
                foreach (var shop in route_shop_group.shop_list)
                {
                    init_shop(shop);
                }
            }

            #region 奇遇事件
            encounter_button.gameObject.SetActive(false);

            if(route.safe_zone_event_list != null && route.safe_zone_event_list.Count!=0)
            {
                var event_id = route.safe_zone_event_list[0];
                safe_zone_events.TryGetValue(event_id.ToString(), out var r_event);

                Addrs.Addressable_Utility.try_load_asset(r_event.view_prefab, out AssetRef _img_asset);
                encounter_button.transform.GetComponent<Image>().sprite = (Sprite)_img_asset.asset;
                encounter_button.gameObject.SetActive(true);

                encounter_button.onClick.AddListener(
                    () =>
                    {
                        ProgressEvent progressEvent = new();
                        progressEvent.record = new();
                        progressEvent.record.dialogue_graph = r_event.dialogue_graph;

                        Encounters.Dialogs.Encounter_Dialog.instance.init(progressEvent, "Dialog_Window");
                    });
            }

            #endregion

            ChooseRoutes();
        }

        private void init_shop(string shop_name)
        {
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);
            ds.try_get_value(Game_Mgr.key_world_id, out string world_id);
            game_worlds.TryGetValue(world_id, out var r_world);
            if (r_world.shop_list_role.Count <= scene_index)
            {
                scene_index = r_world.shop_list_role.Count - 1;
            }

            game_worlds.TryGetValue(world_id, out var r_game_world);

            switch (shop_name)
            {
                case "role":
                    Debug.Log("生成角色商店");
                    init_role(r_game_world.shop_list_role[scene_index]);
                    break;
                case "device":
                    Debug.Log("生成设备商店");
                    init_device(r_game_world.shop_list_device[scene_index]);
                    break;
                case "relic":
                    Debug.Log("生成遗物商店");
                    init_relic(r_game_world.shop_list_relic[scene_index]);
                    break;
                case "repair":
                    Debug.Log("生成维修商店");
                    init_fix();
                    break;
                case "elite_relic":
                    Debug.Log("生成精英遗物商店");
                    init_elite_relic(r_game_world.shop_list_elite_relic[scene_index]);
                    break;
            }
        }

        private void init_role(uint business_id)
        {
            character_button.gameObject.SetActive(true);

            Addressable_Utility.try_load_asset<CharacterBusinessPanel>("CharacterBusinessPanel", out var cbp);
            var cbp_ins = Instantiate(cbp, transform, false);
            cbp_ins.gameObject.SetActive(false);
            cbp_ins.Init(business_id);

            character_button.onClick.AddListener(() =>
            {
                cbp_ins.gameObject.SetActive(true);
                cbp_ins.Open();
            });
        }

        private void init_device(uint business_id)
        {
            device_button.gameObject.SetActive(true);

            Addressable_Utility.try_load_asset<DeviceBusinessPanel>("DeviceBusinessPanel", out var dbp);
            var dbp_ins = Instantiate(dbp, transform, false);
            dbp_ins.gameObject.SetActive(false);
            dbp_ins.Init(business_id);

            device_button.onClick.AddListener(() =>
            {
                dbp_ins.gameObject.SetActive(true);
                dbp_ins.Open();
            });
        }

        private void init_relic(uint business_id)
        {
            relic_button.gameObject.SetActive(true);

            Addressable_Utility.try_load_asset<RelicShopView_3>("RelicBusinessView", out var rbv);
            var rbv_ins = Instantiate(rbv, transform, false);
            rbv_ins.gameObject.SetActive(false);
            Mission.instance.try_get_mgr("BusinessMgr", out BusinessMgr bmgr);
            var relic_business = new RelicBusiness();
            relic_business.add_view(rbv_ins);
            relic_business.Init(business_id);

            relic_button.onClick.AddListener(() =>
            {
                rbv_ins.gameObject.SetActive(true);
                rbv_ins.Open();
            });
        }

        private void init_fix()
        {
            fix_button.gameObject.SetActive(true);

            Addressable_Utility.try_load_asset<Fix_Window>("FixBusiness", out var fb);
            var fb_ins = Instantiate(fb, transform, false);
            fb_ins.gameObject.SetActive(false);
            fb_ins.Init();
            fix_button.onClick.AddListener(() =>
            {
                fb_ins.gameObject.SetActive(true);
                fb_ins.Open();
            });
        }


        /// <summary>
        /// 因为遗物商店只显示3个relic，而精英显示数量不定，不使用同一个外观
        /// </summary>
        /// <param name="business_id"></param>
        private void init_elite_relic(uint business_id)
        {
            elite_relic_button.gameObject.SetActive(true);

            Addressable_Utility.try_load_asset<EliteRelicView>("EliteRelicView", out var erv);
            var erv_ins = Instantiate(erv, transform, false);
            erv_ins.gameObject.SetActive(false);
            Mission.instance.try_get_mgr("BusinessMgr", out BusinessMgr bmgr);
            var relic_business = new RelicBusiness();
            relic_business.add_view(erv_ins);
            relic_business.Init(business_id);

            elite_relic_button.onClick.AddListener(() =>
            {
                erv_ins.Open();
            });
        }

        public void Update()
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            if (bmgr.ow_slots.Count != 0)
            {
                next_button.interactable = false;
                settlement_button.interactable = false;

                backpack_button.gameObject.SetActive(true);
            }
            else
            {
                next_button.interactable = true;
                settlement_button.interactable = true;

                backpack_button.gameObject.SetActive(false);
            }
        }

        public void ChooseRoutes()
        {
            Safe_Area_Helper.create_routes();

            Share_DS.instance.try_get_value(Game_Mgr.key_routes, out List<RouteData> routes);

            route_view.Init(routes);
        }
    }
}
