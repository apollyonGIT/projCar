using System.Collections.Generic;
using Commons;
using UnityEngine;

namespace World.Enemys.BT
{
    public class BT_dungeon_bat_rider : Enemy_Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Be_Separated, IEnemy_Can_Shoot, IEnemy_Can_Flyaround
    {
        #region CONST
        const float FLYAROUND_DEG_MIN = 40F;
        const float FLYAROUND_DEG_MAX = 140F;
        const float FLYAROUND_RADIUS_MIN = 4.5F;
        const float FLYAROUND_RADIUS_MAX = 7F;
        const float FLYAROUND_RADIUS_RATIO = 0.66F;

        const string MUZZLE_BONE_NAME_IN_SPINE = "proj_muzzle";

        const float FLY_SPEED_IDLE = 4F;
        const float FLY_SPEED_MAX = 22F;

        const float SHOOT_DISTANCE_MIN = 6F;
        const float SHOOT_DISTANCE_MAX = 12.5F;

        const float SHOOT_DY_MIN = 4F;

        const ushort TICKS_PER_SEC_IN_SPINE = 30;
        const ushort SHOOT_ATK_TICK_BY_SPINE = 21 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        const ushort SHOOT_END_TICK_BY_SPINE = 35 * Config.PHYSICS_TICKS_PER_SECOND / TICKS_PER_SEC_IN_SPINE;
        #endregion
        private enum EN_dungeon_bat_rider_FSM
        {
            Default,
            Ready,
            Shoot
        }
        EN_dungeon_bat_rider_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

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

        #region Interface Flyaround
        public (float, float) Flyaround_Deg { get; } = (FLYAROUND_DEG_MIN, FLYAROUND_DEG_MAX);
        public (float, float) Flyaround_Radius { get; } = (FLYAROUND_RADIUS_MIN, FLYAROUND_RADIUS_MAX);
        public float Flyaround_Radius_Ratio { get; } = FLYAROUND_RADIUS_RATIO;
        public Vector2 Flyaround_Relative_Following_Pos { get; set; }
        #endregion
        private IEnemy_Can_Flyaround I_Flyaround;

        //==================================================================================================

        void IEnemy_BT.init(Enemy cell, params object[] prms)
        {
            FSM_change_to((EN_dungeon_bat_rider_FSM)System.Enum.Parse(typeof(EN_dungeon_bat_rider_FSM), (string)prms[0]));
            cell.mover.move_type = EN_enemy_move_type.Fly;

            Sub_Monsters = cell._desc.sub_monsters;

            I_Shoot = cell.old_bt as IEnemy_Can_Shoot;
            I_Shoot.Init_Shooting_Data(cell);

            I_Flyaround = cell.old_bt as IEnemy_Can_Flyaround;
            I_Flyaround.Init_Or_Reset_Relative_Following_Pos();
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            Set_Chasing_Speed(cell, FLY_SPEED_IDLE, FLY_SPEED_MAX);

            // Main FSM
            switch (m_state)
            {
                case EN_dungeon_bat_rider_FSM.Default:
                    Vector2 target_pos_focus = Target_Locked_On == null ? ctx.caravan_pos : Target_Locked_On.Position;
                    I_Flyaround.Flyaround_Per_Tick(cell, target_pos_focus);

                    if (Target_Locked_On == null)
                        Lock_Target();

                    if (Target_Locked_On != null)
                    {
                        if (Shoot_CD <= 0)
                            FSM_change_to(EN_dungeon_bat_rider_FSM.Ready);
                        else
                            Shoot_CD--;
                    }

                    cell.dir.x = target_pos_focus.x - cell.pos.x;
                    break;

                case EN_dungeon_bat_rider_FSM.Ready:
                    if (Target_Locked_On == null)
                    {
                        FSM_change_to(EN_dungeon_bat_rider_FSM.Default);
                        break;
                    }

                    var delta_x = Get_Target_Pos().Value.x - cell.pos.x;
                    var delta_x_abs = Mathf.Abs(delta_x);
                    if (delta_x_abs >= SHOOT_DISTANCE_MIN && delta_x_abs <= SHOOT_DISTANCE_MAX)
                        FSM_change_to(EN_dungeon_bat_rider_FSM.Shoot);
                    else
                        cell.position_expt.x = Get_Target_Pos().Value.x + (delta_x >= 0 ? -SHOOT_DISTANCE_MIN : SHOOT_DISTANCE_MIN);

                    cell.position_expt.y = Mathf.Min(cell.position_expt.y, Get_Target_Pos().Value.y + SHOOT_DY_MIN);
                    cell.dir.x = delta_x;

                    break;


                case EN_dungeon_bat_rider_FSM.Shoot:
                    Ticks_In_Current_State++;

                    if (Check_State_Time(SHOOT_END_TICK_BY_SPINE))
                        Target_Locked_On = null;

                    if (Target_Locked_On == null)
                    {
                        FSM_change_to(EN_dungeon_bat_rider_FSM.Default);
                        cell.dir.x = cell.velocity.x;
                        break;
                    }

                    var tpx = Get_Target_Pos().Value.x;
                    var dx = tpx - cell.pos.x;

                    if (Mathf.Abs(dx) >= SHOOT_DISTANCE_MIN)
                        cell.position_expt.x = cell.pos.x + (Get_Target_Vel().Value.x > cell.velocity.x ? SHOOT_DISTANCE_MIN : -SHOOT_DISTANCE_MIN);
                    else
                        cell.position_expt.x = tpx + (dx >= 0 ? -SHOOT_DISTANCE_MIN : SHOOT_DISTANCE_MIN);

                    if (Check_State_Time(SHOOT_ATK_TICK_BY_SPINE) && !Shoot_Finished)
                        I_Shoot.Monster_Shoot(cell, MUZZLE_BONE_NAME_IN_SPINE, Get_Target_Pos().Value, Get_Target_Vel().Value.x);
                    cell.dir.x = dx;

                    break;

                default:
                    break;
            }

            mover.move();
        }


        void FSM_change_to(EN_dungeon_bat_rider_FSM expected_fsm)
        {
            m_state = expected_fsm;
            Ticks_In_Current_State = 0;
            switch (expected_fsm)
            {
                case EN_dungeon_bat_rider_FSM.Shoot:
                    Shoot_Finished = false;
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

