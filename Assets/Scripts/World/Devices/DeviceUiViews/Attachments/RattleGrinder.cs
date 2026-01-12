using UnityEngine;
using UnityEngine.EventSystems;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class RattleGrinder : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private Rattle rattle;
        private Vector2 last_tick_pos;

        public void Init(Rattle r)
        {
            rattle = r;
        }


        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            last_tick_pos = InputController.instance.GetScreenMousePosition();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
            var dv = current_pos - last_tick_pos; //鼠标移动的距离


            rattle.AddEnergy(dv.magnitude * 0.1f); //假设每像素移动增加0.1点能量
            last_tick_pos = current_pos;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            
        }
    }
}
