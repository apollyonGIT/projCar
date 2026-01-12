using Commons;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Lake_Bat;

namespace World.Enemys.BT
{
    public class BT_Lake_Bat : Enemy_BT, IEnemy_BT_FlyAround, IEnemy_BT_Charge<EN_FSM>
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Move,
            Charge,
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        const float C_fly_speed_idle = 5f;
        const float C_fly_speed_brust = 16f;
        const float C_fly_speed_max = 30f;

        bool m_can_bite;
        #endregion

        #region FlyAround
        Vector2 IEnemy_BT_FlyAround.flyAround_deg => new(10f, 170f);

        Vector2 IEnemy_BT_FlyAround.flyAround_radius => new(5f, 12f);

        Vector2 IEnemy_BT_FlyAround.flyAround_pos { get => m_flyAround_pos; set => m_flyAround_pos = value; }
        Vector2 m_flyAround_pos;

        int IEnemy_BT_FlyAround.flyAround_count { get => m_flyAround_count; set => m_flyAround_count = value; }
        int m_flyAround_count;

        IEnemy_BT_FlyAround i_flyAround => this;
        #endregion

        #region Charge
        public float charge_dis_min => 3f;
        public float charge_dis_max => 11f;
        public float charge_tangent_limit => 0.5f;

        public int charge_cd_min => 7 * Config.PHYSICS_TICKS_PER_SECOND;
        public int charge_cd_max => 12 * Config.PHYSICS_TICKS_PER_SECOND;
        public int charge_lasting_tick => (int)(1.5f * Config.PHYSICS_TICKS_PER_SECOND);

        public int charge_cd { get => m_charge_cd; set => m_charge_cd = value; }
        int m_charge_cd;

        public bool charge_dir { get => m_charge_dir; set => m_charge_dir = value; }
        bool m_charge_dir;

        EN_FSM IEnemy_BT_Charge<EN_FSM>.state { get => m_state; set => m_state = value; }
        public EN_FSM charge_next_state => EN_FSM.Move;
        public EN_FSM charge_state => EN_FSM.Charge;

        IEnemy_BT_Charge<EN_FSM> i_charge => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Move;

            cell.mover.move_type = EN_enemy_move_type.Fly;

            SeekTarget_Helper.random_player_part(out var target);
            cell.target = target;

            m_charge_cd = Random.Range(charge_cd_min, charge_cd_max);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            cell.hp = 1000;

            base.tick(cell);
        }


        #region Move
        public void start_Move(Enemy cell)
        {
            i_flyAround.calc_flyAround_pos();
        }


        public void do_Move(Enemy cell)
        {
            Enemy_BT_Speed_CPN.set_chasing_speed(cell, C_fly_speed_idle, C_fly_speed_max);

            i_flyAround.flyAround_on_tick(cell, cell.target.Position);
            i_charge.try_charge(cell);
        }
        #endregion


        #region Charge
        public void start_Charge(Enemy cell)
        {
            i_charge.start_charge(cell);

            m_can_bite = true;
        }


        public void do_Charge(Enemy cell)
        {
            Enemy_BT_Speed_CPN.set_chasing_speed(cell, C_fly_speed_brust, C_fly_speed_max);

            i_charge.do_charge(cell);

            if (m_can_bite && Vector2.Distance(cell.target.Position, cell.pos) <= 1f)
            {
                Enemy_Hurt_Helper.do_hurt(cell);
                Enemy_BT_Audio_CPN.play_attack_audio(cell);

                m_can_bite = false;
            }
        }


        public void end_Charge(Enemy cell)
        {
            i_charge.end_charge(cell);
        }
        #endregion

    }
}

