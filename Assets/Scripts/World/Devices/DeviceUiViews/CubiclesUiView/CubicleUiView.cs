using Addrs;
using Foundations.MVVM;
using UnityEngine;
using UnityEngine.UI;
using World.Characters;
using World.Cubicles;

namespace World.Devices.DeviceUiViews.CubiclesUiView
{
    public class CubicleUiView : MonoBehaviour, ICubicleView
    {
        public Image icon_image;

        private Sprite icon_idle, icon_working;

        public DeviceStationView station_view;

        public BasicCubicle owner;

        public GameObject highlight;

        void IModelView<BasicCubicle>.attach(BasicCubicle owner)
        {
            this.owner = owner;
            station_view.InitCub(owner);

            var c = owner.worker;
            if (c == null)
            {
                station_view.SetCharacter(null);
            }
            else
            {
                //Addressable_Utility.try_load_asset(c.desc.portrait_small, out Sprite s);
                station_view.SetCharacter(c);
            }

            Addressable_Utility.try_load_asset(owner.desc.icon_idle, out icon_idle);
            Addressable_Utility.try_load_asset(owner.desc.icon_working, out icon_working);

            if (icon_image != null)
                icon_image.sprite = c == null ? icon_idle : icon_working;
        }

        void IModelView<BasicCubicle>.detach(BasicCubicle owner)
        {
            if (this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void ICubicleView.notify_tick()
        {
            tick();

            station_view.tick();
        }

        protected virtual void tick()
        {

        }

        public virtual void notify_set_worker(Character c)
        {
            if (icon_image != null)
                icon_image.sprite = c == null ? icon_idle : icon_working;

            if (c == null)
            {
                station_view.SetCharacter(null);
            }
            else
            {
                //Addressable_Utility.try_load_asset(c.desc.portrait_small, out Sprite s);
                station_view.SetCharacter(c);
            }

        }

        void ICubicleView.notify_highlight(bool ret)
        {
            if (highlight != null)
                highlight.SetActive(ret);
        }
    }
}
