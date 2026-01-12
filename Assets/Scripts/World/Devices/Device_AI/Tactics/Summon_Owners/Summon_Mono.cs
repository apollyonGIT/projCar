using Commons;
using Foundations.Tickers;
using System;
using System.Linq;
using UnityEngine;

namespace World.Devices
{
    public class Summon_Mono : MonoBehaviour
    {
        protected Summon_Owner owner;

        protected Vector2 m_pos_offset;
        protected Vector2 m_pos => WorldContext.instance.caravan_pos + m_ori_pos_offset;

        protected Vector2 m_ori_pos_offset;

        string m_GUID;

        bool m_is_active;
        public bool is_active => m_is_active;

        bool m_is_init;

        const int C_survial_tick = 5 * Config.PHYSICS_TICKS_PER_SECOND;
        const float C_fly_speed = 1.5f;
        const float C_valid_target_pos_dis = 0.5f;

        //==================================================================================================

        public void on_init(Summon_Owner owner, Vector2 target_pos)
        {
            this.owner = owner;
            m_pos_offset = target_pos - WorldContext.instance.caravan_pos;

            transform.localPosition = target_pos;
            m_GUID = Guid.NewGuid().ToString();

            m_ori_pos_offset = owner.position - WorldContext.instance.caravan_pos;

            m_is_init = true;
        }


        void active()
        {
            Request_Helper.delay_do("Summon_Mono_Fini" + m_GUID, C_survial_tick, (_) => { on_fini(); });

            m_is_active = true;
        }


        public void on_tick()
        {
            if (!m_is_init) return;

            if (!m_is_active)
            {
                m_ori_pos_offset = Vector2.Lerp(m_ori_pos_offset, m_pos_offset, C_fly_speed * Config.PHYSICS_TICK_DELTA_TIME);
                transform.localPosition = m_ori_pos_offset + WorldContext.instance.caravan_pos;
                if (Vector3.Distance(m_ori_pos_offset, m_pos_offset) < C_valid_target_pos_dis)
                {
                    active();
                }

                return;
            }

            transform.localPosition = m_pos;

            do_bll_tick();
        }


        public void on_fini()
        {
            m_is_active = false;

            var reqs = Request_Helper.query_request("Summon_Mono_Fini" + m_GUID);
            if (reqs.Any())
            {
                foreach (var req in reqs)
                {
                    req.interrupt();
                }
            }

            owner.kill_summon(this);

            DestroyImmediate(gameObject);
        }


        public virtual void do_bll_tick()
        { 
        }
    }
}

