using Commons;
using Foundations;
using Foundations.Tickers;
using System.Linq;

namespace World.Tensions
{
    public class TensionMgr : IMgr
    {
        int m_tension_progress;
        public int tension_progress => m_tension_progress;

        const int tension_progress_limit = (int)1e7;

        public WorldContext ctx;

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        //==================================================================================================

        public TensionMgr(string name, int priority, params object[] args)
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

            ctx = WorldContext.instance;
        }


        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
        }


        void tick()
        {
            tension_change();
            pressure_change();
        }


        void tension_change()
        {
            ref var area_tension = ref ctx.area_tension;
            if (area_tension >= 10)
            {
                m_tension_progress = 0;
                return;
            }

            m_tension_progress += (int)(ctx.caravan_velocity.x * Config.PHYSICS_TICK_DELTA_TIME * 1e4);
            if (m_tension_progress >= tension_progress_limit)
            {
                m_tension_progress -= tension_progress_limit;
                area_tension += 1;
            }
        }


        void pressure_change()
        {
            ref var pressure_stage = ref ctx.pressure_stage;
            ref var pressure = ref ctx.pressure;

            if (pressure_stage == WorldEnum.EN_pressure_stage.peace)
            {
                pressure += ctx.caravan_velocity.x * Config.PHYSICS_TICK_DELTA_TIME;
                pressure += (ctx.area_tension + 10) * ctx.pressure_growth_coef * Config.PHYSICS_TICK_DELTA_TIME;

                ctx.kill_score = 0;
            }

            if (pressure > ctx.pressure_threshold)
            {
                pressure_stage = WorldEnum.EN_pressure_stage.tide;
                Mission.instance.try_get_mgr("EnemyMgr", out Enemys.EnemyMgr enemy_mgr);

                Request req = new("pressure_stage_enter_peace", pressure_stage_enter_peace_condition, 
                    (req) => { req.countdown = 0; },
                    (_) => { ctx.pressure_stage = WorldEnum.EN_pressure_stage.peace; },
                    (req) => { req.countdown++; });


                #region 子函数 pressure_stage_enter_peace_condition
                bool pressure_stage_enter_peace_condition(Request req)
                {
                    if (req.countdown > Config.current.time_pressure_max * Config.PHYSICS_TICKS_PER_SECOND) return true;

                    return ctx.kill_score >= Config.current.pressure_kill_score && req.countdown > Config.current.time_pressure_min * Config.PHYSICS_TICKS_PER_SECOND;
                }
                #endregion
            }

            if (pressure_stage == WorldEnum.EN_pressure_stage.tide)
            {
                pressure = 0;
            }


            
        }
    }
}