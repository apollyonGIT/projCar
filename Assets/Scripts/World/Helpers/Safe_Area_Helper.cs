using Addrs;
using AutoCodes;
using Commons;
using Commons.Levels;
using Foundations;
using System.Collections.Generic;
using UnityEngine;
using World.BackPack;
using World.Business;
using World.Characters;
using World.Devices.Equip;
using World.Progresss;

namespace World.Helpers
{
    public class Safe_Area_Helper
    {
        public static void enter()
        {
            WorldSceneRoot.instance.BlackScreen();
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            foreach (var character in cmgr.characters)
            {
                if (character != null)
                {
                    character.EnterSafeArea();
                }
            }

            //规则：停止世界音效
            Audio_Helper.stop_bgm();
        }

        public static void leave()
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            foreach (var character in cmgr.characters)
            {
                if (character != null)
                {
                    character.LeaveSafeArea();
                }
            }

            Progress_Context.instance.leave_with_next_scene();
        }


        public static void create_routes()
        {
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);

            Game_Frame_Mgr.try_calc_scene_id(++scene_index, out var scene_id, out var scene_type);
            var routes = Game_Frame_Mgr.get_scene_routes(scene_id, scene_type);

            ds.add(Game_Mgr.key_scene_info, (scene_id, scene_type, routes[0]));
            ds.add(Game_Mgr.key_routes, routes);
        }


        public static void select_route(RouteData route)
        {
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_scene_info, out (uint scene_id, Game_Frame_Mgr.EN_scene_type scene_type, RouteData route_data) scene_info);
            ds.add(Game_Mgr.key_scene_info, (scene_info.scene_id, scene_info.scene_type, route));
        }


        public static int GetLootCount(int loot_id)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);

            if(loot_id == 10000)
            {
                return bmgr.coin_count;
            }

            return bmgr.GetLootAmount(loot_id);
        }


        public static bool SpendLootCount(int loot_id, int count)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);

            return bmgr.RemoveLoot(loot_id, count);
        }


        public static void TryToSellDevice(DeviceBusiness db)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            Mission.instance.try_get_mgr("EquipMgr", out EquipmentMgr emgr);
            if (emgr.select_device != null)
            {
                emgr.RemoveDevice(emgr.select_device);

                var price_value = Mathf.CeilToInt(emgr.select_device.desc.base_value * Config.current.shop_device_sell_coef);

                for (int i = 0; i < price_value; i++)
                    bmgr.AddLoot(Config.current.coin_id);

                db.PurchaseDevice(emgr.select_device, 10000, price_value);

                emgr.SelectDevice(null);
            }
        }


        public static void TryToSellCharacter(Business.Business b)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            if (cmgr.select_character != null)
            {
                cmgr.RemoveCharacter(cmgr.select_character);

                for (int i = 0; i < 0; i++)
                    bmgr.AddLoot(Config.current.coin_id);
                b.AddGoods(cmgr.select_character,GoodsType.role,cmgr.select_character.desc.id,10000, 0);
                Cubicle_Helper.SetSelectCharacter(null);
            }
        }


        public static Character CreateCharacter(role role)
        {
            var c = new Character();
            c.Init(role);
            c.Start();

            return c;
        }


        public static DeviceBusinessPanel InsDeviceBusinessPanel(Transform t)
        {
            Addressable_Utility.try_load_asset<DeviceBusinessPanel>("BusinessPanel",out var asset);
            var dbp = GameObject.Instantiate(asset,t,false);
            return dbp;
        }


        public static void InsRelicStore(int relic_shop_id)
        {

        }
    }
}

