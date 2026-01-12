using AutoCodes;
using Commons;
using UnityEngine;
using World.Caravans;

namespace World.Devices.Device_AI
{
    public class JetWheel :BasicWheel,IEnergy,IRotate
    {
        public float jet_angle;
        public float Default_Energy { get; set; }
        public float Current_Energy { get; set; }
        public float Max_Energy { get; set; }
        public float Energy_Recover { get; set; }
        public float Energy_Recover_Factor { get; set; }
        public int Energy_FreezeTick_Current { get; set; }
        public int Energy_FreezeTick_Max { get; set; }

        public float turn_angle => jet_angle;


        private const float single_acceleration_consume = 30f;


        public void TryToAccelerate()
        {
            
        }

        public override void tick()
        {
            base.tick();


            Current_Energy += 10 * Config.PHYSICS_TICK_DELTA_TIME;
            Current_Energy = Mathf.Min(Current_Energy, Max_Energy);
        }

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            Max_Energy = 100;
            Default_Energy = Max_Energy;
            Current_Energy = Max_Energy;
        }

        public void Accelerate()
        {

            Vector2 force = Vector2.zero;
            var jet_dir = (Quaternion.AngleAxis(jet_angle, Vector3.forward) * Vector2.up).normalized; //计算喷射方向
            if (Current_Energy > single_acceleration_consume)
            {
                Current_Energy -= single_acceleration_consume;
                force = jet_dir * single_acceleration_consume;
            }
            else
            {
                force = jet_dir * Current_Energy;
                Current_Energy = 0;
            }

            force /= 5;
            AddForceToCaravan(force);
        }

        private void AddForceToCaravan(Vector2 power)
        {
            WorldContext.instance.caravan_velocity.x = power.x;
            CaravanMover.do_jump_input_vy(power.y);
        }

        public void Rotate(float angle)
        {
            jet_angle += angle;
        }

        public void TryToAutoRotate()
        {
            
        }
    }
}
