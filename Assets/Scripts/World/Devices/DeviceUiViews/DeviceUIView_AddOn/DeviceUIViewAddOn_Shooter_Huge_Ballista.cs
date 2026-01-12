using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Cubicles;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.DevicePanel_Attachment;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Huge_Ballista : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const float LEVER_SHOOTING_ROTATION_ANGLE = 90F; // 弹射器的旋转角度
        #endregion

        public RectTransform lever_for_shooting;
        public RectTransform rotating_deco_dish_1, rotating_deco_dish_2;
        public RectTransform rotating_deco_reloading;
        public RectTransform muzzle_dir_indicator;

        public Slider spring_force_indicator; // 弹簧力指示器
        public Image colt; // 箭矢指示器；
        public GameObject img_colt_unreal;

        public DevicePanelAttachment_Ratchet_Crank widget_ratchet_for_dir;
        public DevicePanelAttachment_Draggable_Spring widget_spring_for_reloading;
        public DevicePanelAttachment_Bow_String widget_bow_string; // 弓弦
        public DevicePanelAttachment_Draggable_Spring widget_spring_lever_for_shooting;

        // -------------------------------------------------------------------------------

        new protected Shooter_Huge_Ballista owner;

        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_rotate = new();

        // -------------------------------------------------------------------------------

        private DeviceRotateCubicle cubicle_rotateDir;

        private readonly DeviceUI_Component_Blinker blinker_for_ammo_loader = new();

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            widget_ratchet_for_dir.Pre_Init_Ratchet_Data(this.owner.Ratchet_Muzzel_Rotation_Crank);
            widget_ratchet_for_dir.Init();
            widget_spring_for_reloading.Init();
            widget_spring_lever_for_shooting.Init();

            spring_force_indicator.value = this.owner.Spring_Force_Current;
            lever_for_shooting.localRotation = Quaternion.Euler(0, 0, LEVER_SHOOTING_ROTATION_ANGLE * (1 - this.owner.Triggerable_Spring_Lever_For_Shooting.Spring_Value_01));
            muzzle_dir_indicator.localRotation = Quaternion.Euler(0, 0, this.owner.Dir_Indicator_Muzzel.Dir_Angle);
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_Huge_Ballista;
            owner.Ratchet_Muzzel_Rotation_Crank.on_rotate += widget_ratchet_for_dir.Synchronize_Dir;
            widget_spring_lever_for_shooting.Attach_Owner_For_Sync_Value(owner.Triggerable_Spring_Lever_For_Shooting);
            widget_spring_for_reloading.Attach_Owner_For_Sync_Value(owner.Triggerable_Spring_For_Reloading);
        }

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            base.attach_cubicle(cubicle);
            if (cubicle is DeviceRotateCubicle)
                cubicle_rotateDir = cubicle as DeviceRotateCubicle;
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(widget_spring_for_reloading);
            autoWorkHighlight_shoot.Add(widget_spring_lever_for_shooting);
            autoWorkHighlight_rotate.Add(widget_ratchet_for_dir);
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            base.detach();
            owner.Ratchet_Muzzel_Rotation_Crank.on_rotate -= widget_ratchet_for_dir.Synchronize_Dir;
            widget_spring_for_reloading.Detach_Owner_For_Sync_Value();
            widget_spring_lever_for_shooting.Detach_Owner_For_Sync_Value();
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Per Tick Check: Trigger Owner Action by Widget
            if (widget_ratchet_for_dir.Rotate_To_Output_Power_Per_Tick(false, out var rotation_power))
                owner.UI_Controlled_Rotate(rotation_power);


            // Sync Data to Owner
            owner.Triggerable_Spring_Lever_For_Shooting.Spring_Value_01 = widget_spring_lever_for_shooting.Get_Relative_Drag_Distance_01();
            owner.Triggerable_Spring_For_Reloading.Spring_Value_01 = widget_spring_for_reloading.Get_Relative_Drag_Distance_01();


            // Update View by Owner
            rotating_deco_dish_1.localRotation = Quaternion.Euler(0, 0, owner.Dir_Indicator_Deco_Dish_1.Dir_Angle);
            rotating_deco_dish_2.localRotation = Quaternion.Euler(0, 0, owner.Dir_Indicator_Deco_Dish_2.Dir_Angle);
            muzzle_dir_indicator.localRotation = Quaternion.Euler(0, 0, owner.Dir_Indicator_Muzzel.Dir_Angle);

            rotating_deco_reloading.localRotation = Quaternion.Euler(0, 0, owner.Dir_Indicator_Deco_Dish_Reloading.Dir_Angle);
            spring_force_indicator.value = owner.Spring_Force_Current;
            widget_bow_string.Update_String_View(120f * owner.Spring_Force_Current);

            lever_for_shooting.localRotation = Quaternion.Euler(0, 0, LEVER_SHOOTING_ROTATION_ANGLE * (1 - owner.Triggerable_Spring_Lever_For_Shooting.Spring_Value_01));
            colt.gameObject.SetActive(owner.current_ammo == owner.fire_logic_data.capacity);
            img_colt_unreal.SetActive(owner.current_ammo < owner.fire_logic_data.capacity && owner.Spring_Force_Current > 0);


            // Set DevicePanelAttachment_Highlightable Status
            widget_ratchet_for_dir.Highlighted = owner.target_list.Count > 0;
            widget_spring_for_reloading.Highlighted = owner.Spring_Force_Current < 1;
            widget_spring_lever_for_shooting.Highlighted = owner.current_ammo > 0;
        }

        protected override void update_cubicle_on_tick()
        {
            base.update_cubicle_on_tick();
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_rotateDir, autoWorkHighlight_rotate);
        }

    }
}
