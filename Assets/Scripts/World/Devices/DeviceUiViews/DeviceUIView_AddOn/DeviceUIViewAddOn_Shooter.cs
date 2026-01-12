using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public abstract class DeviceUIViewAddOn_Shooter : DeviceUIViewAddOn
    {
        public Image target_indicator;
        //public Image reloading_progress_base;
        //public Image reloading_progress;

        // -------------------------------------------------------------------------------

        protected enum Aimming_Status
        {
            No_Target = 0,
            Has_Target = 1,
            Target_Aimed = 2,
        }
        protected Aimming_Status aiming_status = Aimming_Status.No_Target;

        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_reload = new();
        protected List<DevicePanelAttachment_Highlightable> autoWorkHighlight_shoot = new();

        new protected DScript_Shooter owner;

        // -------------------------------------------------------------------------------

        private DeviceLoadCubicle cubicle_reload;
        private DeviceAttackCubicle cubicle_attack;

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
        }

        protected override void attach_owner(Device owner)
        {
            this.owner = owner as DScript_Shooter;
            this.owner.activated = true;
        }

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            if (cubicle is DeviceLoadCubicle)
                cubicle_reload = cubicle as DeviceLoadCubicle;
            else if (cubicle is DeviceAttackCubicle)
                cubicle_attack = cubicle as DeviceAttackCubicle;
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            owner.activated = false;
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();


            if (owner == null)
                Debug.Log("wtf");
            aiming_status = (Aimming_Status)owner.Get_Targets_Status();

            //reloading_progress_base.gameObject.SetActive(owner.is_reloading());
            //reloading_progress.fillAmount = owner.Reloading_Process;

            if (target_indicator == null)
                return;

            switch (aiming_status)
            {
                case Aimming_Status.Target_Aimed:
                    target_indicator.color = Color.green;
                    break;
                case Aimming_Status.Has_Target:
                    target_indicator.color = Color.yellow;
                    break;
                case Aimming_Status.No_Target:
                default:
                    target_indicator.color = Color.red;
                    break;
            }
        }

        protected override void update_cubicle_on_tick()
        {
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_reload, autoWorkHighlight_reload);
            DeviceUIView_Common_Action.Set_Highlight_By_If_Cubicle_Has_Worker(cubicle_attack, autoWorkHighlight_shoot);
        }

        // ===============================================================================

        protected bool Barrel_Bullet_Loaded
        {
            get { return owner.Get_Barrel() == 2; }
        }

        // -------------------------------------------------------------------------------

        protected virtual bool shoot(bool ignore_post_cast = false, Action shoot_finished = null)
        {
            return owner.Try_Shooting(ignore_post_cast, shoot_finished);
        }

        protected virtual void reload()
        {
            owner.Try_Reloading();
        }

        protected virtual void start_reloading()
        {
            owner.Try_Start_Reloading();
        }

        protected virtual void load_bullet_into_barrel()
        {
            owner.Load_Bullet_Into_Barrel();
        }

    }
}
