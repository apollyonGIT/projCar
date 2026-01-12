using Commons;
using UnityEngine;

namespace World.Enemys.BT
{
    public class BT_dungeon_bat : Enemy_Monster_Basic_BT, IEnemy_BT, IEnemy_Can_Flyaround
    {
        #region CONST
        const float FLYAROUND_DEG_MIN = 10F;
        const float FLYAROUND_DEG_MAX = 170F;
        const float FLYAROUND_RADIUS_MIN = 5F;
        const float FLYAROUND_RADIUS_MAX = 12F;
        const float FLYAROUND_RADIUS_RATIO = 0.15F;

        const int CHASING_STATE_LASTING_TICKS_MAX = 15 * Config.PHYSICS_TICKS_PER_SECOND;

        const ushort CHARGE_STATE_LASTING_TICKS = 180;  //1.5 Sec
        const ushort CHARGE_CD_MIN = 7 * Config.PHYSICS_TICKS_PER_SECOND;
        const ushort CHARGE_CD_MAX = 12 * Config.PHYSICS_TICKS_PER_SECOND;

        const float CHARGE_DISTANCE_MIN = 3f;
        const float CHARGE_DISTANCE_MAX = 11f;
        const float CHARGE_TANGENT_LIMIT = 0.5f;

        const float FLY_SPEED_IDLE = 5F;
        const float FLY_SPEED_BURST = 16F;
        const float FLY_SPEED_MAX = 30F;
        #endregion

        private enum EN_dungeon_bat_FSM
        {
            Default,
            Move,
            Charge,
        }
        EN_dungeon_bat_FSM m_state;
        string IEnemy_BT.state => $"{m_state}";

        private bool charge_to_right;
        private bool can_bite;
        private bool charge_over_distance;

        private ushort charge_cd;

        Enemy self;

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
            self = cell;
            FSM_change_to((EN_dungeon_bat_FSM)System.Enum.Parse(typeof(EN_dungeon_bat_FSM), (string)prms[0]));

            I_Flyaround = this;
            I_Flyaround.Init_Or_Reset_Relative_Following_Pos();
        }


        void IEnemy_BT.tick(Enemy cell)
        {
            var ctx = cell.mgr.ctx;
            var mover = cell.mover;

            // Main FSM
            switch (m_state)
            {
                case EN_dungeon_bat_FSM.Default:
                case EN_dungeon_bat_FSM.Move:
                    if (Target_Locked_On == null)
                        Lock_Target();

                    Set_Chasing_Speed(cell, FLY_SPEED_IDLE, FLY_SPEED_MAX);
                    I_Flyaround.Flyaround_Per_Tick(cell, Forced_Get_Target_Pos());

                    if (++Ticks_In_Current_State > CHASING_STATE_LASTING_TICKS_MAX)
                    {
                        I_Flyaround.Init_Or_Reset_Relative_Following_Pos();
                        Ticks_In_Current_State = 0;
                    }

                    if (Target_Locked_On == null)
                        break;

                    var abs_dis_x = Mathf.Abs(cell.pos.x - Get_Target_Pos().Value.x);

                    if (charge_cd == 0)
                    {
                        if (abs_dis_x > CHARGE_DISTANCE_MIN &&
                            abs_dis_x < CHARGE_DISTANCE_MAX &&
                            cell.pos.y < Get_Target_Pos().Value.y + abs_dis_x * CHARGE_TANGENT_LIMIT)
                            FSM_change_to(EN_dungeon_bat_FSM.Charge);
                    }
                    else
                        charge_cd--;

                    break;

                case EN_dungeon_bat_FSM.Charge:
                    // Back to Move State if Condition Met
                    if (Target_Locked_On == null || Check_State_Time(CHARGE_STATE_LASTING_TICKS))
                    {
                        FSM_change_to(EN_dungeon_bat_FSM.Move);
                        break;
                    }

                    Ticks_In_Current_State++;

                    Set_Chasing_Speed(cell, FLY_SPEED_BURST, FLY_SPEED_MAX);

                    var dis_to_focus_pos = Get_Target_Pos().Value - cell.pos;

                    if (!charge_over_distance)
                    {
                        var pos_y_feedback = -Mathf.Min(0, cell.pos.y - Get_Target_Pos().Value.y - 1.5f);
                        cell.position_expt = 2 * Get_Target_Pos().Value + new Vector2(charge_to_right ? 3f : -3f, pos_y_feedback) - cell.pos;

                        if ((dis_to_focus_pos.x + (charge_to_right ? 1f : -1f) < 0) == charge_to_right)
                        {
                            bat_end_charge_earlier(30);
                            charge_over_distance = true;
                        }
                    }
                    else
                    {
                        cell.position_expt = new Vector2(Get_Target_Pos().Value.x + (charge_to_right ? 1 : -1), 10f);
                    }

                    if (can_bite && dis_to_focus_pos.magnitude < 0.5f)
                    {
                        foreach (var atk_info in cell._desc.diy_atk)
                        {
                            Attack_Data attack_data = new()
                            {
                                atk = atk_info.Value,
                                diy_atk_str = atk_info.Key
                            };

                            Target_Locked_On.hurt(cell, attack_data, out var dmg_data);
                        }
                        
                        Audio.AudioSystem.instance.PlayOneShot(cell._desc.SE_attack);
                        can_bite = false;
                        bat_end_charge_earlier(120);
                    }
                    break;

                default:
                    break;
            }

            //朝向
            cell.dir.x = cell.velocity.x;
            mover.move();
        }

        private void FSM_change_to(EN_dungeon_bat_FSM state)
        {
            m_state = state;
            Ticks_In_Current_State = 0;
            switch (state)
            {
                case EN_dungeon_bat_FSM.Default:
                    self.mover.move_type = EN_enemy_move_type.Fly;
                    charge_cd = (ushort)Random.Range(CHARGE_CD_MIN, CHARGE_CD_MAX);
                    break;
                case EN_dungeon_bat_FSM.Move:
                    charge_cd = (ushort)Random.Range(CHARGE_CD_MIN, CHARGE_CD_MAX);
                    break;
                case EN_dungeon_bat_FSM.Charge:
                    charge_to_right = Get_Target_Pos().Value.x >= self.pos.x;
                    can_bite = true;
                    charge_over_distance = false;
                    break;
                default:
                    break;
            }
        }

        private void bat_end_charge_earlier(int ticks_earlier)
        {
            Ticks_In_Current_State = (ushort)Mathf.Max(CHARGE_STATE_LASTING_TICKS - ticks_earlier, Ticks_In_Current_State);
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

