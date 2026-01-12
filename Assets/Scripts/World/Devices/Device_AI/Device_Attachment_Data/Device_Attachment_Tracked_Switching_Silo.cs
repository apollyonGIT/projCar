using System;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class Device_Attachment_Tracked_Switching_Silo : Device_Attachment
    {
        #region Const
        private const float SILO_THRESHOLD_MIDDLE_TO_OPEN = 0.95F; // 弹仓切换轨道的阈值
        private const float SILO_THRESHOLD_MIDDLE_TO_CLOSE = 0.4F; // 弹仓关闭合的阈值
        #endregion

        // -------------------------------------------------------------------------------

        public Action on_silo_track_switched_trigger;
        public Action on_silo_fully_installed_trigger;
        public Action on_silo_moved_on_track;

        // -------------------------------------------------------------------------------

        private enum Silo_State
        {
            silo_installed, //silo_track.value == 0
            silo_middle,
        }
        private Silo_State silo_state;

        private bool _on_track_A = true;  // on_track_A == true 表示弹仓在轨道A上，false表示在轨道B上
        private float _track_value;

        // ===============================================================================

        public void Fully_Install()
        {
            _on_track_A = true;
            Track_Value = 0f;
        }

        public void Fully_Open()
        {
            _on_track_A = false;
            Track_Value = 0f;
        }

        // ===============================================================================

        public float Track_Value
        {
            get { return _track_value; }

            set
            {
                _track_value = Mathf.Clamp01(value);
                switch (silo_state)
                {
                    case Silo_State.silo_middle:
                        if (_track_value > SILO_THRESHOLD_MIDDLE_TO_OPEN)
                        {
                            _track_value = 0f;
                            _on_track_A = !_on_track_A; // 切换轨道
                            silo_state = Silo_State.silo_installed;
                            Audio.AudioSystem.instance.PlayOneShot(Commons.Config.current.SE_device_general_ka_da);
                            on_silo_track_switched_trigger?.Invoke();
                        }
                        else if (_track_value < SILO_THRESHOLD_MIDDLE_TO_CLOSE)
                        {
                            _track_value = 0f;
                            silo_state = Silo_State.silo_installed;
                            Audio.AudioSystem.instance.PlayOneShot(Commons.Config.current.SE_device_general_ka_da);
                            on_silo_fully_installed_trigger?.Invoke();
                        }
                        break;
                    case Silo_State.silo_installed:
                        if (_track_value > SILO_THRESHOLD_MIDDLE_TO_CLOSE)
                            silo_state = Silo_State.silo_middle;
                        break;
                }
                on_silo_moved_on_track?.Invoke();
            }
        }

        public bool On_Track_A
        {
            get { return _on_track_A; }
        }

        public bool Silo_Fully_Installed
        {
            get { return Track_Value == 0 && _on_track_A; }
        }

        public bool Silo_Fully_Open
        {
            get { return Track_Value == 0 && !_on_track_A; }
        }
    }
}