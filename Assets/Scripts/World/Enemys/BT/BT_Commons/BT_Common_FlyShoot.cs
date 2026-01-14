using Foundations.Tickers;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Common_FlyShoot;

namespace World.Enemys.BT
{
    public class BT_Common_FlyShoot : Enemy_BT, IEnemy_BT_FlyAround, IEnemy_BT_FlyFollow, IEnemy_BT_Attack_Delay<EN_FSM>, IEnemy_BT_Shoot
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase,
            Hover,
            Attack,
            AttackAfter
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_before_attack_sec;
        float e_after_attack_sec;

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

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");

            e_flyAround_count_max = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX"));
            e_flyAround_count_min = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN"));

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_running_fire_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "RUNNING_FIRE_COUNT"));
            #endregion

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
                m_state = EN_FSM.Attack;
            }
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            if (m_is_refresh_follow_pos)
            {
                m_flyFollow_pos = cell.pos - cell.target.Position;
                m_is_refresh_follow_pos = false;
            }

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

            m_state = EN_FSM.Chase;
        }
        #endregion


        #region AttackAfter
        public void do_AttackAfter(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);

            m_state = EN_FSM.Attack;
        }
        #endregion


        public override void notify_on_set_face_dir(Enemy cell)
        {
            if (m_state == EN_FSM.Chase)
            {
                base.notify_on_set_face_dir(cell);
                return;
            }

            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }
    }
}


