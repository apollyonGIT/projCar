using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 支持长按并连续触发效果的设备面板附件。
    /// 使用方法：挂载到Prefab中需要点击触发的组件上。
    /// </summary>
    public abstract class DevicePanelAttachment_Trigger : DevicePanelAttachment_Highlightable, IPointerUpHandler, IPointerDownHandler
    {

        protected bool _interactable = true; // 是否可交互   

        // ===============================================================================

        public virtual bool Interactable
        {
            get { return _interactable; }
            set
            {
                if (_interactable == value)
                    return; // 如果状态没有变化，则不做任何操作

                _interactable = value;
            }
        }

        // ===============================================================================

        public abstract void OnPointerDown(PointerEventData eventData);
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            Player_Interacting = false;
        }

    }
}
