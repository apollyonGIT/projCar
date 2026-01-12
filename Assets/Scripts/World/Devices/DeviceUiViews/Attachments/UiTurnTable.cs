using UnityEngine;
using UnityEngine.EventSystems;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class UiTurnTable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Transform center;

        private bool turning;
        private Vector2 last_tick_pos;

        private Device device;

        private bool play_se;  //BasicRam播放音效的判定

        public bool table_direction_offset;


        public void init(Device uiview_owner)
        {
            device = uiview_owner;
        }

        public void tick()
        {
            if(device is IRotate turn)
            {
                if (turning)
                {
                    var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
                    Vector2 center_pos = RectTransformUtility.WorldToScreenPoint(WorldSceneRoot.instance.uiCamera, center.transform.position);

                    var angle = Vector2.SignedAngle(last_tick_pos - center_pos, current_pos - center_pos);

                    turn.Rotate(angle);

                    if (table_direction_offset)
                    {
                        
                        var offset_angle = Vector2.SignedAngle(Vector2.right, last_tick_pos - center_pos);
                        transform.eulerAngles = new Vector3(0, 0, offset_angle);
                    }
                    else
                    {
                        transform.eulerAngles = new Vector3(0, 0, turn.turn_angle);
                    }
                }

                last_tick_pos = InputController.instance.GetScreenMousePosition();
            }

            /*switch (device)
            {
                case NewBasicHook owner_hook:
                    if (turning)
                    {
                        var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
                        Vector2 center_pos = RectTransformUtility.WorldToScreenPoint(WorldSceneRoot.instance.uiCamera, center.transform.position);

                        var angle = Vector2.SignedAngle(last_tick_pos - center_pos, current_pos - center_pos);
                        owner_hook.Rotate_Reel_To_Tighten_Spring(angle);
                    }
                    transform.eulerAngles = new Vector3(0, 0, owner_hook.rotate_angle);
                    last_tick_pos = InputController.instance.GetScreenMousePosition();
                    break;

                case BasicRam owner_ram:
                    if (turning)
                    {
                        var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
                        Vector2 center_pos = RectTransformUtility.WorldToScreenPoint(WorldSceneRoot.instance.uiCamera, center.transform.position);

                        var angle = Vector2.SignedAngle(last_tick_pos - center_pos, current_pos - center_pos);

                        var a = Vector2.SignedAngle(Vector2.right, last_tick_pos - center_pos);
                        transform.eulerAngles = new Vector3(0, 0, a);

                        if (angle == 0 == play_se)
                        {
                            play_se = !play_se;
                            owner_ram.Play_Or_End_SE_By_UI(play_se);
                        }
                    }
                    last_tick_pos = InputController.instance.GetScreenMousePosition();
                    break;
                case ComplexShooter owner_shooter:
                    if (turning)
                    {
                        var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
                        Vector2 center_pos = RectTransformUtility.WorldToScreenPoint(WorldSceneRoot.instance.uiCamera, center.transform.position);
                        var angle = Vector2.SignedAngle(last_tick_pos - center_pos, current_pos - center_pos);
                        owner_shooter.Rotate_Ram_Dir_By_UI(angle);

                        var a = Vector2.SignedAngle(Vector2.right, last_tick_pos - center_pos);
                        transform.eulerAngles = new Vector3(0, 0, a);
                    }
                    last_tick_pos = InputController.instance.GetScreenMousePosition();
                    break;
                default:
                    break;
            }*/
        }


        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            InputController.instance.tick_action -= tick;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            InputController.instance.tick_action += tick;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            turning = true;
            play_se = false;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            turning = false;
            if (device is BasicRam owner_ram)
                owner_ram.Play_Or_End_SE_By_UI(false);
        }
    }
}
