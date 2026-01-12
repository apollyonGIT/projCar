using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Roll_Wood : DeviceUIViewAddOn_Shooter
    {

        private const float LEVER_SHOOTING_ROTATION_ANGLE = 90F; //拉杆旋转的角度

        public DevicePanelAttachment_Trigger_Press_Anim widget_saw_1,widget_saw_2;
        public DevicePanelAttachment_Trigger_Press widget_wood;
        public DevicePanelAttachment_Draggable_Spring widget_DraggableSpring_shooter;

        protected new Roll_Wood owner;

        public Image wood_1, wood_2, wood_3;
        public Transform standby_pos, ready_pos;

        public GameObject woods;

        public TextMeshProUGUI ammo_text;
        public RectTransform RECT_lever_for_shooting;

        public override void attach(Device owner)
        {
            base.attach(owner);

            widget_saw_1.Init(new() { ()=> { this.owner.CutWood();} });
            widget_saw_2.Init(new() { () => { this.owner.CutWood(); } });
            widget_wood.Init(new() { () => { this.owner.SetWood(); } });
            widget_DraggableSpring_shooter.Init();

            widget_saw_1.animator.enabled = false;
            widget_saw_2.animator.enabled = false;
        }

        public override void detach()
        {
            base.detach();
            widget_DraggableSpring_shooter.Detach_Owner_For_Sync_Value();
        }


        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(widget_saw_1);
            autoWorkHighlight_reload.Add(widget_saw_2);

            autoWorkHighlight_shoot.Add(widget_DraggableSpring_shooter);
        }

        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as Roll_Wood;
            widget_DraggableSpring_shooter.Attach_Owner_For_Sync_Value(this.owner.TriggerableSpring_Lever_For_Shooting);
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            owner.TriggerableSpring_Lever_For_Shooting.Spring_Value_01 = widget_DraggableSpring_shooter.Get_Relative_Drag_Distance_01();
            RECT_lever_for_shooting.localRotation = Quaternion.Euler(0, 0, LEVER_SHOOTING_ROTATION_ANGLE * (1 - owner.TriggerableSpring_Lever_For_Shooting.Spring_Value_01));

            set_wood();
            ammo_text.text = $"{owner.current_ammo} {owner.fire_logic_data.capacity}";
        }


        private void set_wood()
        {
            switch (owner.wood_state)
            {
                case Roll_Wood.Wood_State.standby:
                    woods.gameObject.transform.position = standby_pos.position;

                    wood_1.gameObject.SetActive(true);
                    wood_2.gameObject.SetActive(true);
                    wood_3.gameObject.SetActive(true);

                    widget_saw_1.gameObject.SetActive(false);
                    widget_saw_2.gameObject.SetActive(false);

                    widget_saw_1.animator.enabled = false;
                    widget_saw_2.animator.enabled = false;
                    break;
                case Roll_Wood.Wood_State.ready:
                    woods.gameObject.transform.position = ready_pos.position;

                    wood_1.gameObject.SetActive(true);
                    wood_2.gameObject.SetActive(true);
                    wood_3.gameObject.SetActive(true);

                    widget_saw_1.gameObject.SetActive(true);
                    widget_saw_2.gameObject.SetActive(false);

                    if(owner.saw_percent!=0)
                        widget_saw_1.SetAnim(owner.saw_percent);
                    break;
                case Roll_Wood.Wood_State.cut:
                    wood_1.gameObject.SetActive(false);
                    wood_2.gameObject.SetActive(true);
                    wood_3.gameObject.SetActive(true);

                    widget_saw_1.gameObject.SetActive(false);
                    widget_saw_2.gameObject.SetActive(true);
                    if (owner.saw_percent != 0)
                        widget_saw_2.SetAnim(owner.saw_percent);
                    break;
            }
        }
    }
}
