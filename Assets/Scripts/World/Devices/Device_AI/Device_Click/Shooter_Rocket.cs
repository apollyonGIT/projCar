using System.Collections.Generic;


namespace World.Devices.Device_AI
{
    public class Shooter_Rocket : DScript_Shooter
    {
        #region Const
        private const float MATCH_HEAT_MAX = 200F; // 火柴热量最大值
        private const float MATCH_HEAT_DECAY = 0.95F; // 火柴热量衰减系数
        private const int FIRE_COUNTDOWN_MAX = 120; // 火柴点燃后，火焰持续的帧数。倒计时结束后会发射。
        #endregion

        // -------------------------------------------------------------------------------

        public enum Rocket_State
        {
            ready,
            ignited,
            empty,
        }

        public Device_Attachment_Ammo_Box ammoBox = new Device_Attachment_Ammo_Box(3);

        // -------------------------------------------------------------------------------
        private class rocket
        {
            public Rocket_State rocket_state;
            public int fire_countdown;
        }

        private float _match_heat; // 火柴热量
        private bool _is_match_burning; // 火柴火焰是否处于激活状态
        private List<rocket> rockets = new List<rocket> { new rocket(), new rocket(), new rocket() };

        // ===============================================================================

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();
            if (!_is_match_burning)
            {
                if (_match_heat <= MATCH_HEAT_MAX)
                    _match_heat *= MATCH_HEAT_DECAY; // 每帧减少x%
                else
                    _is_match_burning = true;
            }

            foreach (var rocket in rockets)
                rocket_state_tick(rocket, fsm);
        }

        // ===============================================================================

        private void rocket_state_tick(rocket rocket, Device_FSM_Shooter rocket_fsm)
        {
            if (rocket_fsm == Device_FSM_Shooter.broken && rocket.rocket_state == Rocket_State.ignited)
            {
                rocket.rocket_state = Rocket_State.ready;
                return;
            }

            switch (rocket.rocket_state)
            {
                case Rocket_State.ready:
                    break;
                case Rocket_State.ignited:
                    if (--rocket.fire_countdown <= 0)
                        if (DeviceBehavior_Shooter_Try_Shoot(true, true))
                            rocket.rocket_state = Rocket_State.empty;
                    break;
                case Rocket_State.empty:
                    break;
                default:
                    break;
            }
        }

        private bool check_if_rocket_can_reload_and_get_index(out int index)
        {
            for (index = 0; index < rockets.Count; index++)
            {
                if (rockets[index].rocket_state == Rocket_State.empty)
                    return true;
            }
            return false;
        }

        // ===============================================================================

        protected override bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading)
        {
            if (ammoBox.Use_Ammo_Box(check_if_rocket_can_reload_and_get_index(out var index)))
            {
                rockets[index].rocket_state = Rocket_State.ready;
                current_ammo++;
                return true;
            }
            return false;
        }

        // ===============================================================================

        public float UI_Info_Match_Heat
        {
            get { return _match_heat; }
            set { _match_heat = value; }
        }

        public bool UI_Info_Is_Match_Burning
        {
            get { return _is_match_burning; }
            set { _is_match_burning = value; }
        }

        public Rocket_State UI_Info_Get_Rocket_State(int index)
        {
            return rockets[index].rocket_state;
        }

        public void UI_Controlled_Ignite(int rocket_index)
        {
            if (rockets[rocket_index].rocket_state == Rocket_State.ready)
            {
                rockets[rocket_index].rocket_state = Rocket_State.ignited;
                rockets[rocket_index].fire_countdown = FIRE_COUNTDOWN_MAX;
            }
        }
    }

}