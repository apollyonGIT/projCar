using TMPro;
using UnityEngine;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.DevicePanel_Attachment;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Bow : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const float PULL_STRING_DISTANCE_MAX = 320F; //拉弓的最大距离
        #endregion

        public TextMeshProUGUI ammoText;
        public GameObject LoadedArrow;
        public RectTransform Arrow_Location;

        public DevicePanelAttachment_Ammo_Slot widgetAmmoSlot_arrow_quiver;
        public DevicePanelAttachment_Trigger_Hold widgetTriggerPress_quiver_reload;
        public DevicePanelAttachment_Trigger_Press widgetTriggerPress_load_arrow_into_barrel;
        public DevicePanelAttachment_Draggable_Spring widgetSpring_bow_spring;
        public DevicePanelAttachment_Bow_String widgetBowString_bow_string;
        public DevicePanelAttachment_Ammo_Slot widgetAmmoSlot_stored_arrow;
        public DevicePanelAttachment_Process_Bar widget_reloading_process_bar;
        public DevicePanelAttachment_Process_Bar widget_holding_for_starting_reloading_pb;

        // -------------------------------------------------------------------------------

        protected new Shooter_Bow owner;

        // -------------------------------------------------------------------------------

        private readonly DeviceUI_Component_Blinker reloader_blinker = new();

        // ===============================================================================

        public override void attach(Device device_owner)
        {
            base.attach(device_owner);
            widgetAmmoSlot_arrow_quiver.Init();
            widgetAmmoSlot_stored_arrow.Init();
            widgetTriggerPress_load_arrow_into_barrel.Init(new() { load_bullet_into_barrel });
            widgetSpring_bow_spring.Init(new() { owner.Shoot_On_Release_Bow_String });
            widgetBowString_bow_string.Init();

            ammoText.text = $"{owner.current_ammo} {owner.fire_logic_data.capacity}";
            widgetAmmoSlot_arrow_quiver.Update_Ammo_Slot_View(owner.current_ammo);
            widgetAmmoSlot_stored_arrow.Update_Ammo_Slot_View(owner.Stored_Arrow);
            LoadedArrow.SetActive(owner.Get_Barrel() == 2);
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_Bow;
            widgetSpring_bow_spring.Attach_Owner_For_Sync_Value(owner.TriggerableSpring_for_Shooting);
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(widgetTriggerPress_quiver_reload);
            autoWorkHighlight_reload.Add(widgetTriggerPress_load_arrow_into_barrel);
            autoWorkHighlight_shoot.Add(widgetSpring_bow_spring);
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            base.detach();
            widgetSpring_bow_spring.Detach_Owner_For_Sync_Value();
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Sync Data to Owner
            owner.TriggerableSpring_for_Shooting.Spring_Value_01 = widgetSpring_bow_spring.Get_Relative_Drag_Distance_01();
            if (widgetTriggerPress_quiver_reload.Is_Holding(false))
                owner.UI_Hold_And_Set_Reloading_Trigger();


            // Update View by Owner
            var spring_value01 = owner.TriggerableSpring_for_Shooting.Spring_Value_01;
            var string_color = Color.white;
            if (spring_value01 > 0.75)
            {
                widgetSpring_bow_spring.Forced_Drop_Drag();
                widgetSpring_bow_spring.Forced_Set_Relative_Drag_Distance_01(0f);
            }
            else if (spring_value01 > 0.15f)
            {
                var t = Mathf.InverseLerp(0.45f, 0.75f, spring_value01);
                var gb = Mathf.Lerp(1f, 0f, t);
                string_color = new Color(1f, gb, gb);
            }

            var pre_reloading = !owner.Is_Reloading || owner.Reloading_Prereoading_Ticks_T <= 0;
            var y = pre_reloading ? 0 : Mathf.Lerp(150f, 0f, Mathf.Pow(owner.Reloading_Prereoading_Ticks_T, 2));
            Arrow_Location.localPosition = new Vector2(0, y);

            widgetBowString_bow_string.Update_String_View(spring_value01 * PULL_STRING_DISTANCE_MAX, string_color);
            if (LoadedArrow.activeSelf)
            {
                if (owner.Just_Shoot)
                    LoadedArrow.SetActive(false);
            }
            else
            {
                if (owner.Get_Barrel() != 2)
                    owner.Just_Shoot = false;
                else if (!owner.Just_Shoot)
                    LoadedArrow.SetActive(true);
            }
            ammoText.text = $"{owner.current_ammo} {owner.fire_logic_data.capacity}";
            if (pre_reloading)
                widgetAmmoSlot_arrow_quiver.Update_Ammo_Slot_View(owner.current_ammo);
            widgetAmmoSlot_stored_arrow.Update_Ammo_Slot_View(owner.Stored_Arrow);

            widget_reloading_process_bar.Update_View(owner.current_ammo/ (float)owner.fire_logic_data.capacity, owner.Is_Reloading);

            var p = owner.HoldingTimer_reloader.Get_Timer_Range_01();
            widget_holding_for_starting_reloading_pb.Update_View(p, p > 0.01f);


            // Set DevicePanelAttachment_Highlightable Status 1/2
            widgetSpring_bow_spring.Highlighted = owner.TriggerableSpring_for_Shooting.Spring_Value_01 <= 0 && Barrel_Bullet_Loaded;
            if (widgetTriggerPress_quiver_reload.Highlighted)
            {
                if (owner.current_ammo >= owner.fire_logic_data.capacity || owner.Is_Reloading)
                    widgetTriggerPress_quiver_reload.Highlighted = false;
            }
            else
            {
                if (owner.current_ammo <= 0 && !owner.Is_Reloading)
                    widgetTriggerPress_quiver_reload.Highlighted = true;
            }
            var hl = owner.Stored_Arrow < 3 && !owner.Is_Reloading;
            widgetTriggerPress_load_arrow_into_barrel.Highlighted = hl;


            // Update View by DevicePanelAttachment_Highlightable Status 2/2
            if (widgetTriggerPress_load_arrow_into_barrel.Auto)
                widgetAmmoSlot_arrow_quiver.Set_Ammo_Sprite(widgetTriggerPress_load_arrow_into_barrel.sprite_auto);
            else if (hl)
                widgetAmmoSlot_arrow_quiver.Set_Ammo_Sprite(widgetTriggerPress_load_arrow_into_barrel.sprite_highlighted);
            else
                widgetAmmoSlot_arrow_quiver.Set_Ammo_Sprite(widgetTriggerPress_load_arrow_into_barrel.sprite_normal);
        }

    }
}
