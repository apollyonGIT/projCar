using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Lake_Tangler;

namespace World.Enemys.BT
{
    public class BT_Lake_Tangler : Enemy_BT, IEnemy_BT_FlyAround, IEnemy_BT_FlyFollow, IEnemy_BT_Attack_Delay<EN_FSM>, IEnemy_BT_Shoot, IEnemy_BT_Flee<EN_FSM>
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase,
            Hover,
            Attack,
            AttackAfter,
            Flee
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region Outter
        public BT_Lake_GiantJelly owner_bt;
        public Enemy owner;
        #endregion

        #region PRM
        float e_cd;
        float e_before_attack_sec;
        float e_after_attack_sec;

        int e_hp_clip_count;
        int m_remain_hp_valid_line;

        bool m_is_hp_clip_down;

        Request m_temp_req;

        int e_running_fire_count;
        int m_running_fire_count;

        bool m_is_refresh_follow_pos;
        #endregion

        #region FlyAround
        Vector2 IEnemy_BT_FlyAround.flyAround_deg => e_flyAround_deg;

        Vector2 IEnemy_BT_FlyAround.flyAround_radius => e_flyAround_radius;

        

        Vector2 IEnemy_BT_FlyAround.flyAround_pos { get => m_flyAround_pos; set => m_flyAround_pos = value; }
        Vector2 m_flyAround_pos;

        int IEnemy_BT_FlyAround.flyAround_count { get => m_flyAround_count; set => m_flyAround_count = value; }
        int m_flyAround_count;

        IEnemy_BT_FlyAround i_flyAround => this;

        Vector2 e_flyAround_deg;
        Vector2 e_flyAround_radius;
        

        int e_flyAround_count_max;
        int e_flyAround_count_min;
        int m_flyAround_count_limit;
        #endregion

        #region FlyFollow
        Vector2 IEnemy_BT_FlyFollow.flyFollow_pos => m_flyFollow_pos;
        Vector2 m_flyFollow_pos;

        IEnemy_BT_FlyFollow i_flyFollow => this;
        #endregion

        #region Attack_Delay
        float IEnemy_BT_Attack_Delay<EN_FSM>.before_attack_sec => e_before_attack_sec;
        float IEnemy_BT_Attack_Delay<EN_FSM>.after_attack_sec => e_after_attack_sec;

        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.state { get => m_state; set => m_state = value; }
        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.charge_next_state => EN_FSM.Chase;

        IEnemy_BT_Attack_Delay<EN_FSM> i_attack_delay => this;
        #endregion

        #region Shoot
        string IEnemy_BT_Shoot.muzzle_bone_name => "proj_muzzle";

        int IEnemy_BT_Shoot.shoot_cd_max { get => m_shoot_cd_max; set => m_shoot_cd_max = value; }
        int m_shoot_cd_max;

        Vector2 IEnemy_BT_Shoot.projectile_speed_range { get => m_projectile_speed_range; set => m_projectile_speed_range = value; }
        Vector2 m_projectile_speed_range;

        IEnemy_BT_Shoot i_shoot => this;
        #endregion

        #region Flee
        EN_FSM IEnemy_BT_Flee<EN_FSM>.state { get => m_state; set => m_state = value; }

        IEnemy_BT_Flee<EN_FSM> i_flee => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");
            

            e_flyAround_count_max = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX"));
            e_flyAround_count_min = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN"));

            e_cd = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "CD"));
            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_running_fire_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "RUNNING_FIRE_COUNT"));

            e_hp_clip_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "HP_CLIP_COUNT"));
            #endregion

            m_remain_hp_valid_line = Mathf.RoundToInt(cell.hp_max * (1 - 1 / (float)e_hp_clip_count));

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Chase;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            i_shoot.init_data(cell);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        bool valid_hp_clip_down(Enemy cell)
        {
            if (cell.hp > m_remain_hp_valid_line) return false;

            m_is_hp_clip_down = true;

            while (cell.hp <= m_remain_hp_valid_line)
            {
                m_remain_hp_valid_line = Mathf.RoundToInt(cell.hp - cell.hp_max / (float)e_hp_clip_count);
            }

            return true;
        }


        #region Chase
        public void start_Chase(Enemy cell)
        {
            SeekTarget_Helper.select_caravan(out cell.target);

            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Chase(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, cell.target.Position);

            if (m_flyAround_count >= m_flyAround_count_limit)
            {
                m_flyAround_count = 0;
                m_is_refresh_follow_pos = true;
                m_state = EN_FSM.Hover;
            }
        }
        #endregion


        #region Hover
        public void start_Hover(Enemy cell)
        {
            if (m_is_refresh_follow_pos)
            {
                m_flyFollow_pos = cell.pos - cell.target.Position;
                m_is_refresh_follow_pos = false;
            }    

            m_temp_req = Request_Helper.delay_do($"end_hover_{cell.GUID}",
                Mathf.FloorToInt(e_cd * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => { m_state = EN_FSM.Attack; });
        }


        public void do_Hover(Enemy cell)
        {
            if (valid_hp_clip_down(cell))
            {
                m_state = EN_FSM.Chase;
                m_is_hp_clip_down = false;

                m_temp_req?.interrupt();
            }

            i_flyFollow.notify_on_tick(cell, cell.target.Position);
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);

            SeekTarget_Helper.select_caravan(out cell.target);

            if (m_running_fire_count >= e_running_fire_count) return;

            i_attack_delay.@do(cell, shoot, () => { m_state = EN_FSM.AttackAfter; });

            #region 子函数 shoot
            void shoot()
            {
                i_shoot.do_shoot(cell, cell.target.Position);
                m_running_fire_count++;
            }
            #endregion
        }


        public void do_Attack(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);

            if (m_running_fire_count < e_running_fire_count) return;

            m_running_fire_count = 0;
            Request_Helper.del_requests($"end_attack_{cell.GUID}");

            if (valid_hp_clip_down(cell))
            {
                m_state = EN_FSM.Chase;
                m_is_hp_clip_down = false;
            }
            else
            {
                m_state = EN_FSM.Hover;
            }
        }
        #endregion


        #region AttackAfter
        public void do_AttackAfter(Enemy cell)
        {
            m_state = EN_FSM.Attack;
        }
        #endregion


        #region Flee
        public void start_Flee(Enemy cell)
        {
            i_flee.flee_on_start(cell);
        }


        public void do_Flee(Enemy cell)
        {
            i_flee.flee_on_tick(cell);
        }


        public override void notify_on_enter_flee(Enemy cell, string end_state)
        {
            m_temp_req?.interrupt();
            m_temp_req = null;

            i_flee.@do(cell, end_state);
        }
        #endregion


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }
    }
}


