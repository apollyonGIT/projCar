using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Cubicles;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Unique_Drum : DeviceUIViewAddOn
    {
        protected new Unique_War_Drum owner;

        private DeviceAttackCubicle cubicle_attack;

        public Slider attack_slider;

        public List<DevicePanelAttachment_Beat> beats_view = new();

        public Transform beat_content;

        public DevicePanelAttachment_Beat beat_prefab;

        public DevicePanelAttachment_Drum_Stick right_stick, left_stick;

        public Image drum_image;

        public Sprite[] drum_sprite;

        public override void attach(Device owner)
        {
            base.attach(owner);
            List<Action> right_stick_actions = new List<Action>();
            List<Action> left_stick_actions = new List<Action>();
            right_stick_actions.Add(()=>
            {
                (this.owner).UI_Controlled_Beating(true);
            });
            
            left_stick_actions.Add(() =>
            {
                (this.owner).UI_Controlled_Beating(false);
            });

            right_stick.Init(right_stick_actions);
            left_stick.Init(left_stick_actions);
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            drum_image.sprite = owner.cubicle_list[0].worker == null ? drum_sprite[0] : drum_sprite[1];


            attack_slider.value = owner.current_attacking_tick;
            attack_slider.maxValue = owner.attacking_tick;

            int view_cnt = beats_view.Count;
            if(view_cnt - owner.beats.Count > 0)
            {
                var delta = view_cnt - owner.beats.Count;
                
                for(int i = 0; i < delta; i++)
                {
                    Destroy(beats_view[0].gameObject);
                    beats_view.RemoveAt(0);
                }
            }
            else
            {
                var delta = owner.beats.Count - view_cnt;

                for(int i = 0;i < delta; i++)
                {
                    var new_beat = Instantiate(beat_prefab, beat_content);
                    new_beat.gameObject.SetActive(true);
                    beats_view.Add(new_beat);
                }
            }

            for(int i = 0; i < owner.beats.Count; i++)
            {
                beats_view[i].SetBeat(owner.beats[i].is_right,owner.beats[i].state);
            }
        }


        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            if (cubicle is DeviceAttackCubicle)
                cubicle_attack = cubicle as DeviceAttackCubicle;
        }

        protected override void attach_highlightable()
        {
            
        }

        protected override void attach_owner(Device owner)
        {
            this.owner = owner as Unique_War_Drum;
        }

        protected override void update_cubicle_on_tick()
        {
            
        }
    }
}
