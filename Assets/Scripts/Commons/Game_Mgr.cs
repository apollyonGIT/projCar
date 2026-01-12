using Commons.Levels;
using Foundations;
using Foundations.Tickers;
using UnityEngine;

namespace Commons
{
    public class Game_Mgr
    {
        public const string key_world_and_level_infos = "world_and_level_infos";
        public const string key_world_id = "world_id";

        public const string key_scene_index = "scene_index";
        public const string key_scene_info = "scene_info";
        public const string key_routes = "routes";
        public const string key_normal_scene_enter_count = "normal_scene_enter_count";
        public const string key_elite_scene_enter_count = "elite_scene_enter_count";

        public const string key_is_continue_game = "is_continue_game";

        public const string key_game_time = "game_time";                                //单位为帧
        public const string key_total_distance = "total_distance";
        public const string key_kill_enemy_count = "kill_enemy_count";
        public const string key_make_damage_count = "make_damage_count";
        public const string key_total_enemy_hp = "total_enemy_hp";
        public const string key_kill_elite_enemy_count = "elite_enemy_count";

        //==================================================================================================

        public static void on_init_game()
        {
            Share_DS._init();

            Share_DS.instance.add(key_is_continue_game, false);
        }


        public static void on_start_new_game()
        {
            CommonContext._init();
            Game_Frame_Mgr.init();

            Share_DS.instance.add(key_is_continue_game, false);

            Share_DS.instance.add(key_game_time, 0);
            Share_DS.instance.add(key_total_distance, 0f);
            Share_DS.instance.add(key_kill_enemy_count, 0);
            Share_DS.instance.add(key_make_damage_count, 0);
            Share_DS.instance.add(key_total_enemy_hp, 0f);
            Share_DS.instance.add(key_kill_elite_enemy_count, 0);
        }


        public static void on_continue_game()
        {
        }


        public static void on_exit_game()
        {
            
        }


        public static void on_enter_world(params object[] args)
        {
            var world_id = (string)args[0];
            Share_DS.instance.add(key_world_id, world_id);
        }


        public static void on_exit_world(params object[] args)
        {
            Ticker.instance.is_end = true;

            Share_DS.instance.try_get_value(key_world_id, out string world_id);
            var world_progress = (int)args[0];

            Game_Frame_Mgr.upd_world_progress(world_id, world_progress);

            Share_DS.instance.add(key_is_continue_game, true);
        }
    }
}

