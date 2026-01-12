using Commons;
using Foundations;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Audio;
using World.Devices.DeviceEmergencies;
using World.Helpers;
using World.VFXs;

namespace World.Devices.DeviceViews
{
    public class DeviceView : MonoBehaviour, IDeviceView
    {
        public Device owner;

        public List<DeviceKeyPoint> dkp = new();

        public List<DeviceCollider> colliders = new();
        public virtual void attach(Device owner)
        {
            this.owner = owner;
            transform.localPosition = owner.position;

            foreach (var collider in colliders)
            {
                collider.device = owner;
            }
        }

        public virtual void detach(Device owner)
        {
            this.owner = null;
            Destroy(gameObject);
        }

        public virtual void init()
        {
            
        }

        public virtual void init_pos()
        {
            
        }

        protected VFX outlier_vfx;

        public virtual void notify_add_emergency(DeviceEmergency de)
        {
            DeviceKeyPoint p = dkp.Find(x => x.key_name == "fire");

            if (de is DeviceFired)
            {
                if (p != null)
                    outlier_vfx = Vfx_Helper.InstantiateVfx("buff_fire_vfx", 9999, p.transform, true);
            }
            else if (de is DeviceAcid)
            {
                if (p != null)
                    outlier_vfx = Vfx_Helper.InstantiateVfx("buff_acid_vfx", 9999, p.transform, true);
            }
            else if (de is DeviceStupor)
            {
                if (p != null)
                    outlier_vfx = Vfx_Helper.InstantiateVfx("buff_stupor_vfx", 9999, p.transform, true);
            }
            else if (de is DeviceBind)
            {
                if (p != null)
                    outlier_vfx = Vfx_Helper.InstantiateVfx("buff_bind_vfx", 9999, p.transform, true);
            }

            Debug.Log("发生紧急情况");
        }

        public virtual void notify_attack_radius(bool show)
        {
            
        }

        public virtual void notify_change_anim(string anim_name, bool loop)
        {
            
        }

        public virtual void notify_change_anim(string anim_name, bool loop, float percent)
        {
            
        }

        public virtual void notify_change_anim_speed(float f)
        {
            
        }

        public virtual void notify_close_collider(string _name)
        {
            foreach (var collider in colliders)
            {
                if (collider.collider_name == _name)
                {
                    collider.gameObject.SetActive(false);
                    collider.enter_event = null;
                    collider.stay_event = null;
                    collider.exit_event = null;
                }
            }
        }


        protected VFX smoke_vfx;

        public virtual void notify_disable()
        {
            Vfx_Helper.InstantiateVfx(Config.current.devicedestroy_vfx, 240, (Vector2)transform.position);

            DeviceKeyPoint p = dkp.Find(x => x.key_name == "fire");

            smoke_vfx = Vfx_Helper.InstantiateVfx(Config.current.devicesmoke_vfx, 9999, p.transform, true);

            Audio.AudioSystem.instance.PlayOneShot(owner.desc.se_broken);
        }

        public virtual void notify_enable()
        {
            if (smoke_vfx != null)
                smoke_vfx.duration = 0;
        }

        public virtual void notify_fix(int delta)
        {
            
        }

        public virtual void notify_hurt(int dmg)
        {
            
        }

        public virtual void notify_on_tick()
        {
            transform.localPosition = owner.view_position;

            if (owner.desc.rotate_by_car)
            {
                transform.localRotation = EX_Utility.look_rotation_from_left(WorldContext.instance.caravan_dir);
            }

            if (owner.device_type == Device.DeviceType.melee)
                transform.localScale = Vector3.one * BattleContext.instance.melee_scale_factor;

            foreach (var collider in colliders)
            {
                collider.collider_tick();
            }
        }

        public virtual void notify_open_collider(string _name, Action<ITarget> enter_e = null, Action<ITarget> stay_e = null, Action<ITarget> exit_e = null)
        {
            foreach (var collider in colliders)
            {
                if (collider.collider_name == _name && collider.gameObject.activeInHierarchy == false)
                {
                    collider.gameObject.SetActive(true);
                    collider.enter_event += enter_e;
                    collider.stay_event += stay_e;
                    collider.exit_event += exit_e;
                }
            }
        }

        public virtual void notify_player_oper(bool oper)
        {
            
        }

        public virtual void notify_play_audio(string audio_name)
        {
            AudioSystem.instance.PlayOneShot(audio_name);
        }

        public virtual void notify_remove_emergency(DeviceEmergency de)
        {
            if (outlier_vfx != null)
                outlier_vfx.duration = 0;

            Debug.Log("解除紧急情况");
        }

        public virtual void notify_trigger_event(params object[] args)
        {
            
        }

        public virtual void notify_update_data()
        {
            
        }
    }
}
