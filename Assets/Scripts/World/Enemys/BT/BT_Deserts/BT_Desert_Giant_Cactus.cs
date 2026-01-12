using Commons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Desert_Giant_Cactus;

namespace World.Enemys.BT
{
    public class BT_Desert_Giant_Cactus : Enemy_BT, IEnemy_BT_Attack_Delay<EN_FSM>, IEnemy_BT_Shoot
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Attack
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_before_attack_sec;
        float e_after_attack_sec;

        int e_atk_cd;
        int m_atk_cd;

        int e_running_fire_count;
        #endregion

        #region Attack_Delay
        float IEnemy_BT_Attack_Delay<EN_FSM>.before_attack_sec => e_before_attack_sec;
        float IEnemy_BT_Attack_Delay<EN_FSM>.after_attack_sec => e_after_attack_sec;

        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.state { get => m_state; set => m_state = value; }
        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.charge_next_state => EN_FSM.Idle;

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
            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_atk_cd = Mathf.RoundToInt(float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_CD")) * Config.PHYSICS_TICKS_PER_SECOND);
            e_running_fire_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "RUNNING_FIRE_COUNT"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.None;

            i_shoot.init_data(cell);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            m_atk_cd++;
            m_atk_cd = Mathf.Min(m_atk_cd, e_atk_cd);

            base.tick(cell);
        }


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }


        #region Idle
        public void do_Idle(Enemy cell)
        {
            if (m_atk_cd == e_atk_cd)
            {
                m_state = EN_FSM.Attack;
            }
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            SeekTarget_Helper.random_player_part(out cell.target);

            i_attack_delay.@do(cell, shoot, attack_delay_callback);

            #region 子函数 shoot
            void shoot()
            {
                var delta_deg = 180f / (e_running_fire_count - 1);
                cell.try_get_bone_pos(i_shoot.muzzle_bone_name, out Vector2 muzzle_pos);

                for (int i = 0; i < e_running_fire_count; i++)
                {
                    var rad = delta_deg * i * Mathf.Deg2Rad;
                    i_shoot.do_shoot(cell, muzzle_pos + new Vector2(10 * Mathf.Cos(rad), 10 * Mathf.Sin(rad)));
                }
            }
            #endregion

            #region 子函数 attack_delay_callback
            void attack_delay_callback()
            {
                m_atk_cd = 0;
                m_state = EN_FSM.Idle;
            }
            #endregion
        }
        #endregion
    }
}