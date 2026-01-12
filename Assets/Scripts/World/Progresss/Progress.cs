using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using Foundations.Excels;
using Foundations.MVVM;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Devices.Device_AI;
using World.Environments;
using World.Helpers;

namespace World.Progresss
{
    public interface IProgressView : IModelView<Progress>
    {

        void notify_init();
        void notify_on_tick();
        /// <summary>
        /// 提醒玩家 已经接近奇遇了
        /// </summary>
        /// <param name="p"></param>
        /// <param name="b"></param>
        void notify_notice_encounter(float p, bool b);

        void notify_add_progress_event(ProgressEvent pe);

        void notify_remove_progress_event(ProgressEvent pe);

        void notify_enter_progress_event(ProgressEvent pe);

        void notify_arrived_progress_event(ProgressEvent pe);

        void notify_exit_progress_event(ProgressEvent pe);
    }

    public struct single_plot
    {
        public uint event_id;
        public float trigger_progress;      //触发的位置
        public bool ui_visible;
    }


    public class Progress : Model<Progress, IProgressView>
    {
        WorldContext ctx;

        public LinkedList<(uint mg_id, float mg_progress_min, float mg_progress_max)> monster_group_infos = new();
        public List<(uint mg_id, int weight, float length, string choose_road_context)> monster_group_select_list = new();

        public uint? mg_pursuing_id;
        bool m_is_load_pursuing = true;

        Enemys.EnemyPD enemyPD;

        //==================================================================================================

        public float total_progress => m_total_progress;
        public float m_total_progress;

        public float current_progress
        {
            get
            {
                return m_current_progress;
            }
            set
            {
                m_current_progress = Mathf.Clamp(value, 0f, m_total_progress);
            }
        }
        private float m_current_progress;


        public List<single_plot> single_plots = new();      //记录的是随机后生成的奇遇们的基础数据（未触发）


        public List<ProgressEvent> progress_events = new(); //记录的是所有生成的progress 但不一定触发了

        public void tick()
        {
            if (ctx.is_need_reset)
            {
                foreach (var pe in progress_events)
                {
                    pe.pos -= new Vector2(ctx.reset_dis, 0);
                }
            }

            foreach (var view in views)
            {
                view.notify_on_tick();
            }

            for (int i = progress_events.Count - 1; i >= 0; i--)
            {
                progress_events[i].tick();

                if (progress_events[i].need_remove == true)
                {
                    foreach (var view in views)
                    {
                        view.notify_remove_progress_event(progress_events[i]);
                    }

                    progress_events.RemoveAt(i);
                    trigger_next_event();
                }
            }

            //录入当前进度
            ctx.scene_remain_progress = m_total_progress - current_progress;
            ctx.scene_progress_rate = Mathf.Min(current_progress / m_total_progress, 1f);

            //按进度释放怪物组
            if (monster_group_infos.Any())
            {
                var (mg_id, mg_progress_min, mg_progress_max) = monster_group_infos.First.Value;
                if (ctx.scene_progress_rate >= mg_progress_min)
                {
                    enemyPD.load_monster_group(mg_id, end_load_monster_group);

                    monster_group_infos.RemoveFirst();

                    #region 子函数 end_load_monster_group
                    bool end_load_monster_group()
                    {
                        return ctx.scene_progress_rate > mg_progress_max;
                    }
                    #endregion
                }
            }

            //释放追兵怪物组
            if (m_is_load_pursuing && mg_pursuing_id.HasValue)
            {
                m_is_load_pursuing = false; //规则：仅释放一次，因为追兵是贯穿场景的
                enemyPD.load_monster_group(mg_pursuing_id.Value, end_load_pursuing_monster_group, set_pursuing_flag);
                
                WorldSceneRoot.instance.enemy_chase_warning.init((Enemys.EnemyMgr)enemyPD.imgr); //边缘警告表现

                #region 子函数 end_load_pursuing_monster_group
                bool end_load_pursuing_monster_group()
                {
                    return ctx.scene_progress_rate >= 1f;
                }
                #endregion

                #region 子函数 set_pursuing_flag
                void set_pursuing_flag(Enemys.Enemy cell)
                {
                    cell.is_pursuing = true;
                    WorldSceneRoot.instance.enemy_chase_warning.add_chase_mark(cell); //感叹号表现
                }
                #endregion
            }
        }


        public void tick1()
        {

        }

        
        public void Init()
        {
            ctx = WorldContext.instance;
            var r_scene = ctx.r_scene;
            var config = Config.current;

            monster_group_select_list = ctx.routeData.mg_data;
            m_total_progress = ctx.routeData.scene_total_length;

            //X. 其他数据加载
            Mission.instance.try_get_mgr("EnemyMgr", out Enemys.EnemyMgr enemyMgr);
            enemyPD = enemyMgr.pd;

            current_progress = 0f;
            ctx.scene_remain_progress = m_total_progress;
            ctx.scene_progress_rate = 0;

            //录入怪物组信息
            var temp_pos = config.scene_distance_begin;
            foreach (var mg_cell in monster_group_select_list)
            {
                var mg_id = mg_cell.mg_id;
                var mg_start_pos = temp_pos;
                var mg_end_pos = temp_pos + mg_cell.length;

                monster_group_infos.AddLast((mg_id, mg_start_pos / m_total_progress, mg_end_pos / m_total_progress));

                temp_pos = mg_end_pos + config.scene_distance_interval;
            }

            //录入追兵怪物组信息
            m_is_load_pursuing = true;
            mg_pursuing_id = r_scene.MG_pursuing == 0u ? null : r_scene.MG_pursuing;
        }


        public void Move(float dis)
        {
            //规则：精英战时锁定进度
            if (ctx.is_elite_mode) return;

            m_current_progress += dis;
            foreach (var view in views)
            {
                view.notify_notice_encounter(0, false);
            }

            foreach (var pe in progress_events)
            {
                if (m_current_progress > pe.trigger_progress - Config.current.notice_length_1 && m_current_progress < pe.trigger_progress - Config.current.notice_length_2)
                {
                    foreach (var view in views)
                    {
                        view.notify_notice_encounter(pe.trigger_progress, true);
                    }
                    var x = pe.pos.x;
                    Road_Info_Helper.try_get_altitude(x, out var altitude);
                    pe.pos.y = altitude + 2;
                }

                if (Mathf.Abs(m_current_progress - pe.trigger_progress) <= 0.5f && pe.notice_arrived == false)
                {
                    pe.Arrived();

                    foreach (var view in views)
                    {
                        view.notify_arrived_progress_event(pe);
                    }
                }

                if (m_current_progress > pe.trigger_progress - Config.current.trigger_length && m_current_progress < pe.trigger_progress + Config.current.trigger_length && pe.notice_triggered == false)
                {
                    pe.Enter();

                    foreach (var view in views)
                    {
                        view.notify_enter_progress_event(pe);
                    }
                }
                else if (m_current_progress > pe.trigger_progress + Config.current.trigger_length)
                {
                    pe.Exit();

                    foreach (var view in views)
                    {
                        view.notify_exit_progress_event(pe);
                    }
                }

            }
        }

        private void trigger_next_event()
        {
            if (single_plots.Count > 0)
            {
                var event_id = single_plots[0].event_id;

                event_sites.TryGetValue(event_id.ToString(), out var event_site_record);

                Mission.instance.try_get_mgr(Config.EnvironmentMgr_Name, out EnvironmentMgr emgr);
                var dis = single_plots[0].trigger_progress - current_progress;
                if (event_site_record.site_type.value == Site_Type.EN_Site_Type.Dialog)
                {
                    if (event_site_record.prefeb != null)
                    {
                        Debug.Log(event_site_record.prefeb);
                        Addressable_Utility.try_load_asset(event_site_record.prefeb, out EncounterObjects objs);            //奇遇这个为纯外观 不带一丝逻辑
                        if (objs != null)
                        {
                            emgr.AddEncounterObj(objs, new Vector2(WorldContext.instance.caravan_pos.x + dis, 0));
                        }
                    }
                }
                else if (event_site_record.site_type.value == Site_Type.EN_Site_Type.Monster)
                {

                }
                else if (event_site_record.site_type.value == Site_Type.EN_Site_Type.Loot)
                {

                }

                var pos = new Vector2(WorldContext.instance.caravan_pos.x + dis, 3);
                var pe = new ProgressEvent(current_progress + dis, event_site_record, pos);
                progress_events.Add(pe);
                ProgressModule pm = new ProgressModule()
                {
                    pe = pe,
                };
                pe.module = pm;
                foreach (var view in views)
                {
                    view.notify_add_progress_event(pe);
                }

                single_plots.RemoveAt(0);
            }

            return;
        }
    }
}





