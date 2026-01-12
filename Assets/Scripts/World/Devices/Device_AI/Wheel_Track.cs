using AutoCodes;
using UnityEngine;

namespace World.Devices.Device_AI {
    public class Wheel_Track : BasicWheel {
        #region Const
        private const string ANIM_NAME_IDLE = "idle";
        private const string ANIM_NAME_MOVING = "move";

        private const float ANIM_SPEED_FACTOR_WHILE_MOVING = 1.2F;
        #endregion

        private enum Device_FSM_Track {
            idle,
            move
        }
        private Device_FSM_Track fsm;

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
        }

        public override void tick()
        {
            base.tick();
            switch (fsm)
            {
                case Device_FSM_Track.idle:
                    if (WorldContext.instance.caravan_status_acc == WorldEnum.EN_caravan_status_acc.driving)
                    {
                        FSM_change_to(Device_FSM_Track.move);
                    }
                    break;
                case Device_FSM_Track.move:
                    if (WorldContext.instance.caravan_status_acc != WorldEnum.EN_caravan_status_acc.driving)
                    {
                        FSM_change_to(Device_FSM_Track.idle);
                    }
                    var cv = Mathf.Min(20f, WorldContext.instance.caravan_velocity.magnitude);
                    var anim_speed_factor = Mathf.LerpUnclamped(0f, ANIM_SPEED_FACTOR_WHILE_MOVING, cv);
                    Change_Anim_Play_Speed(anim_speed_factor);
                    break;
                default:
                    break;
            }
        }

        private void FSM_change_to(Device_FSM_Track target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Track.idle:
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_NAME_IDLE, true);
                    }
                    break;

                case Device_FSM_Track.move:
                    foreach (var view in views)
                    {
                        view.notify_change_anim(ANIM_NAME_MOVING, true);
                    }
                    break;

                default:
                    break;

            }
        }

        private void Change_Anim_Play_Speed(float f)
        {
            foreach (var view in views)
            {
                view.notify_change_anim_speed(f);
            }
        }

    }
}
        
