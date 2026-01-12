using UnityEngine;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Trebuchet : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const float LEVER_SHOOTING_ROTATION_ANGLE = 90F; //拉杆旋转的角度
        #endregion

        public RectTransform RECT_reloading_deco_dish;
        public RectTransform RECT_loaded_indicator;
        public RectTransform RECT_dir_indicator;
        public RectTransform RECT_lever_for_shooting;

        public GameObject dir_setter_highlight_1, dir_setter_highlight_2, dir_setter_highlight_3, dir_setter_highlight_4;

        public DevicePanelAttachment_Draggable_Spring widget_DraggableSpring_reloader;
        public DevicePanelAttachment_Trigger_Press_ReturnDir widget_TP_Dir_dir_setter;
        public DevicePanelAttachment_Draggable_Spring widget_DraggableSpring_shooter;

        // -------------------------------------------------------------------------------

        new protected Shooter_Trebuchet owner;

        // -------------------------------------------------------------------------------

        private readonly DeviceUI_Component_Blinker reloader_blinker = new();

        // ===============================================================================

        public override void attach(Device device_owner)
        {
            base.attach(device_owner);

            widget_DraggableSpring_reloader.Init();
            widget_TP_Dir_dir_setter.Init(new() { action_on_set_dir });
            widget_DraggableSpring_shooter.Init();

            RECT_reloading_deco_dish.localRotation = Quaternion.Euler(0, 0, owner.DirIndicator_Deco_Dish_Reloading.Dir_Angle);
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_Trebuchet;
            widget_DraggableSpring_reloader.Attach_Owner_For_Sync_Value(owner.TriggerableSpring_For_Reloading);
            widget_DraggableSpring_shooter.Attach_Owner_For_Sync_Value(owner.TriggerableSpring_Lever_For_Shooting);
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(widget_DraggableSpring_reloader);
            autoWorkHighlight_shoot.Add(widget_TP_Dir_dir_setter);
            autoWorkHighlight_shoot.Add(widget_DraggableSpring_shooter);
        }

        public override void detach()
        {
            base.detach();
            widget_DraggableSpring_reloader.Detach_Owner_For_Sync_Value();
            widget_DraggableSpring_shooter.Detach_Owner_For_Sync_Value();
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Sync Data to Owner
            owner.TriggerableSpring_For_Reloading.Spring_Value_01 = widget_DraggableSpring_reloader.Get_Relative_Drag_Distance_01();
            owner.TriggerableSpring_Lever_For_Shooting.Spring_Value_01 = widget_DraggableSpring_shooter.Get_Relative_Drag_Distance_01();


            // Update View by Owner
            RECT_loaded_indicator.localRotation = Quaternion.Euler(0, 0, owner.UI_Info_Indicator_Ammo_Loaded());
            RECT_reloading_deco_dish.localRotation = Quaternion.Euler(0, 0, owner.DirIndicator_Deco_Dish_Reloading.Dir_Angle);

            RECT_dir_indicator.localRotation = Quaternion.Euler(0, 0, owner.UI_Info_Indicator_Shoot_Angle());
            var highlight_general_condition = owner.Can_Set_Dir_Check_FSM() && !widget_TP_Dir_dir_setter.Auto;
            dir_setter_highlight_1.SetActive(highlight_general_condition && owner.UI_Info_Selected_Dir_Index() != 0);
            dir_setter_highlight_2.SetActive(highlight_general_condition && owner.UI_Info_Selected_Dir_Index() != 1);
            dir_setter_highlight_3.SetActive(highlight_general_condition && owner.UI_Info_Selected_Dir_Index() != 2);
            dir_setter_highlight_4.SetActive(highlight_general_condition && owner.UI_Info_Selected_Dir_Index() != 3);

            RECT_lever_for_shooting.localRotation = Quaternion.Euler(0, 0, LEVER_SHOOTING_ROTATION_ANGLE * (1 - owner.TriggerableSpring_Lever_For_Shooting.Spring_Value_01));


            // Set DevicePanelAttachment_Highlightable Status
            widget_DraggableSpring_reloader.Highlighted = owner.current_ammo <= 0;
            widget_DraggableSpring_shooter.Highlighted = owner.current_ammo > 0 && owner.UI_Info_Selected_Dir_Index() != -1;
        }

        // ===============================================================================

        private void action_on_set_dir()
        {
            owner.UI_Controlled_Set_Shoot_Dir(widget_TP_Dir_dir_setter.Get_Raw_Dir_Of_Click());
        }

    }
}
