using AutoCodes;
using Commons;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class BasicWheel : Device
    {
        #region Const
        private const float ANGLE_LIMIT = 1919810F;
        private const float ANGLE_RESET = 114514F * Mathf.PI;
        #endregion

        private enum FSM_Wheel
        {
            Running,
            Braking,
            Jumping,
            Broken,
        }
        private FSM_Wheel fsm;

        private float angle_rotated;
        private float wheel_radius_reciprocal;
        private float wheel_jumping_visual_speed;

        private bool can_rotate = true;

        private float sprint_energy_recharge_speed;
        public float sprint_energy_recharge_speed_bonus;
        private int sprint_stored_times_max;

        // Public For UI etc.
        public int sprint_energy_recharge_cd;
        public float sprint_energy_01;
        public int sprint_stored_times;
        public float sprint_lever_loss;

        public device_wheel wheel_desc;

        //=================================================================================================================

        public override void InitData(device_all rc)
        {
            bones_direction.Clear();
            device_wheels.TryGetValue(rc.id.ToString() + ",0", out wheel_desc);
            wheel_radius_reciprocal = 1f / wheel_desc.wheel_radius_visual;

            sprint_energy_recharge_speed = wheel_desc.sprint_charge_speed;
            sprint_energy_recharge_speed_bonus = wheel_desc.sprint_charge_speed_bonus;
            sprint_stored_times_max = Mathf.Max(1, wheel_desc.sprint_charge_point + BattleContext.instance.wheel_charge_capacity);
            sprint_lever_loss = wheel_desc.sprint_lever_loss;

            bones_direction.Add(BONE_FOR_ROTATE, Vector2.right);

            base.InitData(rc);
        }

        public override void Start()
        {
            base.Start();
            FSM_change_to(FSM_Wheel.Braking);

            var ctx = WorldContext.instance;
            ctx.tractive_force_max = (int)(wheel_desc.tractive_force_max * (1 + BattleContext.instance.wheel_tractive_force / 1000f));
            ctx.feedback_0 = wheel_desc.feedback_0;
            ctx.feedback_1 = wheel_desc.feedback_1;
            ctx.bounce_loss = wheel_desc.bounce_loss;
            ctx.bounce_coef = wheel_desc.bounce_coef;
        }

        public override void UpdateData()
        {
            var ctx = WorldContext.instance;
            ctx.tractive_force_max = (int)(wheel_desc.tractive_force_max * (1 + BattleContext.instance.wheel_tractive_force / 1000f));
            ctx.feedback_0 = wheel_desc.feedback_0;
            ctx.feedback_1 = wheel_desc.feedback_1;
            base.UpdateData();
        }

        public override void tick()
        {
            var vel = (this as ITarget).velocity;
            var wctx = WorldContext.instance;

            switch (fsm)
            {
                case FSM_Wheel.Running:
                    if (faction == WorldEnum.Faction.player)
                    {
                        car_pushing_status_tick();
                        if (wctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.braking)
                            FSM_change_to(FSM_Wheel.Braking);
                        else if (wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.sky)
                            FSM_change_to(FSM_Wheel.Jumping);
                    }

                    wheel_rotate(vel.magnitude * Mathf.Sign(vel.x));

                    if (!is_validate)
                        FSM_change_to(FSM_Wheel.Broken);

                    break;

                case FSM_Wheel.Braking:
                    // 只有玩家可以进入这一状态，所以不需要检查faction
                    car_pushing_status_tick();
                    if (wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.sky)
                        FSM_change_to(FSM_Wheel.Jumping);
                    else if (wctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.driving)
                        FSM_change_to(FSM_Wheel.Running);

                    if (!is_validate)
                        FSM_change_to(FSM_Wheel.Broken);
                    break;

                case FSM_Wheel.Jumping:
                    // 只有玩家可以进入这一状态，所以不需要检查faction
                    car_pushing_status_tick();
                    if (wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.ground)
                        FSM_change_to(wctx.caravan_status_acc == WorldEnum.EN_caravan_status_acc.driving ? FSM_Wheel.Running : FSM_Wheel.Braking);

                    wheel_rotate(wheel_jumping_visual_speed);

                    if (!is_validate)
                        FSM_change_to(FSM_Wheel.Broken);
                    break;

                case FSM_Wheel.Broken:
                    if (is_validate)
                        FSM_change_to(FSM_Wheel.Running);

                    if (faction == WorldEnum.Faction.player && wctx.caravan_status_liftoff == WorldEnum.EN_caravan_status_liftoff.sky)
                        wheel_rotate(wheel_jumping_visual_speed);
                    else
                        wheel_rotate(vel.magnitude);

                    break;

                default:
                    break;
            }

            base.tick();
        }

        private void FSM_change_to(FSM_Wheel expected_fsm)
        {
            switch (expected_fsm)
            {
                case FSM_Wheel.Running:
                    wheel_jumping_visual_speed = 10f;
                    can_rotate = true;
                    break;
                case FSM_Wheel.Braking:
                    wheel_jumping_visual_speed = 0f;
                    can_rotate = false;
                    break;
                case FSM_Wheel.Jumping:
                    can_rotate = true;
                    break;
                case FSM_Wheel.Broken:
                    can_rotate = true;
                    break;
                default:
                    break;
            }
            fsm = expected_fsm;
        }

        // ----------------------------------------------------------------------------------------------

        private void wheel_rotate(float v)
        {
            if (!can_rotate)
                return;

            angle_rotated -= v * wheel_radius_reciprocal * Config.PHYSICS_TICK_DELTA_TIME;   //Caculate in Rad

            if (Mathf.Abs(angle_rotated) > ANGLE_LIMIT)
                angle_rotated -= Mathf.Sign(angle_rotated) * ANGLE_RESET;

            bones_direction[BONE_FOR_ROTATE] = new Vector2(Mathf.Cos(angle_rotated), Mathf.Sin(angle_rotated));
        }

        private void car_pushing_status_tick()
        {
            if (sprint_energy_recharge_cd > 0)
                sprint_energy_recharge_cd--;

            if (sprint_energy_recharge_cd > 0)
                return;

            sprint_energy_01 += sprint_energy_recharge_speed * (1 + BattleContext.instance.wheel_charge_recover / 1000f);

            if (sprint_energy_01 <= 1f)
                return;

            if (sprint_stored_times < sprint_stored_times_max)
            {
                sprint_stored_times++;
                sprint_energy_01--;
            }
            else
            {
                sprint_energy_01 = 1f;
            }
        }

    }
}
