using Addrs;
using Commons;
using Foundations;
using Foundations.Tickers;
using System.Linq;
using UnityEngine;
using World.Helpers;

namespace World.Enemys
{
    public class EnemyPD : Producer
    {
        public override IMgr imgr => mgr;
        EnemyMgr mgr;
        WorldContext ctx;

        //==================================================================================================

        public override void init(int priority)
        {
            ctx = WorldContext.instance;

            mgr = new("EnemyMgr", priority);
            mgr.pd = this;
        }


        public void load_monster_group(uint monster_group_id, System.Func<bool> end_func, System.Action<Enemy> diy_cell_ac = null)
        {
            if (!Config.current.is_load_enemys) return;

            var raw_monster_groups = AutoCodes.mg_scenes.records;
            var monster_groups = raw_monster_groups.Select(t => t.Value).Where(t => t.id == monster_group_id).ToList();

            foreach (var monster_group in monster_groups)
            {
                if (monster_group.is_elite == true)
                {
                    add_elite_group_enemy_req($"add_enemy_req_{monster_group.sub_id}_{monster_group.monster_id}", monster_group, end_func, diy_cell_ac);

                    continue;
                }  

                add_enemy_req($"add_enemy_req_{monster_group_id}_{monster_group.sub_id}_{monster_group.monster_id}", monster_group, end_func, diy_cell_ac);
            }
        }


        public override void call()
        {
        }


        public Request add_enemy_req(string req_name, AutoCodes.mg_scene r, System.Func<bool> end_func, System.Action<Enemy> diy_cell_ac = null)
        {
            Request req = new(req_name, 
                (_) => { return end_func.Invoke(); }, 
                req_start, 
                (_) => { Debug.Log($"======== 怪物组加载结束，序号{req_name}，进度{ctx.scene_progress_rate} ========"); }, 
                (req) => 
                {
                    if (ctx.is_elite_mode) return;
                    try_create_single_cell(req);
                });

            return req;

            #region 子函数 req_start
            void req_start(Request req)
            {
                Debug.Log($"======== 怪物组加载开始，序号{req_name}，进度{ctx.scene_progress_rate} ========");

                var prms_dic = req.prms_dic;

                prms_dic.Add("progress", Random.Range(r.spawn_init.x, r.spawn_init.y)); //进度初动
                prms_dic.Add("progress_max", Random.Range(r.spawn_threshold.x, r.spawn_threshold.y)); //进度上限

                prms_dic.Add("r", r);
            }
            #endregion


            #region 子函数 try_create_single_cell
            void try_create_single_cell(Request req)
            {
                var r = (AutoCodes.mg_scene)req.prms_dic["r"];

                ref var area_tension = ref ctx.area_tension;
                if (area_tension > r.tension_max || area_tension < r.tension_min) return;

                //规则：兽潮
                var count_max = r.count_max;
                var k_time = r.k_time;
                if (ctx.pressure_stage == WorldEnum.EN_pressure_stage.tide)
                {
                    count_max += r.tide_count_addtion;
                    k_time += r.tide_time_addtion;
                }

                var count = mgr.query_cells_by_groupInfo(r.id, r.sub_id).Count();
                if (count >= count_max)
                {
                    req.prms_dic["progress"] = 0f;
                    return;
                }

                var progress = (float)req.prms_dic["progress"];
                var progress_max = (float)req.prms_dic["progress_max"];

                if (progress >= progress_max)
                {
                    progress = 0f;
                    req.prms_dic["progress_max"] = Random.Range(r.spawn_threshold.x, r.spawn_threshold.y);

                    var generate_count = r.generate_count_area == null ? 1 : Random.Range((int)r.generate_count_area.Value.x, (int)r.generate_count_area.Value.y + 1);
                    for (int i = 0; i < generate_count; i++)
                    {
                        var _pos = r.pos + Random.insideUnitCircle * r.pos_rnd_radius;
                        var cell = create_single_cell_directly(r.monster_id, _pos, r.init_state);
                        cell._group_desc = r;

                        diy_cell_ac?.Invoke(cell);
                    }
                }

                var progress_addition = (r.k_distance * ctx.caravan_velocity.x + k_time) * Config.PHYSICS_TICK_DELTA_TIME;
                progress += Mathf.Max(progress_addition, 0);
                req.prms_dic["progress"] = progress;
            }
            #endregion
        }


        public Request add_elite_group_enemy_req(string req_name, AutoCodes.mg_scene r, System.Func<bool> end_func, System.Action<Enemy> diy_cell_ac = null)
        {
            Enemy elite_cell = null;

            Request req = new(req_name,
                (_) => { return end_func.Invoke(); },
                req_start,
                (_) => {
                    elite_cell?.bt.notify_on_enter_flee(elite_cell, r.end_state);
                    Debug.Log($"======== 怪物组加载结束，序号{req_name}，进度{ctx.scene_progress_rate} ========"); 
                },
                try_create_single_cell_in_elite_group);

            return req;

            #region 子函数 req_start
            void req_start(Request req)
            {
                Debug.Log($"======== 怪物组加载开始，序号{req_name}，进度{ctx.scene_progress_rate} ========");

                var prms_dic = req.prms_dic;

                prms_dic.Add("progress", Random.Range(r.spawn_init.x, r.spawn_init.y)); //进度初动
                prms_dic.Add("progress_max", Random.Range(r.spawn_threshold.x, r.spawn_threshold.y)); //进度上限

                prms_dic.Add("count", 0);

                prms_dic.Add("r", r);
            }
            #endregion


            #region 子函数 try_create_single_cell_in_elite_group
            void try_create_single_cell_in_elite_group(Request req)
            {
                var r = (AutoCodes.mg_scene)req.prms_dic["r"];

                //规则：兽潮
                var k_time = r.k_time;
                if (ctx.pressure_stage == WorldEnum.EN_pressure_stage.tide)
                {
                    k_time += r.tide_time_addtion;
                }

                var progress = (float)req.prms_dic["progress"];
                var progress_max = (float)req.prms_dic["progress_max"];
                var count = (int)req.prms_dic["count"];

                if (count > 0) return;

                if (progress >= progress_max)
                {
                    var _pos = r.pos + Random.insideUnitCircle * r.pos_rnd_radius;
                    var cell = create_single_cell_directly(r.monster_id, _pos, r.init_state);
                    cell._group_desc = r;
                    cell.is_elite_group = true;

                    elite_cell = cell;

                    req.prms_dic["count"] = 1;

                    diy_cell_ac?.Invoke(cell);
                }

                var progress_addition = (r.k_distance * ctx.caravan_velocity.x + k_time) * Config.PHYSICS_TICK_DELTA_TIME;
                progress += Mathf.Max(progress_addition, 0);
                req.prms_dic["progress"] = progress;
            }
            #endregion
        }


        EnemyView load_cell(Enemy cell)
        {
            mgr.add_cell(cell);

            Addressable_Utility.try_load_asset(cell.view_resource_name, out EnemyView view_asset);
            var view = Instantiate(view_asset, transform);

            cell.add_view(view);

            return view;
        }


        public void add_enemy_directly_req(int delay_tick, uint monster_id, Vector2 pos, string init_state, System.Action<Enemy> callback_ac = null)
        {
            Request_Helper.delay_do($"add_enemy_req_{monster_id}", delay_tick, 
                (_) => {
                    create_single_cell_directly(monster_id, pos, init_state, callback_ac);
                });
        }


        public Enemy create_single_cell_directly(uint monster_id, Vector2 pos, string init_state, System.Action<Enemy> callback_ac = null)
        {
            Enemy cell = Enemy_Init_Helper.init_enemy_instance(monster_id);
            cell.pos = new(pos.x + mgr.ctx.caravan_pos.x, pos.y);
            callback_ac?.Invoke(cell);

            cell.old_bt?.init(cell, init_state);
            cell.bt?.init(cell, init_state);

            var view = load_cell(cell);

            cell.bt?.load_outter_view(cell, view);
            cell.is_init = true;

            cell.do_after_init(view.gameObject);

            return cell;
        }
    }
}