using AutoCodes;
using UnityEngine;
using World.Enemys;
using World.Enemys.BT;

namespace World.Devices
{
    public class Unique_Catching_Flower : Device
    {
        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK = "attack";
        private const string ANIM_CHEWING = "chewing";
        private const string ANIM_CHEWED = "chewed";
        private const string ANIM_BROKEN = "idle";

        private const string BONE_FOR_ROTATION = "control";

        private const string COLLIDER_SWALLOW = "collider_1";

        private const float ROTATE_SPEED = 5F;

        private const float ATTACK_DISTANCE_SQR = 4F;

        private const int DIGESTION_HP_RATIO = 30;

        private const float SWALLOW_ON_TIME_BY_SPINE = 10f / 15f;
        private const float SWALLOW_OFF_TIME_BY_SPINE = 12f / 15f;
        #endregion

        private enum Device_Catching_Flower_FSM
        {
            idle,
            attack,
            chewing,
            chewed,
            broken
        }
        private Device_Catching_Flower_FSM fsm;

        private int digestion;
        private bool can_swallow;


        public override void InitData(device_all rc)
        {
            bones_direction.Clear();

            bones_direction.Add(BONE_FOR_ROTATION, Vector2.up);
            
            base.InitData(rc);

            #region Anim Events
            var attack_collider_on = new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = SWALLOW_ON_TIME_BY_SPINE,
                anim_event = (Device d) =>
                {
                    d.OpenCollider(COLLIDER_SWALLOW, (ITarget t) =>
                    {
                        Enemy e = t as Enemy;
                        Enemy_Monster_Basic_BT ebt = e.old_bt as Enemy_Monster_Basic_BT;
                        if (ebt != null && can_swallow)
                        {
                            ebt.get_killed(e);
                            can_swallow = false;
                            digestion = e.hp * DIGESTION_HP_RATIO;
                            d.CloseCollider(COLLIDER_SWALLOW);
                            FSM_change_to(Device_Catching_Flower_FSM.chewing);
                        }
                    });
                }
            };

            var attack_collider_off = new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = SWALLOW_OFF_TIME_BY_SPINE,
                anim_event = (Device d) =>
                {
                    d.CloseCollider(COLLIDER_SWALLOW);
                }
            };

            var attack_end = new AnimEvent()
            {
                anim_name = ANIM_ATTACK,
                percent = 1F,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_Catching_Flower_FSM.idle);
                }
            };

            var chew_end = new AnimEvent()
            {
                anim_name = ANIM_CHEWED,
                percent = 1F,
                anim_event = (Device d) =>
                {
                    FSM_change_to(Device_Catching_Flower_FSM.idle);
                }
            };
            #endregion

            anim_events.Add(attack_collider_on);
            anim_events.Add(attack_collider_off);
            anim_events.Add(attack_end);
            anim_events.Add(chew_end);
        }

        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_Catching_Flower_FSM.idle);
        }
        public override void tick()
        {
            if (!is_validate)       //坏了
                FSM_change_to(Device_Catching_Flower_FSM.broken);

            switch (fsm)
            {
                case Device_Catching_Flower_FSM.idle:
                case Device_Catching_Flower_FSM.attack:
                    break;

                case Device_Catching_Flower_FSM.chewing:
                    if (digestion <= 0)
                        FSM_change_to(Device_Catching_Flower_FSM.chewed);
                    else
                        digestion--;
                    break;

                case Device_Catching_Flower_FSM.chewed:
                    break;

                case Device_Catching_Flower_FSM.broken:
                    if (is_validate)
                        FSM_change_to(Device_Catching_Flower_FSM.idle);
                    break;

                default:
                    break;
            }
            base.tick();
        }

        private void FSM_change_to(Device_Catching_Flower_FSM target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_Catching_Flower_FSM.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_Catching_Flower_FSM.attack:
                    can_swallow = true;
                    ChangeAnim(ANIM_ATTACK, false);
                    break;
                case Device_Catching_Flower_FSM.chewing:
                    ChangeAnim(ANIM_CHEWING, true);
                    break;
                case Device_Catching_Flower_FSM.chewed:
                    ChangeAnim(ANIM_CHEWED, false);
                    break;
                case Device_Catching_Flower_FSM.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    break;
                default:
                    break;
            }
        }

        protected override bool target_in_radius(ITarget t)
        {
            var tp = BattleUtility.get_target_colllider_pos(t);
            if (WorldContext.instance.is_need_reset)
            {
                tp -= new Vector2(WorldContext.instance.reset_dis, 0);
            }
            var t_distance = (tp - position).magnitude;
            return t_distance <= desc.basic_range.Item2;
        }

        protected override bool try_get_target()
        {
            var target = BattleUtility.select_target_in_circle_min_angle(position, bones_direction[BONE_FOR_ROTATION], desc.basic_range.Item2, faction, (ITarget t) =>
            {
                return target_can_be_selected(t);
            });

            if(target!=null)
                target_list.Add(target);

            //这里即使返回null也是可以接受的，无需排除此结果
            return target != null;
        }


        private bool can_attack()
        {
            if (target_list[0] == null)
                return false;
            var t_pos_delta = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            return t_pos_delta.sqrMagnitude <= ATTACK_DISTANCE_SQR;
        }

        private void Auto()
        {
            switch (fsm)
            {
                case Device_Catching_Flower_FSM.idle:
                    if (target_list.Count == 0)
                        try_get_target();

                    rotate_bone_to_target(BONE_FOR_ROTATION);

                    if (can_attack())
                        FSM_change_to(Device_Catching_Flower_FSM.attack);
                    break;
                case Device_Catching_Flower_FSM.attack:
                case Device_Catching_Flower_FSM.chewing:
                case Device_Catching_Flower_FSM.chewed:
                case Device_Catching_Flower_FSM.broken:
                default:
                    break;
            }
        }


        #region PlayerControl
        public override void StartControl()
        {
            InputController.instance.left_hold_event += Attack_1;
            InputController.instance.right_hold_event += Aiming;
            base.StartControl();
        }

        public override void EndControl()
        {
            InputController.instance.left_hold_event -= Attack_1;
            InputController.instance.right_hold_event -= Aiming;
            base.EndControl() ;
        }

        private void Aiming()
        {
            if (fsm == Device_Catching_Flower_FSM.idle)
            {
                var dir = InputController.instance.GetWorldMousePosition() - new Vector3(position.x, position.y, 10);
                rotate_bone_to_dir(BONE_FOR_ROTATION, dir);
            }
        }

        private void Attack_1()
        {
                if (fsm == Device_Catching_Flower_FSM.idle)
                    FSM_change_to(Device_Catching_Flower_FSM.attack);
        }
        #endregion
    }


}