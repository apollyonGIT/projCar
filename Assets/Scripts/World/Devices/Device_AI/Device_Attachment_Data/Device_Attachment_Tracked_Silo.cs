using System;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Tracked_Silo : Device_Attachment
    {
        #region Const
        private const float SILO_THRESHOLD_MIDDLE_TO_OPEN = 0.6F; // 弹仓打开的阈值
        private const float SILO_THRESHOLD_MIDDLE_TO_CLOSE = 0.4F; // 弹仓关闭合的阈值
        #endregion

        // -------------------------------------------------------------------------------

        public Action on_silo_fully_open_trigger;
        public Action on_silo_fully_installed_trigger;
        public Action on_silo_moved_on_track;

        // -------------------------------------------------------------------------------

        private enum Silo_State
        {
            installed, //silo_track.value == 0
            middle,
            open, //silo_track.value == 1
        }
        private Silo_State silo_state;
        private float _track_value;

        // ===============================================================================

        public float Track_Value
        {
            get { return _track_value; }

            set
            {
                _track_value = Mathf.Clamp01(value);
                switch (silo_state)
                {
                    case Silo_State.middle:
                        if (_track_value > SILO_THRESHOLD_MIDDLE_TO_OPEN)
                        {
                            on_silo_fully_open_trigger?.Invoke();
                            _track_value = 1f;
                            silo_state = Silo_State.open;
                            Audio.AudioSystem.instance.PlayOneShot(Commons.Config.current.SE_device_general_ka_da);
                        }
                        else if (_track_value < SILO_THRESHOLD_MIDDLE_TO_CLOSE)
                        {
                            on_silo_fully_installed_trigger?.Invoke();
                            _track_value = 0f;
                            silo_state = Silo_State.installed;
                            Audio.AudioSystem.instance.PlayOneShot(Commons.Config.current.SE_device_general_ka_da);
                        }
                        break;
                    case Silo_State.installed:
                        if (_track_value > SILO_THRESHOLD_MIDDLE_TO_CLOSE)
                            silo_state = Silo_State.middle;
                        break;
                    case Silo_State.open:
                        if (_track_value < SILO_THRESHOLD_MIDDLE_TO_OPEN)
                            silo_state = Silo_State.middle;
                        break;
                }
                on_silo_moved_on_track?.Invoke();
            }
        }

        public bool Silo_Fully_Open
        {
            get { return Track_Value == 1; }
        }

        public bool Silo_Fully_Installed
        {
            get { return Track_Value == 0; }
        }

    }
}