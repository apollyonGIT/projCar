using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Ui
{
    public class PointerHovering : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        public GameObject name_obj;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            name_obj.gameObject.SetActive(true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            name_obj.gameObject.SetActive(false);
        }
    }
}
