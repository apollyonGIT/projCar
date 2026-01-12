using Assets.Scripts.World;
using Commons;
using Commons.Levels;
using Foundations;
using Foundations.SceneLoads;
using Foundations.Tickers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using World.BackPack;
using World.Helpers;
using World.SafeArea;
using World.Settlement;

namespace World
{
    public class WorldSceneRoot : SceneRoot<WorldSceneRoot>
    {
        public ScriptableRendererFeature sr;

        [Header("控制台UI")]
        public TMP_InputField console_IPF;

        [Header("安全区相关")]
        public Image black_screen;
        public SafeAreaView safe_area;

        [Header("世界遮罩")]
        public GameObject world_shadow;

        [Header("音效")]
        public AudioSource bgm_source;

        [Header("临时提示UI")]
        public GameObject is_elite_mode_mono;
        public GameObject is_elite_mode_hide_mono;

        [Header("结算面板")]
        public SettlementPanel settlement_panel;

        [Header("怪物提示UI")]
        public Enemy_UIs.Enemy_Chase_Warning enemy_chase_warning;

        //==================================================================================================

        protected override void on_init()
        {
            var ticker = GetComponent<Ticker_Mono>();
            ticker.init(Config.PHYSICS_TICK_DELTA_TIME);

            WorldContext._init();
            WorldContext.instance.attach();
            load_data_to_ctx();

            BattleContext.instance.Init();

            Context_Init_Helper.init_diy_context();
            Character_Module_Helper.character_module.Clear();

            Road_Info_Helper.reset();
            Encounters.Dialogs.Encounter_Dialog._init();

            sr.SetActive(false);

            base.on_init();

            FadeBlackScreen();

            //防穿帮处理
            world_shadow.SetActive(true);
            Request_Helper.delay_do("world_shadow", 2, 
                (_) => 
                {
                    world_shadow.SetActive(false);
                });

            //音效处理
            Audio_Helper.play_bgm();

            //科技
            Tech_Helper.load_techs();
        }


        protected override void on_fini()
        {
            WorldContext.instance.detach();

            base.on_fini();

            BattleContext.instance.Fini();
        }


        void load_data_to_ctx()
        {
            var ctx = WorldContext.instance;
            var ds = Share_DS.instance;
            var config = Config.current;

            //世界id
            ds.try_get_value(Game_Mgr.key_world_id, out string world_id);
            ctx.world_id = world_id;
            Debug.Log($"========  已加载世界：{ctx.world_id}  ========");

            //世界和关卡信息
            ds.try_get_value(Game_Mgr.key_world_and_level_infos, out Dictionary<string, Game_Frame_Mgr.Struct_world_and_level_info> world_and_level_infos);
            var world_and_level_info = world_and_level_infos[world_id];

            ctx.world_progress = world_and_level_info.world_progress;
            ctx.r_game_world = world_and_level_info.r_game_world;
            ctx.r_level = world_and_level_info.r_level;
            ctx.pressure_threshold = ctx.r_game_world.pressure_threshold[ctx.world_progress - 1];
            ctx.pressure_growth_coef = ctx.r_game_world.pressure_growth_coef[ctx.world_progress - 1];
            Debug.Log($"========  已加载关卡：{ctx.r_level.id}_{ctx.r_level.sub_id}  ========");

            //Amy测试
            if (ds.try_get_value("diy_scene_id", out int diy_scene_id)) 
            {
                AutoCodes.scenes.TryGetValue($"{diy_scene_id}", out ctx.r_scene);
                Debug.Log($"========  已加载测试子场景：{ctx.r_scene.id}  ========");

                ds.remove("diy_scene_id");
            }
            else //正常
            {
                ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);

                var scene_id = 0u;
                if (scene_index == 0) //初次关卡
                {
                    //规则：清除玩家数据
                    Saves.Save_DS.instance.clear();

                    //规则：计算scene_id，并自动选路
                    Game_Frame_Mgr.try_calc_scene_id(scene_index, out scene_id, out ctx.scene_type);
                    ctx.routeData = Game_Frame_Mgr.get_scene_routes(scene_id, ctx.scene_type)[0];
                }
                else //后续关卡
                {
                    ds.try_get_value(Game_Mgr.key_scene_info, out (uint scene_id, Game_Frame_Mgr.EN_scene_type scene_type, RouteData route_data) scene_info);
                    scene_id = scene_info.scene_id;
                    ctx.scene_type = scene_info.scene_type;
                    ctx.routeData = scene_info.route_data;

                    ctx.driving_lever = config.init_power_lever;
                    ctx.caravan_velocity = config.init_car_velocity;
                    ctx.caravan_pos = config.init_car_position;

                    ctx.init_caravan_move_type = (Caravans.EN_caravan_move_type)config.init_car_move_type;

                    ctx.caravan_status_acc = (WorldEnum.EN_caravan_status_acc)config.init_car_status_acc;
                    ctx.caravan_status_liftoff = (WorldEnum.EN_caravan_status_liftoff)config.init_car_status_liftoff;
                }

                AutoCodes.scenes.TryGetValue($"{scene_id}", out ctx.r_scene);

                Debug.Log($"========  已加载子场景：{ctx.r_scene.id}，序列：{scene_index}/{ctx.r_level.scene_count - 1}，场景类型：{ctx.scene_type}  ========");
            }
        }


        public void btn_back_initScene()
        {
            Game_Mgr.on_exit_world(WorldContext.instance.world_progress);
            back_initScene();
        }

        public void TimeFreeze(bool b)
        {
            if (b)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }


        public void back_initScene()
        {
            SceneLoad_Utility.load_scene_async("InitScene");
        }


        public void goto_testScene()
        {
            SceneLoad_Utility.load_scene_with_loading("TestScene", true);
        }

        public void back_campScene()
        {
            /*Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            bmgr.Settlement();*/

            SceneLoad_Utility.load_scene("CampScene");
        }

        public void BlackScreen()
        {
            Share_DS.instance.add("is_world_fade_black", true);

            black_screen.transform.SetAsLastSibling();
            StartCoroutine("IBlackScreenCoroutine");
        }

        public void FadeBlackScreen()
        {
            if (!Share_DS.instance.try_get_value("is_world_fade_black", out bool is_world_fade_black) || !is_world_fade_black)
                return;

            Share_DS.instance.remove("is_world_fade_black");

            black_screen.transform.SetAsLastSibling();
            StartCoroutine("IFadeBlackScreenCoroutine");
        }

        private IEnumerator IBlackScreenCoroutine()
        {
            var color = black_screen.color;
            black_screen.color = new Color(color.r, color.g, color.b,0);
            black_screen.gameObject.SetActive(true);
            float alpha = 0;
            while(alpha < 1)
            {
                alpha += 1f/Config.current.safearea_enter_transition_ticks;
                black_screen.color = new Color(color.r, color.g, color.b, alpha);
                yield return new WaitForSeconds(Config.PHYSICS_TICK_DELTA_TIME);
            }

            black_screen.gameObject.SetActive(false);
            safe_area.gameObject.SetActive(true);
            safe_area.Init();

            Time.timeScale = 0;
        }

        private IEnumerator IFadeBlackScreenCoroutine()
        {
            Time.timeScale = 1;

            var color = black_screen.color;
            black_screen.color = new Color(color.r, color.g, color.b, 1);
            black_screen.gameObject.SetActive(true);
            float alpha = 1;
            while (alpha >0)
            {
                alpha -= 1f / Config.current.safearea_enter_transition_ticks;
                black_screen.color = new Color(color.r, color.g, color.b, alpha);
                yield return new WaitForSeconds(Config.PHYSICS_TICK_DELTA_TIME);
            }

            black_screen.gameObject.SetActive(false);
            //safe_area.SetActive(true);
        }


        public void ExitSafeArea()
        {
           /* //创建路线
            Safe_Area_Helper.create_routes();

            //呼出菜单
            Share_DS.instance.try_get_value(Game_Mgr.key_routes, out List<RouteData> routes);

            //选择路线 + 离开
            Safe_Area_Helper.select_route(0);*/
            Safe_Area_Helper.leave();
        }

        public void SetSettlement(bool success)
        {
            settlement_panel.gameObject.SetActive(true);
            settlement_panel.Init(success);
        }

        public bool try_find_pd<T>(out T producer) where T : class
        {
            producer = default;
            if (producers == null) return false;

            producer = producers.Where(t => t is T).First() as T;
            return producer != null;
        }
    }
}

