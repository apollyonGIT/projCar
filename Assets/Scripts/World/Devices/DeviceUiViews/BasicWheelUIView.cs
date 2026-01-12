using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class BasicWheelUiView : DeviceUiView
    {
        public Button Push_Car_btn;
        public Slider Push_Car_Energy_Indicator;
        public TextMeshProUGUI Push_Car_Stored_Times;

        private new BasicWheel owner;

        public Slider speed_slider;
        public Slider lever_slider;

        public override void init()
        {
            base.init();
        }

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as BasicWheel;
        }

        public override void notify_on_tick()
        {
            var wctx = WorldContext.instance;
            base.notify_on_tick();

            var car_speed_kmh = wctx.caravan_velocity.magnitude * 7.2f;   // 7.2F = 3.6F * 2F, where 3.6F is the conversion factor from m/s to km/h, and 2F is the factor to convert from the speed in unit/s to the speed in m/s.

            lever_slider.value = WorldContext.instance.driving_lever;
            speed_slider.value = (Mathf.Log(car_speed_kmh + 4, 2) - 2f) * 0.2f;  //车速刻度取对数显示

            if(Push_Car_btn!=null)
                Push_Car_btn.interactable = WorldContext.instance.driving_lever >= owner.sprint_lever_loss;
        }

        public override void on_click()
        {
        }
    }
}
