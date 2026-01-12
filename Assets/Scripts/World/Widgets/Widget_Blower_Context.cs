using AutoCodes;
using Commons;
using Foundations;
using Foundations.Tickers;
using UnityEngine;

namespace World.Widgets
{
    public class Widget_Blower_Context : Singleton<Widget_Blower_Context>
    {
        public device_wheel wheel_desc;
        float wheel_blower_acc => wheel_desc.blower_charge;
        float wheel_blower_decline => wheel_desc.blower_decline;

        public int trigger_ticks; //玩家按下风箱后经过的帧数
        public float lever_up_rate; //油门上升速率
        public float lever_acc_bonus; //油门变速加成

        bool m_is_blowering;

        //==================================================================================================

        public void attach()
        {
            Ticker.instance.do_when_tick_start += tick;
        }


        public void detach()
        {
            Ticker.instance.do_when_tick_start -= tick;
        }


        public void tick()
        {
            var ctx = WorldContext.instance;

            //风箱变化
            if (m_is_blowering)
                notify_on_blowering();

            //油门变化
            ref var car_lever = ref ctx.driving_lever;
            ref var car_vx = ref ctx.caravan_velocity.x;

            //Setp1. 判断当前油门是否大于1，并依此判断是否进入后续流程
            if (car_lever <= 0) return;

            //Setp2. 判断小车水平车速是否小于等于0，并依次判断是否进入后续流程
            if (car_vx <= 0) return;

            //Setp3. 使油门缓缓降低
            car_lever -= Mathf.Min(car_vx * ctx.total_weight * 0.001f * wheel_blower_decline, 1) * (car_lever - 1);

        }


        public void notify_on_start_blower()
        {
            //规则：按下风箱，初始化数据
            trigger_ticks = 0;
            lever_up_rate = 0;

            WorldContext.instance.caravan_status_acc = WorldEnum.EN_caravan_status_acc.driving;

            m_is_blowering = true;
        }


        public void notify_on_end_blower()
        {
            m_is_blowering = false;
        }


        void notify_on_blowering()
        {
            var config = Config.current;
            var ctx = WorldContext.instance;
            ref var car_lever = ref ctx.driving_lever;

            //Setp1. 实时记录当前的长按帧数，但使其不超过最大值
            if (trigger_ticks < config.blower_ticks_max)
                trigger_ticks++;

            //Setp2. 根据当前帧数，使油门上升速率产生不同的变化
            if (trigger_ticks <= config.blower_ticks_max / (config.blower_stage_num - 1))
                lever_up_rate += wheel_blower_acc * config.blower_stage_num;
            else
                lever_up_rate -= wheel_blower_acc;

            //Setp3. 确保 油门上升速率 不小于零
            lever_up_rate = Mathf.Max(0, lever_up_rate);

            //Setp4. 计算 油门变速加成
            lever_acc_bonus = Mathf.Max(1, Mathf.RoundToInt((1 - car_lever) * config.blower_low_lever_bonus));

            //Setp5. 更新小车油门大小
            car_lever += lever_acc_bonus * lever_up_rate;

        }
    }
}
