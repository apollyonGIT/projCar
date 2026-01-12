using Commons;
using Foundations;
using System.Linq;
using UnityEngine;
using World.BackPack;
using World.Devices;
using World.Enemys;
using World.Helpers;
using World.Progresss;
using static Commons.Attributes;

namespace World
{
    public class World_Console_Code
    {
        //[Detail("【无敌】whosyourdaddy")]
        //public static void whosyourdaddy()
        //{
        //    var ctx = WorldContext.instance;

        //    if (ctx.is_player_seckill)
        //        Debug.Log("whosyourdaddy，已激活");
        //    else
        //        Debug.Log("whosyourdaddy，已取消");
        //}


        public static void help()
        {
            Console_Code_Helper.help(typeof(World_Console_Code));
        }


        //public static void cloud()
        //{
        //    Console_Code_Helper.cloud();
        //}


        [Detail("【一键清屏】clear")]
        public static void clear()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                (cell as ITarget).hurt(null, new Attack_Data()
                {
                    atk = 9999999,
                },
                out _);
            }
        }


        [Detail("【存档】save")]
        public static void save()
        {
            SL_Helper.save_game();
        }


        [Detail("【读档】load")]
        public static void load()
        {
            SL_Helper.load_game();
        }


        public static void dice()
        {
            ProgressEvent progressEvent = new();
            progressEvent.record = new();
            progressEvent.record.dialogue_graph = "dice_000";

            Encounters.Dialogs.Encounter_Dialog.instance.init(progressEvent, "Dice_Game_Window");
        }


        [Detail("【测试敌方小车】car")]
        public static void car()
        {
            if (!Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr mgr)) return;

            mgr.pd.add_enemy_directly_req(0, 211101u, new(1, 0), "None");
        }


        [Detail("【测试boss】boss")]
        public static void boss()
        {
            if (!Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr mgr)) return;

            for (int i = 0; i < 1; i++)
            {
                var _pos = Vector2.zero + Random.insideUnitCircle * 4f;
                mgr.pd.add_enemy_directly_req(0, 201860u, _pos, "Default");
            }
        }


        [Detail("【测试蝙蝠群】bat")]
        public static void bat()
        {
            if (!Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr mgr)) return;

            for (int i = 0; i < 40; i++)
            {
                var _pos = Vector2.zero + Random.insideUnitCircle * 1f;
                mgr.pd.add_enemy_directly_req(0, 201801u, _pos, "Default");
            }
        }


        [Detail("【测试骑手群】rider")]
        public static void rider()
        {
            if (!Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr mgr)) return;

            for (int i = 0; i < 6; i++)
            {
                var _pos = new Vector2(10, 0) + Random.insideUnitCircle * 1f;
                mgr.pd.add_enemy_directly_req(0, 201105u, _pos, "Default");
            }

            for (int i = 0; i < 6; i++)
            {
                var _pos = new Vector2(-8, 0) + Random.insideUnitCircle * 1f;
                mgr.pd.add_enemy_directly_req(0, 201105u, _pos, "Default");
            }
        }


        [Detail("【测试骑手群_后】rider1")]
        public static void rider1()
        {
            if (!Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr mgr)) return;

            for (int i = 0; i < 10; i++)
            {
                var _pos = new Vector2(-8, 0) + Random.insideUnitCircle * 1f;
                mgr.pd.add_enemy_directly_req(0, 201105u, _pos, "Default");
            }
        }


        [Detail("【buff测试】fire")]
        public static void fire()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                Enemy_Buff_Helper.buff_fire(cell, 600);
            }
        }


        [Detail("【buff测试】acid")]
        public static void acid()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                Enemy_Buff_Helper.buff_acid(cell, 600);
            }
        }


        [Detail("【buff测试】blind")]
        public static void blind()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                Enemy_Buff_Helper.buff_blind(cell, 600);
            }
        }


        [Detail("【buff测试】bind")]
        public static void bind()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                Enemy_Buff_Helper.buff_bind(cell);
            }
        }


        [Detail("【buff测试】stun")]
        public static void stun()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                Enemy_Buff_Helper.buff_stun(cell, 600);
            }
        }


        [Detail("【加钱】greedisgood")]
        public static void greedisgood()
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            bmgr.coin_count = 100000;
        }


        [Detail("【跳转子场景】to(id)")]
        public static void to(string id)
        {
            var ctx = WorldContext.instance;

            var scene_id = int.Parse(id);
            Share_DS.instance.add("diy_scene_id", scene_id);

            Game_Mgr.on_enter_world(ctx.world_id);
            WorldSceneRoot.instance.goto_testScene();
        }

        [Detail("【切换天气】changeweather(id)")]

        public static void changeweather(string id)
        {
            var weather_id = int.Parse(id);
            Mission.instance.try_get_mgr("Weather", out Weather.WeatherMgr wmgr);
            wmgr.ChangeWeather(weather_id);
        }


        [Detail("【切换天气_下雨】rain")]

        public static void rain()
        {
            Mission.instance.try_get_mgr("Weather", out Weather.WeatherMgr wmgr);
            wmgr.ChangeWeather(460802);
        }


        [Detail("【buff测试】buffdevice(buff_type,buff_amount)")]
        public static void buffdevice(string buff_type, string buff_amount)
        {
            var amount = int.Parse(buff_amount);

            Mission.instance.try_get_mgr("DeviceMgr", out DeviceMgr dmgr);
            foreach (var slot in dmgr.slots_device)
            {
                var device = slot.slot_device;
                if (device != null)
                {
                    (device as ITarget).applied_outlier(null, buff_type, amount);
                }
            }
        }

        [Detail("【结算战利品】settlement")]
        public static void settlement()
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            bmgr.Settlement(true);
        }


        [Detail("【小车匀速跑】run")]
        public static void run()
        {
            ref var is_run_mode = ref WorldContext.instance.is_run_mode;
            is_run_mode = !is_run_mode;
        }


        [Detail("【扣血测试】t")]
        public static void t()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                (cell as ITarget).hurt(null, new Attack_Data()
                {
                    atk = 10,
                },
                out _);
            }
        }


        [Detail("【加血测试】h")]
        public static void h()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                (cell as ITarget).hurt(null, new Attack_Data()
                {
                    atk = 300,
                    diy_atk_str = "acid"
                },
                out _);
            }
        }


        [Detail("【速进安全区】safearea")]
        public static void safearea()
        {
            Safe_Area_Helper.enter();
        }


        [Detail("【击飞】impact")]
        public static void impact()
        {
            Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr);
            foreach (var cell in emgr.cells)
            {
                var target = (ITarget)cell;
                target.impact(WorldEnum.impact_source_type.melee, Vector2.zero, new Vector2(1, 1), 5f);
            }
        }


        [Detail("【打开背包】bag")]
        public static void bag()
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr mgr);
            var view = (BackPackMgrView)mgr.views.Where(t => t is BackPackMgrView).First();

            view.gameObject.SetActive(true);
        }


        [Detail("【物品测试】money")]
        public static void money()
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            for (int i = 0; i < 5; i++)
            {
                //bmgr.AddLoot(Config.current.coin_id);
                bmgr.AddLoot(6010);
            }

        }

        [Detail("【角色昏迷】role_coma")]
        public static void role_coma()
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out Characters.CharacterMgr cmgr);
            cmgr.AddState(new Characters.CharacterStates.Coma()
            {
                duration = 1200,
            });
        }


        [Detail("【模拟掉落】loot")]
        public static void loot()
        {
            for (int i = 0; i < 10; i++)
            {
                Drop_Loot_Helper.drop_loot(600810u);
            }
        }
    }
}

