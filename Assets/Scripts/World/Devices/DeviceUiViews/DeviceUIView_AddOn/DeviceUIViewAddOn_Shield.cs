using UnityEngine;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shield : DeviceUIViewAddOn
    {
        #region Const
        private const float ATTACK_TRIGGER_DISTANCE = 500;
        private const float ATTACK_TRIGGER_DECAY = 0.85F; // 衰减系数
        #endregion

        public DevicePanelAttachment_Trigger_Press widget_def_start;
        public DevicePanelAttachment_Trigger_Hold widget_reset_block_cd;
        public DevicePanelAttachment_Ratchet_Crank widget_ratchet_for_dir;

        // -------------------------------------------------------------------------------

        protected bool cubicle_occupied_rotate_dir;

        new protected BasicShield_Click owner;

        // ===============================================================================

        public override void attach(Device device_owner)
        {
            base.attach(device_owner);
            widget_def_start.Init(new() { start_def });
            widget_reset_block_cd.Init();
            widget_ratchet_for_dir.Pre_Init_Ratchet_Data(owner.Ratchet_Shield_Rotation_Crank);
            widget_ratchet_for_dir.Init();
        }

        protected override void attach_owner(Device device_owner)
        {
            owner = device_owner as BasicShield_Click;
            owner.Ratchet_Shield_Rotation_Crank.on_rotate += widget_ratchet_for_dir.Synchronize_Dir;
        }

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            // we dont know the cubicle yet 
        }

        protected override void attach_highlightable()
        {
            // we dont know the cubicle yet 
        }

        public override void detach()
        {
            base.detach();
            owner.Ratchet_Shield_Rotation_Crank.on_rotate -= widget_ratchet_for_dir.Synchronize_Dir;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if (widget_ratchet_for_dir.Rotate_To_Output_Power_Per_Tick(false, out var rotation_power))
                accelerate_rotatino(rotation_power);

            if (widget_reset_block_cd.Is_Holding(false))
                reset_block_cd();
        }

        protected override void update_cubicle_on_tick()
        {
            // we dont know the cubicle yet 
        }

        // ===============================================================================

        protected void start_def()
        {
            owner.UI_Controlled_Start_Def();
        }

        protected void reset_block_cd()
        {
            owner.UI_Controlled_Reset_Block_CD();
        }

        protected void accelerate_rotatino(float input)
        {
            owner.UI_Controlled_Accelerate_Rotation(input);
        }
    }
}
