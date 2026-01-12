using Foundations;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;
using World;
using World.BackPack;
using World.Devices;
using World.Devices.DeviceUiViews;
using World.Widgets;

public class InputController : MonoBehaviourSingleton<InputController>
{
    public Control_1 c1;
    private const int input_priority = 0;
    private const string input_name = "input";
    public event Action tick_action;

    public event Action right_click_event, right_hold_event, right_release_event;
    public event Action left_hold_event, left_release_event;
    public event Action esc_event;
    public event Action r_event;

    public Device control_device;
    public Texture2D aim_cursor;

    public GraphicRaycaster m_Raycaster;
    public EventSystem m_EventSystem;



    protected override void on_init()
    {
        base.on_init();
        {
            c1 = new Control_1();
        }
    }

    public void Del(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
        
        if(bmgr.select_slot!= null)
        {
            if (bmgr.select_slot.loot == null)
                return;
            else
            {
                bmgr.RemoveLoot(bmgr.select_slot.loot);
                bmgr.CancelSelectSlot();
            }
        }
    }

    public void SelectDevice_5(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.DeviceMgr_Name, out DeviceMgr dmgr);
        var device_slot = dmgr.slots_device.Find(sd => sd._name == "slot_front");
        var device = device_slot.slot_device;
        if (device != null)
        {
            dmgr.SelectDevice(device);
        }
    }

    public void SelectDevice_4(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.DeviceMgr_Name, out DeviceMgr dmgr);
        var device_slot = dmgr.slots_device.Find(sd => sd._name == "slot_back");
        var device = device_slot.slot_device;
        if (device != null)
        {
            dmgr.SelectDevice(device);
        }
    }

    public void SelectDevice_3(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.DeviceMgr_Name, out DeviceMgr dmgr);
        var device_slot = dmgr.slots_device.Find(sd => sd._name == "slot_front_top");
        var device = device_slot.slot_device;
        if (device != null)
        {
            dmgr.SelectDevice(device);
        }
    }

    public void SelectDevice_2(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.DeviceMgr_Name, out DeviceMgr dmgr);
        var device_slot = dmgr.slots_device.Find(sd => sd._name == "slot_top");
        var device = device_slot.slot_device;
        if (device != null)
        {
            dmgr.SelectDevice(device);
        }
    }

    public void SelectDevice_1(InputAction.CallbackContext context)
    {
        Mission.instance.try_get_mgr(Commons.Config.DeviceMgr_Name, out DeviceMgr dmgr);
        var device_slot = dmgr.slots_device.Find(sd => sd._name == "slot_back_top");
        var device = device_slot.slot_device;
        if (device != null)
        {
            dmgr.SelectDevice(device);
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        PointerEventData m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = Mouse.current.position.ReadValue();

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);

        if (results.Count > 0)
        {
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Performed:            //长按结束，开始执行逻辑
                if(context.interaction is HoldInteraction)
                {
                      LeftHold(context);
                }
                break;
            case InputActionPhase.Started:              //开始点按或者长按
                break;
            case InputActionPhase.Canceled:
                LeftRelease(context);
                break;
        }
    }


    public void SetDeviceControl(Device d)
    {
        if (control_device != null)
            EndDeviceControl();
        d.StartControl();
        control_device = d;

        Cursor.SetCursor(aim_cursor, Vector2.zero, CursorMode.Auto);
    }

    public void EndDeviceControl()
    {
        if (control_device == null)
            return;

        tick_action -= left_hold_event;
        tick_action -= right_hold_event;

        control_device.EndControl();
        control_device = null;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void EscClick(InputAction.CallbackContext context)
    {
        EndDeviceControl();
    }



    #region PullUp
    bool holding_pullup;

    private void pull_up()
    {
        Widget_DrivingLever_Context.instance.Drag_Lever(true, true, true);
    }
    public void PullUp(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:            //长按结束，开始执行逻辑
                if (context.interaction is HoldInteraction)
                {
                    holding_pullup = true;
                    Widget_Blower_Context.instance.notify_on_start_blower();
                }
                else
                {
                    pull_up();
                }
                break;
            case InputActionPhase.Started:              //开始点按或者长按
                break;
            case InputActionPhase.Canceled:
                if (holding_pullup)
                {
                    holding_pullup = false;
                    Widget_Blower_Context.instance.notify_on_end_blower();
                }
                break;
        }
    }
    #endregion

    #region PullDown
    bool holding_pulldown;

    private void pull_down()
    {
        Widget_DrivingLever_Context.instance.Drag_Lever(true, false, true);
    }

    public void PullDown(InputAction.CallbackContext context)
    {
        /*switch (context.phase)
        {
            case InputActionPhase.Performed:            //长按结束，开始执行逻辑
                if (context.interaction is HoldInteraction)
                {
                    holding_pulldown = true;
                    tick_action += pull_down;
                }
                else
                {
                    pull_down();
                }
                break;
            case InputActionPhase.Started:              //开始点按或者长按
                break;
            case InputActionPhase.Canceled:
                if (holding_pulldown)
                {
                    holding_pulldown = false;
                    tick_action -= pull_down;
                }
                break;
        }*/
    }
    #endregion

    public void Brake(InputAction.CallbackContext context)
    {
        Widget_DrivingLever_Context.instance.SetLever(0, true);
        WorldContext.instance.caravan_status_acc = WorldEnum.EN_caravan_status_acc.braking;
    }

    #region LeftButton
    [HideInInspector]
    public bool holding_left;

    private void LeftHold(InputAction.CallbackContext context)
    {
        holding_left = true;
        tick_action += left_hold_event;
    }

    private void LeftRelease(InputAction.CallbackContext context)
    {
        if (holding_left)
        {
            holding_left = false;
            tick_action -= left_hold_event;
        }
        left_release_event?.Invoke();
    }

    public void RemoveLeftHold(Action action)
    {
        if (holding_left)
        {
            holding_left = false;
            tick_action -= left_hold_event;
        }

        left_hold_event -= action;
    }
    #endregion

    public void RClick(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                r_event?.Invoke();
                Widget_Fix_Context.instance.PlayerOper(!Widget_Fix_Context.instance.player_oper);

                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Canceled:
                break;
        }
        
    }

    public void PClick(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (Widget_PushCar_Context.instance.AbleToPush_CheckCD)
                    if (Widget_PushCar_Context.instance.AbleToPush())
                        Widget_PushCar_Context.instance.PushCaravan();
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    public void AltPress(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                alt_press();
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Canceled:
                break;
        }

        void alt_press()
        {
            Debug.Log("AltPress");
            List<RaycastResult> results = new List<RaycastResult>();

            GraphicRaycaster graphicRaycaster = WorldSceneRoot.instance.uiRoot.GetComponent<GraphicRaycaster>();
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = GetScreenMousePosition()
            };
            graphicRaycaster.Raycast(pointerEventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent<IAltUi>(out IAltUi alt_ui))
                {
                    alt_ui.Begin();
                    return;
                }
            }
        }
    }

    public void AltRelease(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                alt_release();
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Canceled:
                break;
        }
        void alt_release()
        {
            Debug.Log("AltRelease");
            List<RaycastResult> results = new List<RaycastResult>();

            GraphicRaycaster graphicRaycaster = WorldSceneRoot.instance.uiRoot.GetComponent<GraphicRaycaster>();
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = GetScreenMousePosition()
            };
            graphicRaycaster.Raycast(pointerEventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent<IAltUi>(out IAltUi alt_ui))
                {
                    alt_ui.End();
                    return;
                }
            }
        }
    }

    public Vector2 GetScreenMousePosition()
    {
        return Mouse.current.position.ReadValue();
    }

    public Vector3 GetWorldMousePosition()
    {
        var mousePosition = GetScreenMousePosition();
        var pos = WorldSceneRoot.instance.mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 10 - WorldSceneRoot.instance.mainCamera.transform.position.z));

        return pos;
    }

    public void tick()
    {
        tick_action?.Invoke();
    }

    public void OnEnable()
    {
        c1.Enable();
        Ticker.instance.add_tick(input_priority, input_name, tick);
    }
    private void OnDisable()
    {
        c1.Disable();
        Ticker.instance.remove_tick(input_name);
    }
}
