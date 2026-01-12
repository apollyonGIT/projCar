using Addrs;
using Commons;
using Foundations.MVVM;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Caravans;
using World.Devices.DeviceEmergencies;
using World.Devices.DeviceEmergencies.DeviceEmergenciesView;
using World.Devices.DeviceUiViews.CubiclesUiView;
using World.Helpers;
using World.Ui;

namespace World.Devices.DeviceUiViews
{
    public enum DeviceHealthState
    {
        Full,
        Damaged,
        Destroyed,
    }

    public class DeviceUiView : MonoBehaviour, IDeviceView, IUiFix, IUiView, IPointerClickHandler
    {
        public Device owner;


        public Image danger_icon;
        public List<CubicleUiView> stations = new();
        public List<DeviceUIViewAddOn> view_add_ons = new();

        private DeviceEmergencyView dev;


        Vector2 IUiView.pos => transform.position;

        public virtual void attach(Device owner)
        {
            this.owner = owner;


            for(int i = 0;i<stations.Count; i++)
            {
                if(owner.cubicle_list.Count > i)
                {
                    owner.cubicle_list[i].add_view(stations[i]);
                }
            }

            Ui_Pos_Helper.ui_views.Add(this);

            if (view_add_ons != null)
                foreach (var add_on in view_add_ons)
                    add_on.attach(owner);
            // mask.gameObject.SetActive(!owner.is_validate);

            if (owner.state != Device.DeviceState.normal)
            {
                show_emergency(owner.device_emergency);
            }
        }

        void IModelView<Device>.detach(Device owner)
        {
            if (this.owner != null)
                this.owner = null;
            for (int i = 0; i < stations.Count; i++)
            {
                if (owner.cubicle_list.Count > i)
                {
                    owner.cubicle_list[i].remove_view(stations[i]);
                }
            }

            if (view_add_ons != null)
                foreach (var add_on in view_add_ons)
                    add_on.detach();

            dev?.owner.remove_view(dev);
            dev = null;

            Destroy(gameObject);
        }

        public virtual void init()
        {

        }

        public virtual void init_pos()
        {

        }


        void IDeviceView.notify_change_anim(string anim_name, bool loop)
        {

        }

        void IDeviceView.notify_change_anim_speed(float f)
        {

        }

        void IDeviceView.notify_close_collider(string _name)
        {

        }

        void IDeviceView.notify_hurt(int dmg)
        {

        }


        public virtual void notify_on_tick()
        {

            var b = owner.target_list.Count == 0 && owner.outrange_targets.Count == 0;

            if (view_add_ons != null)
                foreach (var add_on in view_add_ons)
                {
                    add_on.notify_on_tick();
                }
        }

        void IDeviceView.notify_open_collider(string _name, Action<ITarget> t1, Action<ITarget> t2, Action<ITarget> t3)
        {

        }

        public bool Fix()
        {
            if (owner.current_hp == owner.battle_data.hp)  //这里后续要考虑hp修正
                return false;
            owner.Fix(Mathf.Max(1, (int)(owner.battle_data.hp * Config.current.fix_device_job_effect)));
            return true;
        }

        void IDeviceView.notify_disable()
        {
          //  mask.gameObject.SetActive(true);
        }

        void IDeviceView.notify_enable()
        {
         //   mask.gameObject.SetActive(false);
        }

        public virtual void OperateDrag(Vector2 dir)
        {
            owner.OperateDrag(dir);
        }

        public virtual void JoyStickOper(bool ret)
        {
            if (ret)
                owner.StartControl();
            else
                owner.EndControl();
        }

        public void notify_player_oper(bool oper)
        {

        }

        private void show_emergency(DeviceEmergency de)
        {
            if (de is DeviceFired df)
            {
                Addressable_Utility.try_load_asset("FiredView", out DeviceFiredView dfv);
                dev = Instantiate(dfv, transform, false);
                df.add_view(dev);
            }
            else if (de is DeviceStupor ds)
            {
                Addressable_Utility.try_load_asset("StuporView", out DeviceStuporView dsv);
                dev = Instantiate(dsv, transform, false);
                ds.add_view(dev);
            }
            else if (de is DeviceBind db)
            {
                Addressable_Utility.try_load_asset("BindView", out DeviceBindView dbv);
                dev = Instantiate(dbv, transform, false);
                db.add_view(dev);
            }
            else if(de is DeviceAcid da)
            {
                Addressable_Utility.try_load_asset("AcidView", out DeviceAcidView dav);
                dev = Instantiate(dav, transform, false);
                da.add_view(dev);
            }

            de.init();

            if (danger_icon == null) return;
            danger_icon.gameObject.SetActive(true);
        }

        void IDeviceView.notify_add_emergency(DeviceEmergency de)
        {
            show_emergency(de);

            Debug.Log("发生紧急情况");
        }

        void IDeviceView.notify_remove_emergency(DeviceEmergency de)
        {
            dev = null;
            de.remove_all_views();

            if (danger_icon == null) return;
            danger_icon.gameObject.SetActive(false);
            Debug.Log("解除紧急情况");
        }

        void IDeviceView.notify_attack_radius(bool show)
        {

        }

        public virtual void notify_update_data()
        {

        }

        void IDeviceView.notify_fix(int delta)
        {

        }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            on_click();
        }


        public virtual void on_click()
        {
            
        }

        public virtual void notify_play_audio(string audio_name)
        {
            
        }

        void IDeviceView.notify_change_anim(string anim_name, bool loop, float percent)
        {
            
        }

        void IDeviceView.notify_trigger_event(params object[] args)
        {
            foreach(var view in view_add_ons)
            {
                view.trigger_event(args);   
            }
        }
    }
}
