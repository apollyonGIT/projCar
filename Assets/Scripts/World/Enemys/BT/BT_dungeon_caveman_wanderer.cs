using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_dungeon_caveman_wanderer : Enemy_Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Jump, IEnemy_Can_Shoot
    {
        #region CONST
        const int JUMP_CD = 7;

        const int TARGET_RESELECTED_TICK = 360;

        const float SHOOT_DISTANCE_MIN = 5.5F;
        const float SHOOT_DISTANCE_MAX = 13F;
        const string MUZZLE_BONE_NAME_IN_SPINE = "proj_muzzle";

        const float MOVE_TO_DISTANCE_MAX = (SHOOT_DISTANCE_MIN + SHOOT_DISTANCE_MAX) * 0.5F;

        const float JUMP_SPEED_X_MIN = 2F;
        const float JUMP_SPEED_X_MAX = 4F;
        const float JUMP_SPEED_Y_MIN = 2F;
        const float JUMP_X_DISTANCE_MOD_COEF = 0.1F;
        const float JUMP_Y_DISTANCE_MOD_COEF = 0F;

        const ushort TICKS_PER_SEC_IN_SPINE = 30;
        const ushort SHOOT_ATK_TICK_IN_SPINE = 27 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort SHOOT_END_TICK_IN_SPINE = 60 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        #endregion

        private enum EN_dungeon_caveman_wanderer_FSM
        {
            Default,    // Init State
            Idle,
            Walk_1,
            Walk_2,
            Atk_Shoot,
        }
        private EN_dungeon_caveman_wanderer_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        bool move_behavior_rnd_delayed_once;
        private bool walk_left_right_change;

        Enemy self;

        #region Interface Jump
        public bool Jump_Finished { get; set; }
        public int Jump_CD_Ticks { get; set; } = JUMP_CD;
        public IEnemy_Can_Jump.Enemy_Jump_Mode Jump_Mode { get; set; }
        public IEnemy_Can_Jump I_Jump { get; set; }
        public float Jump_Speed_Min { get; set; } = JUMP_SPEED_X_MIN;
        public float Jump_Speed_Max { get; set; } = JUMP_SPEED_X_MAX;
        public float Jump_Y_Speed_Min { get; set; } = JUMP_SPEED_Y_MIN;
        public float X_Distance_Mod_Coef { get; set; } = JUMP_X_DISTANCE_MOD_COEF;
        public float Y_Distance_Mod_Coef { get; set; } = JUMP_Y_DISTANCE_MOD_COEF;
        void IEnemy_Can_Jump.Be_Panic_In_Air() => FSM_change_to(EN_dungeon_caveman_wanderer_FSM.Default);
        #endregion

        #region Interface Shoot
        public int Shoot_CD { get; set; }
        public int Shoot_CD_Max { get; set; }
        public bool Shoot_Finished { get; set; }
        public float Projectile_Speed { get; set; }
        public (float, float) Projectile_Speed_Range { get; set; }
        public IEnemy_Can_Shoot I_Shoot { get; set; }
        #endregion

        //==================================================================================================

        void IEnemy_BT.init(Enemy cell, params object[] prms)
        {
            FSM_change_to((EN_dungeon_caveman_wanderer_FSM)System.Enum.Parse(typeof(EN_dungeon_caveman_wanderer_FSM), (string)prms[0]));
            cell.mover.move_type = EN_enemy_move_type.Hover;
            I_Jump = cell.old_bt as IEnemy_Can_Jump;
            self = cell;

            I_Shoot = cell.old_bt as IEnemy_Can_Shoot;
            I_Shoot.Init_Shooting_Data(cell);
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            if (cell.acc_attacher != Vector2.zero)
                FSM_change_to(EN_dungeon_caveman_wanderer_FSM.Default);

            Shoot_CD = Shoot_CD <= 0 ? 0 : --Shoot_CD;

            switch (m_state)
            {
                case EN_dungeon_caveman_wanderer_FSM.Default:
                    if (mover.move_type == EN_enemy_move_type.Slide && cell.acc_attacher == Vector2.zero)
                        FSM_change_to(EN_dungeon_caveman_wanderer_FSM.Idle);
                    break;

                case EN_dungeon_caveman_wanderer_FSM.Idle:
                    if (Target_Locked_On == null || Ticks_Target_Has_Been_Locked_On >= TARGET_RESELECTED_TICK)
                        Lock_Target();
                    else
                        Ticks_Target_Has_Been_Locked_On++;

                    if (Target_Locked_On != null)
                    {
                        var delta_pos = Get_Target_Pos().Value - cell.pos;
                        var distance = delta_pos.magnitude;

                        if (Shoot_CD <= 0 && distance <= SHOOT_DISTANCE_MAX && distance >= SHOOT_DISTANCE_MIN)
                        {
                            FSM_change_to(EN_dungeon_caveman_wanderer_FSM.Atk_Shoot);
                            cell.dir.x = delta_pos.x;
                            break;
                        }

                        if (Jump_CD_Ticks <= 0)
                        {
                            if (move_behavior_rnd_delayed_once)
                                move_behavior_rnd_delayed_once = false;
                            else if (Random.value < 0.9f)
                            {
                                Jump_CD_Ticks = Random.Range(10, 60);
                                Shoot_CD += Random.Range(10, 20);
                                move_behavior_rnd_delayed_once = true;
                                break;
                            }

                            if (distance < Random.Range(0, SHOOT_DISTANCE_MIN))
                                Jump_Mode = IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_Away;
                            else if (distance > Random.Range(MOVE_TO_DISTANCE_MAX, SHOOT_DISTANCE_MAX))
                                Jump_Mode = IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_To;
                            else
                                Jump_Mode = IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_Around;

                            FSM_change_to(walk_left_right_change ? EN_dungeon_caveman_wanderer_FSM.Walk_1 : EN_dungeon_caveman_wanderer_FSM.Walk_2);
                        }
                        else
                            Jump_CD_Ticks--;  //cool down only if has target                  
                    }

                    break;

                case EN_dungeon_caveman_wanderer_FSM.Walk_1:
                case EN_dungeon_caveman_wanderer_FSM.Walk_2:
                    if (mover.move_type == EN_enemy_move_type.Slide)
                        m_state = EN_dungeon_caveman_wanderer_FSM.Idle;
                    break;

                case EN_dungeon_caveman_wanderer_FSM.Atk_Shoot:
                    Ticks_In_Current_State++;

                    if (Check_State_Time(SHOOT_END_TICK_IN_SPINE) || Target_Locked_On == null)
                    {
                        FSM_change_to(EN_dungeon_caveman_wanderer_FSM.Idle);
                        break;
                    }

                    cell.dir.x = Get_Target_Pos().Value.x - cell.pos.x;
                    if (!Shoot_Finished && Check_State_Time(SHOOT_ATK_TICK_IN_SPINE))
                        I_Shoot.Monster_Shoot(cell, MUZZLE_BONE_NAME_IN_SPINE, Get_Target_Pos().Value, Get_Target_Vel().Value.x);

                    break;

                default:
                    break;
            }

            mover.move();

        }

        private void FSM_change_to(EN_dungeon_caveman_wanderer_FSM expected_FSM)
        {
            m_state = expected_FSM;
            Ticks_In_Current_State = 0;
            switch (expected_FSM)
            {
                case EN_dungeon_caveman_wanderer_FSM.Atk_Shoot:
                    Shoot_Finished = false;
                    break;
                case EN_dungeon_caveman_wanderer_FSM.Walk_1:
                case EN_dungeon_caveman_wanderer_FSM.Walk_2:
                    self.dir.x = I_Jump.Jump_By_Mode(self, Get_Target_Pos().Value, JUMP_CD) ?? self.dir.x;
                    walk_left_right_change = !walk_left_right_change;
                    break;
                default:
                    break;
            }
        }

        void IEnemy_BT.notify_on_enter_die(Enemy cell)
        {
            basic_die(cell);
        }


        void IEnemy_BT.notify_on_dying(Enemy cell)
        {

        }


        void IEnemy_BT.notify_on_dead(Enemy cell)
        {

        }
    }
}

