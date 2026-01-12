
using UnityEngine;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_DMelee_Blade : DeviceUIViewAddOn_DMelee
    {
        public float X; //测完转为const

        protected new DScript_Melee_Blade owner;

        public DevicePanelAttachment_Trigger_Hold hold_for_attacking;

        public DevicePanelAttachment_Animator handle_anim;

        public Transform up_pos, down_pos;

        protected Vector2 hold_position;

        protected bool up2down = true;

        protected bool can_drag = true;


        private bool special_set_zero;

        public override void attach(Device owner)
        {
            base.attach(owner);
            hold_for_attacking.Init(new() { record_hold_position,()=> { handle_anim.SetFloat("X01", 0); } });
            handle_anim.Init();
        }

        private void record_hold_position()
        {
            can_drag = true;
            hold_position = InputController.instance.GetScreenMousePosition();
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_melee.Add(hold_for_attacking);
        }

        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as DScript_Melee_Blade;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if(special_set_zero)
            {
                special_set_zero = false;
                handle_anim.SetFloat("X01", 0);
            }    

            if (hold_for_attacking.Is_Holding(true) && can_drag)
            {
                var mouse_position = InputController.instance.GetScreenMousePosition();
                
                var y_delta = up2down ? (hold_position.y - mouse_position.y) : (mouse_position.y - hold_position.y);

                handle_anim.SetFloat("X01", y_delta / X);

                if(y_delta > X)
                {
                    up2down = !up2down;

                    owner.Try_Attacking();

                    handle_anim.SetFloat("X01", 1);

                    special_set_zero = true;

                    can_drag = false;
                }
            }

            hold_for_attacking.Interactable = owner.CanAttack();
            hold_for_attacking.Highlighted = owner.CanAttack();

            handle_anim.tick();
        }
    }
}
