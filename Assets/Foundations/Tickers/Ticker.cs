using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Foundations.Tickers
{
    public class Ticker : Singleton<Ticker>
    {
        public bool can_start_tick = false;
        public bool is_end = false;

        float m_physics_tick_timer = 0f;
        float m_delta_time;

        List<tick_action> m_change_tick_actions = new();
        Dictionary<string, tick_action> m_tick_actions = new();
        List<tick_action> m_change_tick_after_actions = new();
        Dictionary<string, tick_action> m_tick_after_actions = new();

        List<tick_action> m_change_tick1_actions = new();
        Dictionary<string, tick_action> m_tick1_actions = new();
        List<tick_action> m_change_tick2_actions = new();
        Dictionary<string, tick_action> m_tick2_actions = new();
        List<tick_action> m_change_tick3_actions = new();
        Dictionary<string, tick_action> m_tick3_actions = new();

        public event Action do_when_tick_start;
        public event Action do_when_tick_end;
        public event Action do_when_tick_request;

        float m_refresh_coef;

        //==================================================================================================

        struct tick_action
        {
            public EN_tick_action_state state;
            public string name;
            public Action action;
            public int priority;
        }


        enum EN_tick_action_state
        {
            none,
            add,
            remove
        }

        //==================================================================================================

        public void init(float delta_time)
        {
            //可改写：配置的帧数
            m_delta_time = delta_time;
            Physics2D.autoSyncTransforms = false;
            Physics2D.simulationMode = SimulationMode2D.Script;

            var refresh_rate = (float)Screen.currentResolution.refreshRateRatio.value;
            refresh_rate = Mathf.Min(1 / delta_time, refresh_rate);
            m_refresh_coef = 1 / delta_time / refresh_rate;
        }


        /// <summary>
        /// 固定帧率管理
        /// </summary>
        public void update()
        {
            if (!can_start_tick) return;

#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPaused)
            {
                m_physics_tick_timer = 0;
                do_tick();
                return;
            }
#endif
            var d = Mathf.Min(Time.deltaTime, m_delta_time * Time.timeScale * m_refresh_coef);
            m_physics_tick_timer += d;

            while (m_physics_tick_timer >= m_delta_time)
            {
                m_physics_tick_timer -= m_delta_time;

                do_tick();
            }
        }


        void do_tick()
        {
            tick_start();
            if (is_end) return;

            do_when_tick_request?.Invoke();

            tick();
            tick1();
            Physics2D.SyncTransforms();
            Physics2D.Simulate(m_delta_time);
            tick2();
            tick3();

            tick_end();
        }


        void do_tick_actions(ref List<tick_action> change_list, ref Dictionary<string, tick_action> dic)
        {
            //动态增减physics_tick列表
            if (change_list.Any())
            {
                foreach (var e in change_list)
                {
                    if (e.state == EN_tick_action_state.add)
                        EX_Utility.dic_cover_add(ref dic, e.name, e);
                    else if (e.state == EN_tick_action_state.remove)
                        dic.Remove(e.name);
                }

                dic = dic.OrderBy(e => e.Value.priority).ToDictionary(e => e.Key, e => e.Value);
                change_list.Clear();
            }

            //执行
            foreach (var info in dic)
            {
                var k = info.Key;
                var v = info.Value;

                var action = dic[k].action;
                action?.Invoke();
            }
        }


        void tick()
        {
            do_tick_actions(ref m_change_tick_actions, ref m_tick_actions);
        }


        /// <summary>
        /// 外部调用： 增加physics_tick方法
        /// </summary>
        public void add_tick(int priority, string name, Action action)
        {
            var e = new tick_action()
            {
                name = name,
                action = action,
                priority = priority,
                state = EN_tick_action_state.add
            };
            m_change_tick_actions.Add(e);
        }


        /// <summary>
        /// 外部调用： 去除physics_tick方法
        /// </summary>
        public void remove_tick(string name)
        {
            var e = new tick_action()
            {
                name = name,
                state = EN_tick_action_state.remove
            };
            m_change_tick_actions.Add(e);
        }


        void tick_start()
        {
            do_when_tick_start?.Invoke();
        }


        void tick_end()
        {
            do_tick_actions(ref m_change_tick_after_actions, ref m_tick_after_actions);

            do_when_tick_end?.Invoke();
        }


        /// <summary>
        /// 外部调用： 增加physics_tick_after方法
        /// </summary>
        public void add_tick_after(int priority, string name, Action action)
        {
            var e = new tick_action()
            {
                name = name,
                action = action,
                priority = priority,
                state = EN_tick_action_state.add,
            };
            m_change_tick_after_actions.Add(e);
        }


        /// <summary>
        /// 外部调用： 去除physics_tick_after方法
        /// </summary>
        public void remove_tick_after(string name)
        {
            var e = new tick_action()
            {
                name = name,
                state = EN_tick_action_state.remove
            };
            m_change_tick_after_actions.Add(e);
        }


        //=============================================================================================

        void tick1()
        {
            do_tick_actions(ref m_change_tick1_actions, ref m_tick1_actions);
        }


        public void add_tick1(int priority, string name, Action action)
        {
            var e = new tick_action()
            {
                name = name,
                action = action,
                priority = priority,
                state = EN_tick_action_state.add
            };
            m_change_tick1_actions.Add(e);
        }


        public void remove_tick1(string name)
        {
            var e = new tick_action()
            {
                name = name,
                state = EN_tick_action_state.remove
            };
            m_change_tick1_actions.Add(e);
        }


        void tick2()
        {
            do_tick_actions(ref m_change_tick2_actions, ref m_tick2_actions);
        }


        public void add_tick2(int priority, string name, Action action)
        {
            var e = new tick_action()
            {
                name = name,
                action = action,
                priority = priority,
                state = EN_tick_action_state.add
            };
            m_change_tick2_actions.Add(e);
        }


        public void remove_tick2(string name)
        {
            var e = new tick_action()
            {
                name = name,
                state = EN_tick_action_state.remove
            };
            m_change_tick2_actions.Add(e);
        }


        void tick3()
        {
            do_tick_actions(ref m_change_tick3_actions, ref m_tick3_actions);
        }


        public void add_tick3(int priority, string name, Action action)
        {
            var e = new tick_action()
            {
                name = name,
                action = action,
                priority = priority,
                state = EN_tick_action_state.add
            };
            m_change_tick3_actions.Add(e);
        }


        public void remove_tick3(string name)
        {
            var e = new tick_action()
            {
                name = name,
                state = EN_tick_action_state.remove
            };
            m_change_tick3_actions.Add(e);
        }


        public void clean_req<T>(T request) where T : Request
        {
            if (do_when_tick_request == null) return;

            foreach (var e in do_when_tick_request.GetInvocationList())
            {
                var t = (T)e.Target;
                if (t.req_name == request.req_name)
                    do_when_tick_request -= request.@tick;
            }
        }


        public IEnumerable<Request> query_req(string req_name_contain)
        {
            if (do_when_tick_request == null) yield break;

            foreach (var e in do_when_tick_request.GetInvocationList())
            {
                var t = (Request)e.Target;

                if (t.req_name.Contains(req_name_contain))
                    yield return t;
            }
        }
    }

}


