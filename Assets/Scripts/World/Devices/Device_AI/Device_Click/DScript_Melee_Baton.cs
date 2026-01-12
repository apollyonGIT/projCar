namespace World.Devices.Device_AI
{
    public class DScript_Melee_Baton : DScript_Melee
    {
        public float self_repair_process = 0;

        public float self_repair_speed = 1/360f;

        public int current_durability = 15;

        public int max_durability = 15;


        protected override void idle_state_tick()
        {
            base.idle_state_tick();
            if (current_durability <= 0)
            {
                FSM_change_to(Device_FSM_Melee.repair);
            }
        }

        protected override void repair_state_tick()
        {
            self_repair_process += self_repair_speed;

            if (self_repair_process >= 1)
            {
                self_repair_process = 0;
                current_durability = max_durability;

                FSM_change_to(Device_FSM_Melee.idle);
            }
        }

        protected override void open_collider(Device d)
        {
            base.open_collider(d);

            current_durability--;
        }

        private bool can_attack_check_durability()
        {
            return current_durability > 0;
        }

        protected override bool can_manual_attack()
        {
            return base.can_manual_attack() && can_attack_check_durability();
        }

    }
}
