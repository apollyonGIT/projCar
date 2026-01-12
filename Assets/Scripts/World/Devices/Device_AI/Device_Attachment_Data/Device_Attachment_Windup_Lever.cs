using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Windup_Lever : Device_Attachment
    {
        public Device_Attachment_Windup_Lever(Device_Attachment_Triggerable_Spring spring_lever, Device_Attachment_Dir_Indicator ui_deco_dish)
        {
            this.spring_lever = spring_lever;
            this.ui_deco_dish = ui_deco_dish;
            spring_lever.on_spring_untriggered += on_spring_reset;
        }

        // -------------------------------------------------------------------------------

        #region Const
        private const float MAX_OVER_ACCUMULATED = 2F;

        private const float RELOADING_RATIO_PER_OPR_0 = 0.77F; // 每次装载量
        private const float RELOADING_RATIO_PER_OPR_1 = 0.33F; // 每次装载量
        private const float RELOADING_COEF = 5e-4F; // 装填难度修正
        private const float RELOADING_UI_DECO_DISH_ROTATION_ANGLE = 9600F;
        #endregion

        // -------------------------------------------------------------------------------

        private readonly Device_Attachment_Triggerable_Spring spring_lever;
        private readonly Device_Attachment_Dir_Indicator ui_deco_dish;

        private float wind_up_done;
        private float wind_up_expt; // [0, 1]
        private float wind_up_expt_accumulated;

        // ===============================================================================

        /// <summary>
        /// Per Tick: owner.SpringForceCurrent += return_value
        /// </summary>
        /// <param name="spring_force_current"></param>
        /// <returns></returns>
        public float Tick_Update(float spring_force_current)
        {
            if (spring_lever.Spring_Value_01 > 0)
                wind_up_expt = Mathf.Max(wind_up_expt, spring_lever.Spring_Value_01);

            var wind_up_total = get_max_windup();
            if (spring_force_current < 1 && wind_up_done < wind_up_total)
            {
                var reloading_mod_by_spring_force = Mathf.Lerp(1f, 0.5f, spring_force_current);
                var delta = (wind_up_total - wind_up_done) * Mathf.Lerp(RELOADING_RATIO_PER_OPR_0, RELOADING_RATIO_PER_OPR_1, spring_force_current) * reloading_mod_by_spring_force;
                if (delta > 0)
                {
                    var delta_mod = delta * RELOADING_COEF;
                    wind_up_done += delta_mod;
                    ui_deco_dish.Rotate_Delta_Angle(delta_mod * RELOADING_UI_DECO_DISH_ROTATION_ANGLE);
                    return delta_mod;
                }
            }

            return 0f;
        }

        /// <summary>
        /// 一般是在武器发射后调用
        /// </summary>
        public void Reset_Windup()
        {
            wind_up_expt_accumulated = 0;
            wind_up_expt = 0;
            wind_up_done = 0;
        }

        // ===============================================================================

        private void on_spring_reset()
        {
            wind_up_expt_accumulated = get_max_windup();
            wind_up_expt = 0;
        }

        private float get_max_windup()
        {
            return Mathf.Min(wind_up_expt_accumulated + wind_up_expt, wind_up_done + MAX_OVER_ACCUMULATED);
        }

    }
}