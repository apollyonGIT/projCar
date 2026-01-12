using UnityEngine;

namespace World.Devices.Device_AI
{
    public class DScript_Melee_Claymore : DScript_Melee,ISharpNew
    {
        public float sharp_process;

        public float sharp_job_process = 0;


        #region 外部调用函数

        public void Try_Charging()
        {
            DeviceBehavior_Melee_Try_Charge();
        }

        public void Try_Sharping(float percent)
        {
            if(percent == -1)
            {
                sharp_process = 0;
                return;
            }

            sharp_process = Mathf.Max(sharp_process,percent);

            if(sharp_process >=1)
                DeviceBehavior_Melee_Sharpen(sharp_process);
        }

        public float Get_Sharpness_Dmg_Coef()
        {
            return get_sharpness_modified_dmg_coef_by_stage(get_sharpness_stage());
        }

        public float Get_Sharpness()
        {
            return sharpness_current;
        }

        public void TryToAutoSharp()
        {
            if (fsm == Device_FSM_Melee.broken || sharpness_current >= 0.6f)
            {
                return;
            }
            if (sharp_job_process < 1)
            {
                sharp_job_process += melee_logic_data.job_speed[1];
                return;
            }

            if (Auto_Sharp_Job_Content())
            {
                sharp_job_process = 0;
            }
        }

        protected bool Auto_Sharp_Job_Content()
        {
            sharp_process += 0.02f;
            sharp_process = Mathf.Min(sharp_process, 1f);
            return true;
        }


        public override void TryToAutoAttack()
        {
            if (charge_process >= 1)
                DeviceBehavior_Melee_Try_Attack(false,"attack_1",null);
            else
            {
                DeviceBehavior_Melee_Try_Charge();
            }
        }

        #endregion
    }
}
