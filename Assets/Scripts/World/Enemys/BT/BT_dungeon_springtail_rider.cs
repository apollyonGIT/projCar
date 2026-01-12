using Commons;
using System.Collections.Generic;
using UnityEngine;

namespace World.Enemys.BT
{
    public class BT_dungeon_springtail_rider : Enemy_Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Jump, IEnemy_Can_Be_Separated, IEnemy_Can_Shoot
    {
        #region CONST
        const int JUMP_CD = 20;

        const int TARGET_RESELECTED_TICK = 360;

        const float SHOOT_DISTANCE_MIN = 5.5F;
        const float SHOOT_DISTANCE_MAX = 12F;
        const float SHOOT_DISTANCE_MIDDLE = (SHOOT_DISTANCE_MIN + SHOOT_DISTANCE_MAX) * 0.5F;
        const string MUZZLE_BONE_NAME_IN_SPINE = "proj_muzzle";

        const float JUMP_SPEED_X_MIN = 3F;
        const float JUMP_SPEED_X_MAX = 25F;
        const float JUMP_SPEED_Y_MIN = 2.5F;
        const float JUMP_X_DISTANCE_MOD_COEF = 1F;
        const float JUMP_Y_DISTANCE_MOD_COEF = 1 / 32F;

        const ushort TICKS_PER_SEC_IN_SPINE = 30;
        const ushort JUMP_TICK_BY_SPINE = 14 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort SHOOT_ATK_TICK_BY_SPINE = 17 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort SHOOT_END_TICK_BY_SPINE = 30 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;

        const float RIDER_POS_Y_OFFSET = 0.756F - 0.418F;
        #endregion

        private enum EN_dungeon_springtail_rider_FSM
        {
            Default,    // Init State
            Idle,
            Jumping,  // While in Air
            Shoot,  // Attack
        }
        private EN_dungeon_springtail_rider_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

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
        void IEnemy_Can_Jump.Be_Panic_In_Air() => FSM_change_to(EN_dungeon_springtail_rider_FSM.Default);
        #endregion

        #region Interface Separatable
        public bool Separate_All { get; set; }
        public Dictionary<string, int> Sub_Monsters { get; set; }
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
            FSM_change_to((EN_dungeon_springtail_rider_FSM)System.Enum.Parse(typeof(EN_dungeon_springtail_rider_FSM), (string)prms[0]));
            cell.mover.move_type = EN_enemy_move_type.Hover;

            I_Jump = cell.old_bt as IEnemy_Can_Jump;

            Sub_Monsters = cell._desc.sub_monsters;

            I_Shoot = cell.old_bt as IEnemy_Can_Shoot;
            I_Shoot.Init_Shooting_Data(cell);
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            switch (m_state)
            {
                case EN_dungeon_springtail_rider_FSM.Default:
                    if (mover.move_type == EN_enemy_move_type.Slide && cell.acc_attacher == Vector2.zero)
                        FSM_change_to(EN_dungeon_springtail_rider_FSM.Idle);
                    break;

                case EN_dungeon_springtail_rider_FSM.Idle:
                    if (Target_Locked_On == null || Ticks_Target_Has_Been_Locked_On >= TARGET_RESELECTED_TICK)
                        Lock_Target();
                    else
                        Ticks_Target_Has_Been_Locked_On++;

                    if (Target_Locked_On == null)
                        break;

                    var delta_x = Get_Target_Pos().Value.x - cell.pos.x;

                    if (Jump_CD_Ticks <= 0)
                    {
                        FSM_change_to(EN_dungeon_springtail_rider_FSM.Jumping);
                        var abs = Mathf.Abs(delta_x);
                        cell.dir.x = delta_x;

                        if (abs < SHOOT_DISTANCE_MIN)
                            Jump_Mode = IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_Away;
                        else if (abs > SHOOT_DISTANCE_MIDDLE)
                            Jump_Mode = IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_To;
                        else
                            Jump_Mode = IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_Around;

                        break;
                    }
                    else
                        Jump_CD_Ticks--;  //cool down only if has target

                    if (Shoot_CD <= 0)
                    {
                        var abs = Mathf.Abs(delta_x);
                        if (abs >= SHOOT_DISTANCE_MIN && abs <= SHOOT_DISTANCE_MAX)
                            FSM_change_to(EN_dungeon_springtail_rider_FSM.Shoot);
                        cell.dir.x = delta_x;
                    }

                    break;


                case EN_dungeon_springtail_rider_FSM.Jumping:
                    Ticks_In_Current_State++;

                    if ((Jump_Finished && mover.move_type == EN_enemy_move_type.Slide) || Target_Locked_On == null)
                    {
                        FSM_change_to(EN_dungeon_springtail_rider_FSM.Idle);
                        break;
                    }

                    if (!Jump_Finished && Check_State_Time(JUMP_TICK_BY_SPINE))
                        cell.dir.x = I_Jump.Jump_By_Mode(cell, Get_Target_Pos().Value, JUMP_CD) ?? cell.dir.x;

                    Ticks_Target_Has_Been_Locked_On++;

                    break;


                case EN_dungeon_springtail_rider_FSM.Shoot:
                    Ticks_In_Current_State++;

                    if (Check_State_Time(SHOOT_END_TICK_BY_SPINE) || Target_Locked_On == null)
                    {
                        FSM_change_to(EN_dungeon_springtail_rider_FSM.Idle);
                        break;
                    }

                    cell.dir.x = Get_Target_Pos().Value.x - cell.pos.x;

                    if (!Shoot_Finished && Check_State_Time(SHOOT_ATK_TICK_BY_SPINE))
                        I_Shoot.Monster_Shoot(cell, MUZZLE_BONE_NAME_IN_SPINE, Get_Target_Pos().Value, Get_Target_Vel().Value.x);

                    break;

                default:
                    break;
            }

            Shoot_CD = Shoot_CD > 0 ? --Shoot_CD : 0;

            mover.move();
        }


        private void FSM_change_to(EN_dungeon_springtail_rider_FSM expected_FSM)
        {
            m_state = expected_FSM;
            Ticks_In_Current_State = 0;
            switch (expected_FSM)
            {
                case EN_dungeon_springtail_rider_FSM.Idle:
                    Target_Locked_On = null;
                    break;
                case EN_dungeon_springtail_rider_FSM.Shoot:
                    Shoot_Finished = false;
                    break;
                case EN_dungeon_springtail_rider_FSM.Jumping:
                    Jump_Finished = false;
                    break;
                default:
                    break;
            }
        }

        override protected void basic_die(Enemy self)
        {
            IEnemy_Can_Be_Separated i_sepatate = self.old_bt as IEnemy_Can_Be_Separated;
            i_sepatate.Die_Of_Being_Seperated(self);
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

