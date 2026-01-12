using Addrs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.DeviceUiViews;
using World.Devices.DeviceUiViews.CubiclesUiView;

namespace World.Devices
{
    public class DeviceOverview : MonoBehaviour, IPointerClickHandler
    {
        #region CONST
        private const int HP_BAR_SCALE_GRIDS_COUNT = 3;
        private const int HP_BAR_HP_PER_SCALE_GRID = 50;
        private const float HP_BAR_CONVERT_COEF = 1f / (HP_BAR_SCALE_GRIDS_COUNT * HP_BAR_HP_PER_SCALE_GRID);
        #endregion

        public Image index_image;

        public Image device_icon;
        public Image slot_image;

        public Slider hp_slider;
        public Slider true_hp_slider;
        public Image true_hp_img;

        public GameObject select;

        public DeviceHoveringInfo hoveringinfo;
        public GameObject operate_content;

        public DeviceMgr owner;

        public Device data;

        private DeviceUiView control_panel;
        private DeviceHealthState dhs;

        public Transform cub_content;
        public List<CubicleUiView> cubicles = new List<CubicleUiView>();

        public void SetOverview(string slot_name,Device device)
        {
            data = device;

            foreach(var cub in cubicles)
            {
                Destroy(cub.gameObject);
            }
            cubicles.Clear();


            if (device == null)
            {
                device_icon.gameObject.SetActive(false);
                slot_image.gameObject.SetActive(false);
                hp_slider.value = 0;
                true_hp_slider.value = 0;
            }
            else
            {
                init_cubicles();
                device_icon.gameObject.SetActive(true);
                slot_image.gameObject.SetActive(true);

                set_slot_icon(slot_name);
                hp_slider.value = device.current_hp;
                hp_slider.maxValue = device.battle_data.hp;
                true_hp_slider.value = device.current_hp;
                true_hp_slider.maxValue = device.battle_data.hp;

                if (hoveringinfo != null)
                {
                    hoveringinfo.data = device;
                }
            }
        }

        private void init_cubicles()
        {
            foreach(var cub in data.cubicle_list)
            {
               /* Addressable_Utility.try_load_asset<CubicleUiView>(cub.desc.cubicle_prefab, out var cubicle_ui_view);
                var cv = Instantiate(cubicle_ui_view, cub_content, false);
                cub.add_view(cv);
                cubicles.Add(cv);*/
            }
        }

        public void Init(string slot_name,Device device,DeviceMgr owner)
        {
            this.owner = owner;
            data = device;

            if(device == null)
            {
                device_icon.gameObject.SetActive(false);
                slot_image.gameObject.SetActive(false);
                hp_slider.value = 0;
                true_hp_slider.value = 0;
            }
            else
            {
                init_cubicles();   
                device_icon.gameObject.SetActive(true);
                slot_image.gameObject.SetActive(true);

                set_slot_icon(slot_name);
                hp_slider.value = device.current_hp;
                hp_slider.maxValue = device.battle_data.hp;
                true_hp_slider.value = device.current_hp;
                true_hp_slider.maxValue = device.battle_data.hp;
                true_hp_img.pixelsPerUnitMultiplier = device.battle_data.hp * HP_BAR_CONVERT_COEF;

                if (hoveringinfo != null)
                {
                    hoveringinfo.data = device;
                }
            }
        }

        public void SetIndexImage(int index)
        {
            Addressable_Utility.try_load_asset<Sprite>($"number_icons[ui_battle_workingboard_number_{index}]", out var sprite);
            index_image.sprite = sprite;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            owner.SelectDevice(data);   
        }

        private void set_slot_icon(string slot_name)
        {
            Sprite sprite = null;
            switch (slot_name)
            {
                case "slot_top":
                    Addressable_Utility.try_load_asset("slot_top_icon", out sprite);
                    break;
                case "slot_back_top":
                    Addressable_Utility.try_load_asset("slot_back_top_icon", out sprite);
                    break;
                case "slot_back":
                    Addressable_Utility.try_load_asset("slot_back_icon", out sprite);
                    break;
                case "slot_front_top":
                    Addressable_Utility.try_load_asset("slot_front_top_icon", out sprite);
                    break;
                case "slot_front":
                    Addressable_Utility.try_load_asset("slot_front_icon", out sprite);
                    break;
                default:
                    break;
            }

            if (sprite != null)
            {
                slot_image.sprite = sprite;
            }
        }

        public void tick()
        {
            if (data == null)
                return;

            hp_slider.value = data.current_hp;
            true_hp_slider.value = data.current_hp;

            if (data.current_hp < data.battle_data.hp && dhs != DeviceHealthState.Damaged && data.is_validate)
                dhs = DeviceHealthState.Damaged;
            else if (data.current_hp == data.battle_data.hp && dhs != DeviceHealthState.Full)
                dhs = DeviceHealthState.Full;
            else if (!data.is_validate && dhs != DeviceHealthState.Destroyed)
                dhs = DeviceHealthState.Destroyed;
        }

        public void Select()
        {
            select.gameObject.SetActive(true);

            if (data == null)
                return;
            Addressable_Utility.try_load_asset<DeviceUiView>(data.desc.ui_prefab, out var device_ui_view);
            control_panel = Instantiate(device_ui_view, operate_content.transform, false);

            data.add_view(control_panel);
        }

        public void UnSelect()
        {
            select.gameObject.SetActive(false);
            if(control_panel != null)
            {
                data.remove_view(control_panel);
            }
        }

    }
}
