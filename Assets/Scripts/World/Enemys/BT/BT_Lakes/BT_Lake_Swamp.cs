using Commons;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Lake_Swamp;

namespace World.Enemys.BT
{
    public class BT_Lake_Swamp : Enemy_BT, IEnemy_BT_Attack_Delay<EN_FSM>, IEnemy_BT_Shoot
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase,
            Shoot
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_walk_speed;
        float e_atk_dis;

        Vector2 e_shoot_cd;
        int m_shoot_cd;

        int e_running_fire_count;

        float e_before_attack_sec;
        float e_after_attack_sec;
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
            e_walk_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "WALK_SPEED"));
            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_DIS"));

            e_shoot_cd = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "SHOOT_CD");
            e_running_fire_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "RUNNING_FIRE_COUNT"));

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Chase;

            cell.mover.move_type = EN_enemy_move_type.Walk;

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
            SeekTarget_Helper.random_player_part(out cell.target);
        }


        public void do_Chase(Enemy cell)
        {
            cell.position_expt = cell.target.Position;
            cell.speed_expt = e_walk_speed;

            m_shoot_cd = Mathf.Max(0, --m_shoot_cd);
            
            if (Mathf.Abs(cell.pos.x - cell.target.Position.x) <= e_atk_dis)
            {
                cell.speed_expt = 0f;

                if (m_shoot_cd <= 0)
                    m_state = EN_FSM.Shoot;
            }    
        }
        #endregion


        #region Shoot
        public void start_Shoot(Enemy cell)
        {
            i_attack_delay.@do(cell, shoot);

            #region 子函数 shoot
            void shoot()
            {
                for (int i = 0; i < e_running_fire_count; i++)
                {
                    i_shoot.do_shoot(cell, cell.target.Position);
                }
            }
            #endregion
        }


        public void end_Shoot(Enemy cell)
        {
            m_shoot_cd = Mathf.RoundToInt(Random.Range(e_shoot_cd.x, e_shoot_cd.y) * Config.PHYSICS_TICKS_PER_SECOND);
        }
        #endregion


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }
    }
}

