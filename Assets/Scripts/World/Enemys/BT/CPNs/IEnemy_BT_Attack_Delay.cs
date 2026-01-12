using Commons;
using Foundations.Tickers;
using System;
using UnityEngine;

namespace World.Enemys.BT
{
    public interface IEnemy_BT_Attack_Delay<T> where T : Enum
    {
        float before_attack_sec { get; }

        float after_attack_sec { get; }

        public T state { get; set; }
        public T charge_next_state { get; }

        //==================================================================================================

        void @do(Enemy cell, Action hurt_ac, int time_count = 1, bool is_jump_state = true)
        {
            Request_Helper.delay_do($"start_attack_{cell.GUID}", Mathf.FloorToInt(before_attack_sec * time_count * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => {
                    hurt_ac?.Invoke();

                    Request_Helper.delay_do($"end_attack_{cell.GUID}",
                    Mathf.FloorToInt(after_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                    (_) => {
                        if (is_jump_state)
                            state = charge_next_state; 
                    });
                });
        }


        void @do(Enemy cell, Action hurt_ac, Action end_ac)
        {
            Request_Helper.delay_do($"start_attack_{cell.GUID}", Mathf.FloorToInt(before_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => {
                    hurt_ac?.Invoke();

                    Request_Helper.delay_do($"end_attack_{cell.GUID}",
                    Mathf.FloorToInt(after_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                    (_) => {
                        end_ac?.Invoke();
                    });
                });
        }


        void do_first(Enemy cell, Action hurt_ac)
        {
            Request_Helper.delay_do($"start_attack_{cell.GUID}_1", Mathf.FloorToInt(before_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => {
                    hurt_ac?.Invoke();
                });
        }


        void do_for(Enemy cell, Action hurt_ac, int time_count)
        {
            Request_Helper.delay_do($"start_attack_{cell.GUID}_{time_count}", Mathf.FloorToInt((before_attack_sec + after_attack_sec) * time_count * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => {
                    hurt_ac?.Invoke();
                });
        }


        void do_end(Enemy cell, Action hurt_ac, int time_count, Action end_ac)
        {
            Request_Helper.delay_do($"start_attack_{cell.GUID}_{time_count}", Mathf.FloorToInt((before_attack_sec + after_attack_sec) * time_count * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => {
                    hurt_ac?.Invoke();

                    Request_Helper.delay_do($"end_attack_{cell.GUID}",
                    Mathf.FloorToInt(after_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                    (_) => {
                        end_ac?.Invoke();
                    });
                });
        }
    }
}

