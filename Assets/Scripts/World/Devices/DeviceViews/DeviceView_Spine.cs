using Commons;
using Foundations;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace World.Devices.DeviceViews
{
    public class DeviceView_Spine : DeviceView
    {
        public SkeletonAnimation anim;

        MaterialPropertyBlock mpb;
        //==================================================================================================

        private void set_anim_process(float percent)
        {
            var current = anim.AnimationState.GetCurrent(0);
            current.AnimationStart = current.Animation.Duration * percent;
        }


        public override void init_pos()
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach (var slot in dmgr.slots_device)
            {
                var device = slot.slot_device;
                var slot_name = slot._name;

                if (device == owner)
                {
                    if (anim != null)
                    {
                        anim.GetComponent<MeshRenderer>().sortingOrder = BattleUtility.slot_2_order_in_layer(slot_name);
                    }
                }
            }

            transform.localPosition = owner.view_position;
            if (owner.desc.rotate_by_car)
            {
                transform.localRotation = EX_Utility.look_rotation_from_left(WorldContext.instance.caravan_dir);
            }

            if (owner.device_type == Device.DeviceType.melee)
                transform.localScale = Vector3.one * BattleContext.instance.melee_scale_factor;
        }

        public  override void notify_change_anim(string anim_name, bool loop)
        {
            if (anim != null)
                anim.state.SetAnimation(0, anim_name, loop);
        }


        /// <summary>
        /// 当当前动画播放完后，重置动画触发事件，准备过渡到下一个动画
        /// </summary>
        /// <param name="entry"></param>
        protected virtual void AnimComplete(Spine.TrackEntry entry)
        {
            var current = anim.state.GetCurrent(0);

            if (entry.Animation.Name != current.Animation.Name)
                return;

            if (current.IsComplete)
            {
                foreach (var anim_event in owner.anim_events)
                {
                    if (anim_event.anim_name == current.Animation.Name && anim_event.triggered == false)
                    {
                        anim_event.triggered = true;
                        anim_event.anim_event?.Invoke(owner);
                    }

                    if (anim_event.anim_name == current.Animation.Name)
                    {
                        anim_event.triggered = false;
                    }
                }
            }
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if (anim != null)
            {
                anim.Update(Config.PHYSICS_TICK_DELTA_TIME);

                foreach (var (bone_name, dir) in owner.bones_direction)
                {
                    var owner_matrix = Matrix4x4.TRS(Vector3.zero, transform.rotation,Vector3.one);
                    var world_to_owner_matrix = owner_matrix.inverse;

                    Vector3 new_dir = world_to_owner_matrix.MultiplyVector(dir);

                    var bone = anim.skeleton.FindBone(bone_name);
                    if (bone != null)
                    {
                        if (Vector2.SignedAngle(Vector2.right, new_dir) > 90)
                        {
                            anim.skeleton.FlipX = true;
                            bone.Rotation = Vector2.SignedAngle(new_dir, Vector2.left);
                        }
                        else if (Vector2.SignedAngle(Vector2.right, new_dir) < -90)
                        {
                            anim.skeleton.FlipX = true;
                            bone.Rotation = Vector2.SignedAngle(new_dir, Vector2.left);
                        }
                        else
                        {
                            anim.skeleton.FlipX = false;
                            bone.Rotation = Vector2.SignedAngle(Vector2.right, new_dir);
                        }
                    }
                }

                foreach (var anim_event in owner.anim_events)
                {
                    if (anim_event.anim_name == anim.AnimationName)
                    {
                        anim_event_trigger(anim_event);
                    }
                }
            }
        }

        protected void anim_event_trigger(AnimEvent ae)
        {
            if (ae.triggered)
                return;
            var current = anim.state.GetCurrent(0);
            var duration_frame = Mathf.CeilToInt(current.Animation.Duration * Config.PHYSICS_TICKS_PER_SECOND);   //动画帧数总长
            var current_frame = Mathf.CeilToInt(current.AnimationTime * Config.PHYSICS_TICKS_PER_SECOND);
            var trigger_frame = Mathf.CeilToInt(duration_frame * ae.percent);

            if (current_frame >= trigger_frame && ae.triggered == false && current_frame < duration_frame)
            {
                ae.triggered = true;
                ae.anim_event?.Invoke(owner);
            }

            //根据ManualUpdate  动画帧和逻辑帧应该同步
        }

        public override void notify_change_anim_speed(float f)
        {
            anim.timeScale = f;
        }

        public override void notify_hurt(int dmg)
        {
            if (anim == null) return;

            var r = anim.GetComponent<MeshRenderer>();
            mpb = new MaterialPropertyBlock();
            mpb.SetFloat("_FillPhase", 1);
            r.SetPropertyBlock(mpb);
            StopCoroutine("IColorRevert");
            StartCoroutine("IColorRevert");

            if(owner.current_hp > 0)
            {
                Audio.AudioSystem.instance.PlayOneShot(owner.desc.se_be_hit);
            }
        }

        IEnumerator IColorRevert()
        {
            yield return new WaitForSeconds(0.1f);

            var p = 1f;
            var r = anim.GetComponent<MeshRenderer>();
            mpb = new MaterialPropertyBlock();
            while (p >= 0)
            {
                p -= 0.02f;
                mpb.SetFloat("_FillPhase", p);
                r.SetPropertyBlock(mpb);
                yield return null;
            }

        }

        public override void attach(Device owner)
        {
            base.attach(owner);

            if (anim != null)
            {
                anim.state.Complete += AnimComplete;
            }
        }

        public override void notify_change_anim(string anim_name, bool loop, float percent)
        {
            if (anim != null)
            {
                anim.state.SetAnimation(0, anim_name, loop);
                set_anim_process(percent);
            }
        }
    }
}

