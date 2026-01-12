using AutoCodes;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class FireWheel:BasicWheel, IAttack_Device
    {
        private const string COLLIDER_1 = "collider_1";
        private const string KEY_POINT_1 = "collider_1";

        private const string FIRECOLLIDER = "fire_collider";

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
        }

        private bool fired = false;
        public override void tick()
        {
            base.tick();
            int dmg_interval = 0;
            if (fired)
            {
                dmg_interval--;
            }


            if(WorldContext.instance.caravan_velocity.x >= 4 && fired == false)
            {
                fired = true;
                OpenCollider(FIRECOLLIDER, null, stay_collider);
                #region stay_collider
                void  stay_collider(ITarget t)
                {
                    if (dmg_interval <= 0)
                    {
                        int final_dmg = 1;
                        Attack_Data attack_data = new()
                        {
                            atk = final_dmg,
                            critical_chance = 0,
                            critical_dmg_rate = 0,
                        };

                        attack_data.calc_device_coef(this);

                        t.hurt(this, attack_data, out var dmg_data);
                        BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                        if (t.hp <= 0)
                        {
                            kill_enemy_action?.Invoke(t);
                        }
                    }

                }
                #endregion 
            }
            else if(WorldContext.instance.caravan_velocity.x <4 && fired)
            {
                fired = false;
                CloseCollider(FIRECOLLIDER);
            }
        }
    }
}
