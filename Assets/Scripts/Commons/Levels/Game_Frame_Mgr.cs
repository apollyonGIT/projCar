using AutoCodes;
using Foundations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Commons.Levels
{
    public class RouteData
    {
        public uint shop_group_id = new();   //记录商店组id

        public List<uint> safe_zone_event_list = new();

        public (uint, int, string) weather_data;

        public List<(uint mg_id, int weight, float length, string each_context)> mg_data = new(); //怪物组

        public string mg_context; //怪物文本

        public float scene_total_length; //场景总长度
    }


    public class Game_Frame_Mgr
    {
        public struct Struct_world_and_level_info
        {
            public AutoCodes.game_world r_game_world;
            public AutoCodes.level r_level;
            public int world_progress;
        }

        public enum EN_scene_type
        {
            none,
            init,
            normal,
            elite,
            boss
        }

        //==================================================================================================

        public static void init()
        {
            Dictionary<string, Struct_world_and_level_info> world_and_level_infos = new();

            var r_game_worlds = AutoCodes.game_worlds.records;
            var access_worlds = CommonContext.instance.access_world_list;

            foreach (var (world_id, r_game_world) in r_game_worlds.Where(t => t.Value.can_load || access_worlds.Contains(t.Key)))
            {
                var level_keys = AutoCodes.levels.records.Where(t => t.Key.Split(',')[0] == $"{r_game_world.level}").ToArray();
                var _r_level = level_keys[Random.Range(0, level_keys.Length)].Value;

                //录入
                Struct_world_and_level_info _struct = new()
                {
                    r_game_world = r_game_world,
                    r_level = _r_level,
                    world_progress = 1
                };
                world_and_level_infos.Add(world_id, _struct);
            }

            var ds = Share_DS.instance;
            ds.add(Game_Mgr.key_world_and_level_infos, world_and_level_infos);

            //场景序列号
            ds.add(Game_Mgr.key_scene_index, 0);
        }


        public static void upd_world_progress(string world_id, int world_progress)
        {
            Share_DS.instance.try_get_value(Game_Mgr.key_world_and_level_infos, out Dictionary<string, Struct_world_and_level_info> world_and_level_infos);
            var _struct = world_and_level_infos[world_id];
            _struct.world_progress = world_progress;
            world_and_level_infos[world_id] = _struct;

            Share_DS.instance.add(Game_Mgr.key_world_and_level_infos, world_and_level_infos);
        }


        public static bool try_calc_scene_id(int scene_index, out uint scene_id, out EN_scene_type scene_type)
        {
            scene_id = default;
            scene_type = EN_scene_type.none;

            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_world_id, out string world_id);

            ds.try_get_value(Game_Mgr.key_world_and_level_infos, out Dictionary<string, Struct_world_and_level_info> world_and_level_infos);
            var world_and_level_info = world_and_level_infos[world_id];

            var r_level = world_and_level_info.r_level;

            if (scene_index > r_level.scene_count - 1 || scene_index < 0) return false;

            if (scene_index == 0)
            {
                scene_id = r_level.scene_init;
                scene_type = EN_scene_type.init;

                ds.add(Game_Mgr.key_normal_scene_enter_count, 0);
                ds.add(Game_Mgr.key_elite_scene_enter_count, 0);
            }
            else if (scene_index == r_level.scene_count - 1)
            {
                scene_id = r_level.scene_boss;
                scene_type = EN_scene_type.boss;

                ds.add(Game_Mgr.key_normal_scene_enter_count, 0);
                ds.add(Game_Mgr.key_elite_scene_enter_count, 0);
            }
            else
            {
                ds.try_get_value(Game_Mgr.key_normal_scene_enter_count, out int normal_scene_enter_count);
                ds.try_get_value(Game_Mgr.key_elite_scene_enter_count, out int elite_scene_enter_count);

                if (valid_is_elite_scene(normal_scene_enter_count, r_level))
                {
                    scene_id = select_scene_id_from_list(r_level.scene_elite, scene_index);
                    scene_type = EN_scene_type.elite;

                    ds.add(Game_Mgr.key_normal_scene_enter_count, 0);
                    ds.add(Game_Mgr.key_elite_scene_enter_count, elite_scene_enter_count + 1);

                    Debug.Log("连续普通战斗次数，已清空");
                    Debug.Log($"总计精英战斗{elite_scene_enter_count}次");
                }
                else
                {
                    scene_id = select_scene_id_from_list(r_level.scene_normal, scene_index);
                    scene_type = EN_scene_type.normal;

                    ds.add(Game_Mgr.key_normal_scene_enter_count, normal_scene_enter_count + 1);

                    Debug.Log($"已连续普通战斗{normal_scene_enter_count}次");
                }
            }

            return true;
        }


        public static List<RouteData> get_scene_routes(uint scene_id, EN_scene_type scene_type)
        {
            List<RouteData> results = new();

            scenes.TryGetValue($"{scene_id}", out var r_scene);

            switch (scene_type)
            {
                case EN_scene_type.init:
                    {
                        var r = ins_route(r_scene);
                        results = r;
                    }
                    break;
                default:
                    {
                        var r = ins_route(r_scene, Random.Range(r_scene.RS_num.Item1, r_scene.RS_num.Item2 + 1));
                        results = r;
                    }
                    break;
            }

            return results;
        }


        static List<RouteData> ins_route(scene r_scene, int count = 1)
        {
            List<RouteData> datas = new ();

            for (int i = 0; i < count; i++)
            {
                var data = new RouteData();
                datas.Add(data);
            }

            if (r_scene.RS_Shop_pool != null)
            {
                var shop_list = select_dic<uint>(r_scene.RS_Shop_pool, count);
                for (int i = 0; i < count; i++)
                {
                    datas[i].shop_group_id = shop_list[i];
                }
            }

            if (r_scene.RS_Event_pool != null)
            {
                for (int i = 0; i < count; i++)
                {
                    datas[i].safe_zone_event_list = select_dic<uint>(r_scene.RS_Event_pool, 1);
                }
            }

            if (r_scene.RS_Weather_pool != null)
            {
                var weathers_dic = r_scene.RS_Weather_pool.GroupBy(kvp => (kvp.Item1,kvp.Item2,kvp.Item4) ).ToDictionary(group=> group.Key,group => group.Sum(kvp=>kvp.Item3));

                for (int i = 0 ; i < count; i++)
                {
                    var weather_list = select_dic<(uint, int, string)>(weathers_dic, 1);
                    datas[i].weather_data = (weather_list[0].Item1, weather_list[0].Item2, weather_list[0].Item3);
                }
            }

            if (r_scene.RS_MG_pool != null)
            {
                for (int i = 0; i < count; i++)
                {
                    calc_mg(r_scene, out datas[i].mg_data, out datas[i].mg_context, out datas[i].scene_total_length);
                }
            }

            return datas;
        }


        static List<T> select_dic<T>(Dictionary<T, int> dictionary, int count)
        {
            var items = dictionary.ToList();
            int totalWeight = items.Sum(item => item.Value);
            var selected = new List<T>();
            var random = new System.Random();

            while (selected.Count < count && totalWeight > 0)
            {
                // 生成随机数并选择元素
                int randomNumber = random.Next(totalWeight);
                int cumulative = 0;
                T selectedKey = default;

                foreach (var item in items)
                {
                    cumulative += item.Value;
                    if (randomNumber < cumulative)
                    {
                        selectedKey = item.Key;
                        break;
                    }
                }

                if (selectedKey == null) continue;

                selected.Add(selectedKey);

                // 更新剩余元素和总权重
                var selectedItem = items.First(item => EqualityComparer<T>.Default.Equals(item.Key, selectedKey));
                items.Remove(selectedItem);
                totalWeight -= selectedItem.Value;
            }

            return selected;
        }


        static void calc_mg(scene r_scene, out List<(uint mg_id, int weight, float length, string each_context)> _monster_group_select_list, out string total_context, out float _scene_total_length)
        {
            _monster_group_select_list = new();
            _scene_total_length = default;
            total_context = "";

            //1. 获取场景内的 [怪物组] 的数量
            var mg_num = r_scene.RS_MG_num;

            //2. 确定场景中会出现哪些 [怪物组]
            var mg_pool = r_scene.RS_MG_pool.ToList();
            var i = 0;

            while (i < mg_num)
            {
                var random = Random.Range(0, mg_pool.Select(t => t.Item2).Sum());

                var index = 0;
                var weight_sum = 0f;
                for (int j = 0; j < mg_pool.Count; j++)
                {
                    weight_sum += mg_pool[j].Item2;
                    if (weight_sum > random)
                    {
                        index = j;
                        break;
                    }
                }

                _monster_group_select_list.Add(mg_pool[index]);
                mg_pool.RemoveAt(index);

                i++;
            }

            //3、4. 根据已选取的 [怪物组] 和 场景类型，填充场景，并计算总长度
            var config = Config.current;
            var dis = config.scene_distance_begin + config.scene_distance_end + (mg_num - 1) * config.scene_distance_interval;
            dis += _monster_group_select_list.Select(t => t.length).Sum();

            _scene_total_length = dis;

            //规则：计算怪物文本，每段以空行连接，若为空则跳过
            foreach (var (_,_,_,_each_ctx) in _monster_group_select_list)
            {
                total_context += _each_ctx;

                if (!string.IsNullOrEmpty(total_context))
                    total_context += "\n";
            }

            if (total_context.Length > 1)
                total_context = total_context[..^1];
        }


        static bool valid_is_elite_scene(int normal_scene_enter_count, level r_level)
        {
            var odds = r_level.elite_odds;
            if (odds == null) return false;

            return Random.Range(0, 100) < odds[normal_scene_enter_count];
        }


        static uint select_scene_id_from_list(List<uint> list, int index)
        {
            if (index > list.Count - 1)
                return list.Last();
            else
                return list[index];
        }
    }
}

