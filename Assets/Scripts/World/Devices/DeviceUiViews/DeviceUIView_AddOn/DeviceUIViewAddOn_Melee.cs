using UnityEngine;
using UnityEngine.UI;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Melee : DeviceUIViewAddOn
    {
        #region Const
        private const float ATTACK_TRIGGER_DISTANCE = 500;
        private const float ATTACK_TRIGGER_DECAY = 0.85F; // 衰减系数
        #endregion

        public Image img_of_atk;
        public Image img_of_atk_highlight;

        public GameObject autoWorkIndicator_attack;

        //public DevicePanelAttachment_Pickupable Widget_Attack;
        public DevicePanelAttachment_Trigger Widget_Trigger_Attack;



        // -------------------------------------------------------------------------------

        new protected BasicMelee_Click owner;

        // -------------------------------------------------------------------------------

        private DeviceAttackCubicle cubicle_attack;

        private Vector2 _atk_ui_weave_distance;  // 挥舞攻击必定是通过ui触发的，因此挥舞攻击的距离记录在ui中即可。
        private bool atk_img_flip;

        private bool _can_horizontal_weave;
        private bool _can_vertical_weave;

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
           // Widget_Attack.Init();

            _can_horizontal_weave = this.owner.UI_Atk_Can_Horizontal;
            _can_vertical_weave = this.owner.UI_Atk_Can_Vertical;
        }

        protected override void attach_owner(Device owner)
        {
            this.owner = owner as BasicMelee_Click;
        }

        protected override void attach_cubicle(BasicCubicle cubicle)
        {
            if (cubicle is DeviceAttackCubicle)
                cubicle_attack = cubicle as DeviceAttackCubicle;
        }

        protected override void attach_highlightable()
        {
            //autoWorkHighlight_rotate.Add(widget_ratchet_for_dir);
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();
            var atking = owner.UI_Info_Is_Attacking();
            img_of_atk.color = atking ? Color.yellow : Color.white;
            img_of_atk_highlight.color = atking ? Color.yellow : Color.white;
            melee_weave();

            img_of_atk.rectTransform.localScale = new Vector3(atk_img_flip ? -1 : 1, 1, 1);
        }

        protected override void update_cubicle_on_tick()
        {
            if (cubicle_attack != null)
                autoWorkIndicator_attack.SetActive(cubicle_attack.worker != null);
        }

        // ===============================================================================

        protected void melee_weave()
        {
            /*if (Widget_Attack.Is_Picked_Up())
            {
                _atk_ui_weave_distance += Widget_Attack.Get_Move_Distance_In_A_Tick();

                if (_can_horizontal_weave && horizontal_atk_trigger())
                {
                    attack(true);
                    atk_img_flip = !atk_img_flip;
                    _atk_ui_weave_distance = Vector2.zero;
                }
                else if (_can_vertical_weave && vertical_atk_trigger())
                {
                    attack(false);
                    _atk_ui_weave_distance = Vector2.zero;
                }
                else
                {
                    _atk_ui_weave_distance *= ATTACK_TRIGGER_DECAY;
                }
            }
            else
            {
                _atk_ui_weave_distance = Vector2.zero;
                atk_img_flip = false;
            }*/

            bool horizontal_atk_trigger()
            {
                return (!atk_img_flip && _atk_ui_weave_distance.x > ATTACK_TRIGGER_DISTANCE) || (atk_img_flip && _atk_ui_weave_distance.x < -ATTACK_TRIGGER_DISTANCE);
            }

            bool vertical_atk_trigger()
            {
                return _atk_ui_weave_distance.y > ATTACK_TRIGGER_DISTANCE;
            }
        }

        // ===============================================================================

        protected void attack(bool is_horizontal)
        {
            owner.UI_Controlled_Attack(is_horizontal);
        }
    }
}
