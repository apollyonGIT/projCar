using System.Collections.Generic;
using UnityEngine.UI;
using World.Devices.Device_AI;
using static World.Devices.Device_AI.Shooter_Rocket;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Rocket : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const int ROCKET_NUM = 3;
        #endregion

        public DevicePanelAttachment_Reload_Ammo_Box reload_ammo_box;
        public DevicePanelAttachment_Reload_Ammo_Box ammo_box_view_for_auto_indicator;
        public DevicePanelAttachment_Grinder grinder;

        public Image match_flame; // 火柴火焰图标

        public List<Image> rocket_body_list;
        public List<Image> rocket_flame_list;

        // -------------------------------------------------------------------------------

        new protected Shooter_Rocket owner;

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            reload_ammo_box.Init(new() { reload });
            grinder.Init(new() { get_match_friction, end_hover_match });
        }

        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as Shooter_Rocket;
        }

        protected override void attach_highlightable()
        {
            // to do
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            var ownerAmmoBoxStage = owner.ammoBox.GetAmmoBoxStage(out var reloadCountRemaining);
            reload_ammo_box.Update_View(ownerAmmoBoxStage, reloadCountRemaining);
            if (ammo_box_view_for_auto_indicator.gameObject.activeSelf)
                ammo_box_view_for_auto_indicator.Update_View(ownerAmmoBoxStage, reloadCountRemaining);

            grinder.Fetch_Top_Object_From_Base(owner.UI_Info_Is_Match_Burning);
            match_flame.gameObject.SetActive(owner.UI_Info_Is_Match_Burning);

            for (int i = 0; i < ROCKET_NUM; i++)
            {
                rocket_body_list[i].enabled = owner.UI_Info_Get_Rocket_State(i) != Rocket_State.empty;
                rocket_flame_list[i].gameObject.SetActive(owner.UI_Info_Get_Rocket_State(i) == Rocket_State.ignited);
            }
        }

        // ===============================================================================

        private void get_match_friction()
        {
            owner.UI_Info_Match_Heat += grinder.Get_Accumulated_Grind(false);
        }

        private void end_hover_match()
        {
            owner.UI_Info_Match_Heat = 0;
            owner.UI_Info_Is_Match_Burning = false;
            match_flame.gameObject.SetActive(false);
        }

        // ===============================================================================

        public void UI_Event_Ignite(int rocket_index)
        {
            // On Pointer Enter
            if (owner.UI_Info_Is_Match_Burning)
                owner.UI_Controlled_Ignite(rocket_index);
        }
    }
}
