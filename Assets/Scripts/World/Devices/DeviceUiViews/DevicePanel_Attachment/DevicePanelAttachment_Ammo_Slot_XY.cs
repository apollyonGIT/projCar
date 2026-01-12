using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews 
{ 
    public class DevicePanelAttachment_Ammo_Slot_XY : DevicePanelAttachment
    {
        public List<Image> ammo;

        public Sprite[] ammo_sprite;

        // -------------------------------------------------------------------------------

        private int ammo_num_max;
        private int ammo_num_last_check;

        // ===============================================================================

        public override void Init(List<Action> action = null)
        {
            ammo_num_max = ammo.Count;
            ammo_num_last_check = ammo_num_max;
        }

        protected override void Init_Actions(List<Action> action)
        {

        }

        // ===============================================================================

        public void Update_Ammo_Slot_View(int current_ammo)
        {
            if (current_ammo == ammo_num_last_check)
                return;

            for (int i = 0; i < ammo_num_max; i++)
                ammo[i].gameObject.SetActive(i < current_ammo);
            ammo_num_last_check = current_ammo;
        }

        public void Set_Index_Ammo_Slot_View(int index, int sprite_index)
        {
            if(sprite_index <0 || sprite_index >= ammo_sprite.Length)
            {
                ammo[index].gameObject.SetActive(false);
                return;
            }
                
            ammo[index].gameObject.SetActive(true);
            ammo[index].sprite = ammo_sprite[sprite_index];
        }

        public void Set_Ammo_Sprite(Sprite s)
        {
            foreach (var img in ammo)
                img.sprite = s;
        }

        // -------------------------------------------------------------------------------

        public Vector2 Get_Ammo_Slot_Anchored_Position(int i)
        {
            if (i >= ammo_num_max)
                return Vector2.zero;
            return ammo[i].rectTransform.anchoredPosition;
        }

        public Transform Get_Ammo_Slot_Transform(int i)
        {
            if (i >= ammo_num_max)
                return null;
            return ammo[i].rectTransform.transform;
        }
    }
}
