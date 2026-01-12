using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static World.Devices.Device_AI.Device_Attachment_Ammo_Box;

namespace World.Devices.DeviceUiViews
{
    public class DevicePanelAttachment_Reload_Ammo_Box : DevicePanelAttachment, IPointerClickHandler, IDragHandler, IBeginDragHandler
    {
        #region Constant
        private const float DRAG_DISTANCE_MAX = 100F; // Maximum drag distance for the ammo box to open
        #endregion

        public Image IMG_boxMainBody;

        public Sprite unopened, open_left, empty;
        public List<Sprite> fully_opened;

        // -------------------------------------------------------------------------------

        private float mouse_pos_init_x;
        private bool reload_triggered;

        private AmmoBoxStage ammo_box_stage = AmmoBoxStage.Unopened;

        private Action reload_action;

        // ===============================================================================

        protected override void Init_Actions(List<Action> action)
        {
            reload_action += action[0];
        }

        // ===============================================================================

        public void Update_View(AmmoBoxStage ownerData_stage, int ownerData_reloadCountRemaining)
        {
            ammo_box_stage = ownerData_stage;
            switch (ownerData_stage)
            {
                case AmmoBoxStage.Unopened:
                    IMG_boxMainBody.sprite = unopened;
                    break;
                case AmmoBoxStage.Open_Left:
                    IMG_boxMainBody.sprite = open_left;
                    break;
                case AmmoBoxStage.Fully_Open:
                    IMG_boxMainBody.sprite = fully_opened[Mathf.Min(ownerData_reloadCountRemaining, fully_opened.Count) - 1];
                    break;
                case AmmoBoxStage.Empty:
                    IMG_boxMainBody.sprite = empty;
                    break;
                default:
                    break;
            }
        }

        private void reload_trigger()
        {
            reload_action?.Invoke();
            reload_triggered = true;
        }

        // ===============================================================================

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            switch (ammo_box_stage)
            {
                case AmmoBoxStage.Fully_Open:
                case AmmoBoxStage.Empty:
                    reload_trigger();
                    break;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            switch (ammo_box_stage)
            {
                case AmmoBoxStage.Unopened:
                case AmmoBoxStage.Open_Left:
                    mouse_pos_init_x = InputController.instance.GetScreenMousePosition().x;
                    break;
            }
            reload_triggered = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (reload_triggered)
                return;

            float mouse_pos_x;
            switch (ammo_box_stage)
            {
                case AmmoBoxStage.Unopened:
                    mouse_pos_x = InputController.instance.GetScreenMousePosition().x;
                    if (mouse_pos_x < mouse_pos_init_x - DRAG_DISTANCE_MAX)
                        reload_trigger();
                    break;
                case AmmoBoxStage.Open_Left:
                    mouse_pos_x = InputController.instance.GetScreenMousePosition().x;
                    if (mouse_pos_x > mouse_pos_init_x + DRAG_DISTANCE_MAX)
                        reload_trigger();
                    break;
            }
        }
    }
}