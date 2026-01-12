using Commons;
using System;
using UnityEngine;


namespace World.Enemys.BT
{
    public class BT_dungeon_springtail : Enemy_Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Jump
    {
        #region CONST
        const int JUMP_CD_MIN = 80;
        const int JUMP_CD_MAX = 100;
        const int MELEE_ATK_CD = 240;

        const int TARGET_RESELECTED_TICK = 360;

        const float JUMP_SPEED_X_MIN = 0.75F;
        const float JUMP_SPEED_X_MAX = 30F;

        const float JUMP_SPEED_Y_MIN = 3.5F;
        const float JUMP_X_DISTANCE_MOD_COEF = 1.75F;
        const float JUMP_Y_DISTANCE_MOD_COEF = 1 / 32F;

        const float BITE_ATTACK_DISTANCE = 1F;

        const ushort TICKS_PER_SEC_IN_SPINE = 30;
        const ushort JUMP_TICK_BY_SPINE = 12 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort ATK_DMG_TICK_BY_SPINE = 4 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort ATK_END_TICK_BY_SPINE = 8 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        #endregion

        private enum EN_dungeon_springtail_FSM
        {
            Default,    // Init State
            Idle,
            Jumping,
            Atk_Melee,
            Falling,    // Die and Fall
        }
        private EN_dungeon_springtail_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        private int melee_atk_cd = UnityEngine.Random.Range(0, MELEE_ATK_CD);
        private bool melee_atk_finished;
        static private int Rnd_Jump_CD => UnityEngine.Random.Range(JUMP_CD_MIN, JUMP_CD_MAX);

        #region Interface Jump
        public bool Jump_Finished { get; set; }
        public int Jump_CD_Ticks { get; set; } = Rnd_Jump_CD;
        public IEnemy_Can_Jump.Enemy_Jump_Mode Jump_Mode { get; set; }
        public IEnemy_Can_Jump I_Jump { get; set; }
        public float Jump_Speed_Min { get; set; } = JUMP_SPEED_X_MIN;
        public float Jump_Speed_Max { get; set; } = JUMP_SPEED_X_MAX;
        public float Jump_Y_Speed_Min { get; set; } = JUMP_SPEED_Y_MIN;
        public float X_Distance_Mod_Coef { get; set; } = JUMP_X_DISTANCE_MOD_COEF * UnityEngine.Random.Range(0.9f, 1.1f);
        public float Y_Distance_Mod_Coef { get; set; } = JUMP_Y_DISTANCE_MOD_COEF * UnityEngine.Random.Range(0.9f, 1.1f);
        void IEnemy_Can_Jump.Be_Panic_In_Air() => FSM_change_to(EN_dungeon_springtail_FSM.Default);
        #endregion

        //==================================================================================================

        void IEnemy_BT.init(Enemy cell, params object[] prms)
        {
            FSM_change_to((EN_dungeon_springtail_FSM)Enum.Parse(typeof(EN_dungeon_springtail_FSM), (string)prms[0]));
            cell.mover.move_type = EN_enemy_move_type.Hover;

            I_Jump = cell.old_bt as IEnemy_Can_Jump;
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            if (cell.acc_attacher != Vector2.zero)
                FSM_change_to(EN_dungeon_springtail_FSM.Default);

            switch (m_state)
            {
                case EN_dungeon_springtail_FSM.Default:
                    if (mover.move_type == EN_enemy_move_type.Slide && cell.acc_attacher == Vector2.zero)
                        FSM_change_to(EN_dungeon_springtail_FSM.Idle);
                    break;

                case EN_dungeon_springtail_FSM.Idle:
                    if (Jump_CD_Ticks > 0)
                        break;

                    if (Target_Locked_On == null || Ticks_Target_Has_Been_Locked_On >= TARGET_RESELECTED_TICK)
                        Lock_Target();

                    if (Target_Locked_On == null)
                        break;

                    var delta_x = Get_Target_Pos().Value.x - cell.pos.x;
                    cell.dir.x = delta_x;
                    Jump_Mode = Mathf.Abs(delta_x) < BITE_ATTACK_DISTANCE ? IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_Around : IEnemy_Can_Jump.Enemy_Jump_Mode.Jump_To;
                    FSM_change_to(EN_dungeon_springtail_FSM.Jumping);

                    break;


                case EN_dungeon_springtail_FSM.Jumping:
                    Ticks_In_Current_State++;

                    if (Jump_Finished && mover.move_type == EN_enemy_move_type.Slide)
                        Target_Locked_On = null;

                    if (Target_Locked_On == null || Jump_Finished == (mover.move_type == EN_enemy_move_type.Slide))
                    {
                        FSM_change_to(EN_dungeon_springtail_FSM.Idle);
                        break;
                    }

                    if (!Jump_Finished && Check_State_Time(JUMP_TICK_BY_SPINE))
                        cell.dir.x = I_Jump.Jump_By_Mode(cell, Get_Target_Pos().Value, Rnd_Jump_CD) ?? cell.dir.x;

                    var dis = Get_Target_Pos().Value - cell.pos;
                    if (melee_atk_cd <= 0 && dis.magnitude < BITE_ATTACK_DISTANCE)
                    {
                        FSM_change_to(EN_dungeon_springtail_FSM.Atk_Melee);
                        cell.dir.x = dis.x;
                    }

                    break;

                case EN_dungeon_springtail_FSM.Atk_Melee:
                    Ticks_In_Current_State++;
                    if (!melee_atk_finished && Check_State_Time(ATK_DMG_TICK_BY_SPINE))
                    {
                        melee_atk_finished = true;

                        Attack_Data attack_data = new()
                        {
                            atk = 10
                        };

                        Target_Locked_On.hurt(cell, attack_data, out var dmg_data);
                        melee_atk_cd = MELEE_ATK_CD;
                    }
                    else if (Check_State_Time(ATK_END_TICK_BY_SPINE))
                        FSM_change_to(mover.move_type == EN_enemy_move_type.Slide ? EN_dungeon_springtail_FSM.Idle : EN_dungeon_springtail_FSM.Default);
                    break;

                default:
                    break;
            }

            mover.move();

            if (melee_atk_cd > 0)
                melee_atk_cd--;

            if (Target_Locked_On != null && Ticks_Target_Has_Been_Locked_On < ushort.MaxValue)
                Ticks_Target_Has_Been_Locked_On++;

            if (Jump_CD_Ticks > 0)
                Jump_CD_Ticks--;
        }


        private void FSM_change_to(EN_dungeon_springtail_FSM expected_FSM)
        {
            m_state = expected_FSM;
            Ticks_In_Current_State = 0;
            switch (expected_FSM)
            {
                case EN_dungeon_springtail_FSM.Default:
                    break;
                case EN_dungeon_springtail_FSM.Idle:
                    break;
                case EN_dungeon_springtail_FSM.Jumping:
                    Jump_Finished = false;
                    break;
                case EN_dungeon_springtail_FSM.Atk_Melee:
                    melee_atk_finished = false;
                    break;
                case EN_dungeon_springtail_FSM.Falling:
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

