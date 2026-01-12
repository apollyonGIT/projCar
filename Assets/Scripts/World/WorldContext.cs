using Commons.Levels;
using Foundations;
using Foundations.Tickers;
using Spine;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class WorldContext : Singleton<WorldContext>
    {
        public Vector2 caravan_pos;
        public Vector2 caravan_velocity;
        public Vector2 caravan_acc;

        public float caravan_rad;
        public Vector2 caravan_dir => EX_Utility.convert_rad_to_dir(caravan_rad);

        public float caravan_rotation_theta;
        public float caravan_rotation_omega;
        public float caravan_rotation_beta;

        public WorldEnum.EN_caravan_status_acc caravan_status_acc;
        public WorldEnum.EN_caravan_status_liftoff caravan_status_liftoff;

        public float caravan_vx_stored;

        public Dictionary<string, Bone> caravan_bones;
        public Dictionary<string, Vector3> caravan_slots;

        public float caravan_move_delta_dis;

        public int caravan_hp;
        public int caravan_hp_max;

        public float total_weight;
        public float driving_lever;
        public int tractive_force_max;
        public float feedback_0;
        public float feedback_1;
        public float bounce_loss;
        public float bounce_coef;

        public float boost_rate;
        public float slow_rate;

        public Caravans.EN_caravan_move_type init_caravan_move_type = Caravans.EN_caravan_move_type.Run;
        public bool is_ban_land;

        public int area_tension;    //地区紧张度
        public float pressure;      //氛围压力
        public WorldEnum.EN_pressure_stage pressure_stage;

        public int kill_score;

        public float reset_dis = 51.2f;
        public bool is_need_reset;

        public Vector2 camera_velocity => caravan_velocity;

        public bool is_run_mode;

        public bool is_elite_mode;

        public RouteData routeData;

        //==================================================================================================

        public string world_id;

        public AutoCodes.game_world r_game_world;
        public AutoCodes.level r_level;
        public int world_progress; //1-5,决定关卡类型

        public float pressure_threshold;
        public float pressure_growth_coef;

        public AutoCodes.scene r_scene;

        public Commons.Levels.Game_Frame_Mgr.EN_scene_type scene_type;

        public float scene_remain_progress; //玩家在场景中的位置进度
        public float scene_progress_rate;

        //==================================================================================================

        public void attach()
        {
            Ticker.instance.do_when_tick_start += ctx_tick_start;
            Ticker.instance.do_when_tick_end += ctx_tick_end;
        }
        

        public void detach()
        {

        }


        private void ctx_tick_start()
        {
            if (caravan_pos.x > reset_dis)
            {
                is_need_reset = true;
            }

            //测试mode，小车强制匀速行驶
            if (is_run_mode)
                caravan_velocity.x = 10f;

            //临时，elite_mode下相关UI显示
            var root = WorldSceneRoot.instance;
            root.is_elite_mode_mono.SetActive(is_elite_mode);
            root.is_elite_mode_hide_mono.SetActive(!is_elite_mode);
        }


        private void ctx_tick_end()
        {
            is_need_reset = false;
        }
    }
}

