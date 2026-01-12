using Commons;
using Foundations.Tickers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.CaravanBoards
{
    public class SpeedChargeView : MonoBehaviour
    {
        public GameObject obj_cd;
        public Image bg_shadow;

        int m_spring_cd_ticks;
        int m_spring_cd_remain_ticks;

        int m_spring_ticks;
        int m_spring_remain_ticks;

        bool m_is_end_charge;

        //==================================================================================================

        private void Start()
        {
            obj_cd.SetActive(false);
        }


        public void @do()
        {
            Request req = new("SpeedCharge_CD",
                (_) => { return m_spring_cd_remain_ticks <= 0; },

                (_) => {
                    notify_on_start_charge();
                    obj_cd.SetActive(true);
                },

                (_) => { obj_cd.SetActive(false); },

                (_) => {
                    if (!m_is_end_charge)
                        notify_on_charging();

                    bg_shadow.fillAmount = m_spring_cd_remain_ticks / (float)m_spring_cd_ticks;
                    m_spring_cd_remain_ticks--;
                }
            );
        }


        bool valid_is_end_charge()
        {
            var ctx = WorldContext.instance;

            var c1 = m_spring_remain_ticks <= 0; //冲刺持续时间结束
            var c2 = ctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.braking; //小车进入刹车状态
            var c3 = ctx.scene_remain_progress <= 0; //进入安全区 ≈ 判断剩余进度
            var c4 = ctx.caravan_hp <= 0; //玩家战败

            return c1 || c2 || c3 || c4;
        }


        void notify_on_start_charge()
        {
            var ctx = WorldContext.instance;
            var config = Config.current;

            //Step1. 使玩家小车立刻获得一个速度
            ctx.caravan_vx_stored += config.sprint_vx_burst;
            ctx.caravan_status_acc = WorldEnum.EN_caravan_status_acc.driving;

            //Step2. 使小车进入冲刺状态，持续帧数为 sprint_ticks
            m_spring_ticks = config.sprint_ticks;
            m_spring_remain_ticks = m_spring_ticks;

            //Step3. 使冲刺按钮进入cd，且cd剩余帧数为 spring_cd_ticks
            m_spring_cd_ticks = config.sprint_cd_ticks;
            m_spring_cd_remain_ticks = m_spring_cd_ticks;

            //Common
            m_is_end_charge = false;
        }


        void notify_on_charging()
        {
            var ctx = WorldContext.instance;
            var config = Config.current;

            //Step1. 保证玩家的油门不会降低到 spring_lever_min 以下
            ctx.driving_lever = Mathf.Max(ctx.driving_lever, config.sprint_lever_min);

            //Step2. 在冲刺持续时间内，玩家会持续获得一定的速度
            ctx.caravan_vx_stored += config.sprint_vx_keep;

            //Common
            m_spring_remain_ticks--;

            if (valid_is_end_charge())
            {
                m_is_end_charge = true;
            }
        }
    }
}

