using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tests
{
    public class HoldingEvent : MonoBehaviour
    {

        [SerializeField]
        public UnityEvent holding_event;


        public Slider holding_slider;

        public float Ping = 1;
        private bool IsStart = false;
        private float LastTime = 0;
        void Update()
        {
            if(holding_slider!=null)
                holding_slider.value =( Time.realtimeSinceStartup - LastTime )/ Ping;

            if (IsStart && Ping > 0 && LastTime > 0 && Time.realtimeSinceStartup - LastTime > Ping)
            {
                Debug.Log("长按触发");
                IsStart = false;
                LastTime = 0;

                holding_event?.Invoke();
            }
        }
        public void LongPress(bool bStart)
        {
            IsStart = bStart;
            if (IsStart)
            {
                LastTime = Time.realtimeSinceStartup;
                Debug.Log("长按开始");
            }
            else if (LastTime != 0)
            {
                LastTime = 0;
                Debug.Log("长按取消");
            }
        }
    }
}
