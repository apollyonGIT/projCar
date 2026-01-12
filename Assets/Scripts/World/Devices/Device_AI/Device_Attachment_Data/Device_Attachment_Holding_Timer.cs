using System;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Holding_Timer : Device_Attachment
    {
        public Device_Attachment_Holding_Timer(Action timer_up_action)
        {
            on_timer_is_up += timer_up_action;
        }

        // -------------------------------------------------------------------------------

        #region Const
        private const int TIMER_RANGE_TICKS = 120; // 触发所需的按住时间
        #endregion

        // -------------------------------------------------------------------------------

        readonly private Action on_timer_is_up;

        private int main_timer;
        private bool is_holding;

        // ===============================================================================

        public void Tick_Update(bool can_holding)
        {
            main_timer += (is_holding && can_holding) ? 1 : -5;
            main_timer = Mathf.Clamp(main_timer, 0, TIMER_RANGE_TICKS);

            if (is_holding)
                is_holding = false;

            if (main_timer >= TIMER_RANGE_TICKS)
            {
                on_timer_is_up?.Invoke();
                main_timer = 0;
            }
        }

        public float Get_Timer_Range_01()
        {
            return (float)main_timer / TIMER_RANGE_TICKS;
        }

        public void Set_Is_Holding()
        {
            is_holding = true;
        }

    }
}