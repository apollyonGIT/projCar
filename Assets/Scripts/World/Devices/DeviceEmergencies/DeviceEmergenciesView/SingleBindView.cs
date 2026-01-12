using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class SingleBindView : MonoBehaviour,IPointerClickHandler
    {
        public GameObject highlight;

        public DeviceBindView owner;
        public vine data;
        public void Init(Vector2 point1, Vector2 point2, vine v, DeviceBindView deviceBindView)
        {
            owner = deviceBindView;
            data = v;

            var len = (point1 - point2).magnitude;
            var dir = (point2 - point1).normalized;

            transform.localPosition = (point1 + point2) / 2 - new Vector2(transform.parent.GetComponent<RectTransform>().rect.width / 2, transform.parent.GetComponent<RectTransform>().rect.height / 2);
            transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
        }

        public void tick()
        {
            if(highlight!=null)
                highlight.SetActive(data.upper_vines.Count == 0);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            (owner.owner as DeviceBind).RemoveVine(data);
            Destroy(gameObject);
        }
    }
}
