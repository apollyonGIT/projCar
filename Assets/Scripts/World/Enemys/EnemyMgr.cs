using Foundations;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Enemys
{
    public class EnemyMgr : IMgr
    {
        public LinkedList<Enemy> cells { get; private set; }
        public LinkedList<Enemy> fini_cells = new();

        public WorldContext ctx;
        public EnemyPD pd;

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public Action<EnemyMgr> outter_tick;

        //==================================================================================================

        public EnemyMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }


        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);

            ctx = WorldContext.instance;
            reset();
        }


        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }


        void tick()
        {
            foreach (var cell in cells)
            {
                if (ctx.is_need_reset)
                {
                    cell.pos.x -= ctx.reset_dis;
                }

                cell.tick();
            }

            foreach (var cell in fini_cells)
            {
                cells.Remove(cell);
                cell.remove_all_views();
            }
            fini_cells.Clear();

            //规则：计算是否为精英模式
            var is_elite_mode = query_cells_by_is_elite_group(true).Count() > 0;
            WorldContext.instance.is_elite_mode = is_elite_mode;

            outter_tick?.Invoke(this);
        }


        void tick1()
        {
            foreach (var cell in cells)
            {
                cell.tick1();
            }
        }


        public void @reset()
        {
            cells = new();
        }


        public void add_cell(Enemy cell)
        {
            cells.AddLast(cell);
        }


        public IEnumerable<Enemy> query_cells_by_id(uint id)
        { 
            return cells.Where(t => t._desc.id == id);
        }


        public IEnumerable<Enemy> query_cells_by_groupInfo(uint group_id, uint group_sub_id)
        {
            return cells.Where(t => t._group_desc != null && t._group_desc.id == group_id && t._group_desc.sub_id == group_sub_id);
        }


        public IEnumerable<Enemy> query_cells_by_is_elite_group(bool is_elite)
        {
            return cells.Where(t => t.is_elite_group);
        }
    }
}