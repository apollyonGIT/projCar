using System.Collections.Generic;
using UnityEngine;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Ram : DeviceUIViewAddOn
    {
        #region Const
        private const float ATTACK_TRIGGER_DISTANCE = 500;
        private const float ATTACK_TRIGGER_DECAY = 0.85F; // 衰减系数
        #endregion

        public GameObject ram_dir_indicator_current;
        public GameObject ram_dir_indicator_target;

        public DevicePanelAttachment_Ratchet_Crank widget_ratchet_for_dir;

        // -------------------------------------------------------------------------------

        protected bool cubicle_occupied_rotate_dir;

        new protected BasicRam_Click owner;

        // -------------------------------------------------------------------------------

        private List<DevicePanelAttachment_Highlightable> autoWorkHighlight_rotate = new();
        private DeviceRotateCubicle cubicle_rotate;

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            widget_ratchet_for_dir.Pre_Init_Ratchet_Data(this.owner.Ram_Ratchet_Rotation_Handle);
            widget_ratchet_for_dir.Init();

            ram_dir_indicator_current.transform.rotation = Quaternion.Euler(0, 0, this.owner.Ram_Dir_Indicator_Current.Dir_Angle);
            ram_dir_indicator_target.transform.rotation = Quaternion.Euler(0, 0, this.owner.Ram_Dir_Indicator_Target.Dir_Angle);
        }

        protected override void attach_owner(Device owner)
        {
            this.owner = owner as BasicRam_Click;
            this.owner.Ram_Ratchet_Rotation_Handle.on_rotate += widget_ratchet_for_dir.Synchronize_Dir;
        }

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            if (cubicle is DeviceRotateCubicle)
                cubicle_rotate = cubicle as DeviceRotateCubicle;
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_rotate.Add(widget_ratchet_for_dir);
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            base.detach();
            owner.Ram_Ratchet_Rotation_Handle.on_rotate -= widget_ratchet_for_dir.Synchronize_Dir;
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Per Tick Check: Trigger Owner Action by Widget
            if (widget_ratchet_for_dir.Rotate_To_Output_Power_Per_Tick(false, out var rotation_power))
                rotate_dir(rotation_power);


            // Update View by Owner
            var angle_current = owner.Ram_Dir_Indicator_Current.Dir_Angle;
            var angle_target = owner.Ram_Dir_Indicator_Target.Dir_Angle;
            ram_dir_indicator_current.transform.rotation = Quaternion.Euler(0, 0, angle_current);
            ram_dir_indicator_target.transform.rotation = Quaternion.Euler(0, 0, angle_target);


            // Set DevicePanelAttachment_Highlightable Status
            widget_ratchet_for_dir.Highlighted = Mathf.Abs(angle_current - angle_target) > 2f;
        }

        protected override void update_cubicle_on_tick()
        {
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_rotate, autoWorkHighlight_rotate);
        }

        // ===============================================================================

        protected void rotate_dir(float input)
        {
            owner.UI_Controlled_Ram_Rotate(input);
        }
    }
}
