using System;
using UnityEngine;

namespace World.Devices.Device_AI
{
    /// <summary>
    /// <para>提供了两个Action：</para>
    /// <para>1.on_spring_trigger, 在弹簧被触发时调用</para>
    /// <para>2.on_spring_moved，在弹簧被拖动时调用。一般是在UI中挂载，同步更新外观。</para>
    /// </summary>
    public class Device_Attachment_Triggerable_Spring : Device_Attachment
    {

        #region Ctor

        public Device_Attachment_Triggerable_Spring()
        {
            trigger_threshold = SPRING_TRIGGER_THRESHOLD;
            untrigger_threshold = SPRING_UNTRIGGERED_THRESHOLD;
            se_spring_trigger = SE_SPRING_TRIGGER;
            se_spring_untrigger = SE_SPRING_UNTRIGGER;
        }

        public Device_Attachment_Triggerable_Spring(float trigger_threshold, float untrigger_threshold)
        {
            this.trigger_threshold = trigger_threshold;
            this.untrigger_threshold = untrigger_threshold;
            se_spring_trigger = SE_SPRING_TRIGGER;
            se_spring_untrigger = SE_SPRING_UNTRIGGER;
        }

        public Device_Attachment_Triggerable_Spring(string se_trigger, string se_untrigger)
        {
            trigger_threshold = SPRING_TRIGGER_THRESHOLD;
            untrigger_threshold = SPRING_UNTRIGGERED_THRESHOLD;
            se_spring_trigger = se_trigger;
            se_spring_untrigger = se_untrigger;
        }

        public Device_Attachment_Triggerable_Spring(float trigger_threshold, float untrigger_threshold, string se_trigger, string se_untrigger)
        {
            this.trigger_threshold = trigger_threshold;
            this.untrigger_threshold = untrigger_threshold;
            se_spring_trigger = se_trigger;
            se_spring_untrigger = se_untrigger;
        }

        #endregion

        // ===============================================================================

        #region Const
        private const string SE_SPRING_TRIGGER = "se_ag_gun_bolt_load";
        private const string SE_SPRING_UNTRIGGER = "se_ag_gun_bolt_unload";

        private const float SPRING_TRIGGER_THRESHOLD = 0.9F;
        private const float SPRING_UNTRIGGERED_THRESHOLD = 0.3F;
        #endregion

        // -------------------------------------------------------------------------------

        public Action on_spring_untriggered;
        public Action on_spring_triggered;
        public Action on_spring_moved;

        // -------------------------------------------------------------------------------

        private string se_spring_trigger;
        private string se_spring_untrigger;
        private float trigger_threshold;
        private float untrigger_threshold;

        private bool _triggered;
        private float _spring_value_01;

        // ===============================================================================

        public float Spring_Value_01
        {
            get { return _spring_value_01; }
            set
            {
                _spring_value_01 = Mathf.Clamp01(value);

                if (_triggered)
                {
                    if (_spring_value_01 < untrigger_threshold)
                    {
                        _triggered = false;
                        Audio.AudioSystem.instance.PlayOneShot(se_spring_untrigger);
                        on_spring_untriggered?.Invoke();
                    }
                }
                else
                {
                    if (_spring_value_01 > trigger_threshold)
                    {
                        _triggered = true;
                        Audio.AudioSystem.instance.PlayOneShot(se_spring_trigger);
                        on_spring_triggered?.Invoke();
                    }
                }

                on_spring_moved?.Invoke();
            }
        }

        public bool Triggered
        {
            get { return _triggered; }
        }

    }
}