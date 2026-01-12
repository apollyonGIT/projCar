using UnityEngine;


namespace World.Devices.DeviceUiViews
{
    [RequireComponent(typeof(DeviceUiView))]
    public abstract class DeviceUIViewAddOn : MonoBehaviour
    {

        protected Device owner;

        // ===============================================================================

        public virtual void attach(Device owner)
        {
            attach_owner(owner);

            if (owner.cubicle_list != null)
                foreach (var cubicle in owner.cubicle_list)
                    attach_cubicle(cubicle);

            attach_highlightable();
        }

        /// <summary>
        /// 当owner类型改变时，需要继承并重新绑定owner类型。
        /// </summary>
        /// <param name="owner"></param>
        protected abstract void attach_owner(Device owner);
        protected abstract void attach_cubicle(Cubicles.BasicCubicle cubicle);
        protected abstract void attach_highlightable();

        // -------------------------------------------------------------------------------

        public virtual void detach()
        {

        }

        // -------------------------------------------------------------------------------

        public virtual void notify_on_tick()
        {
            update_cubicle_on_tick();
        }

        protected abstract void update_cubicle_on_tick();


        public virtual void trigger_event(params object[] args)
        {

        }   
    }
}
