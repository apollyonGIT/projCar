using UnityEngine;

namespace World.Devices.DeviceViews
{
    public class DeviceAnimator : MonoBehaviour
    {
        public Animator animator;

        public void tick()
        {
            animator.Update(Commons.Config.PHYSICS_TICK_DELTA_TIME);
        }

        public void SetTrigger(string trigger_name)
        {
            animator.SetTrigger(trigger_name);
        }

        public void SetFloat(string float_name, float value)
        {
            animator.SetFloat(float_name, value);
        }

        public void SetInteger(string int_name, int value)
        {
            animator.SetInteger(int_name, value);
        }

        public void SetBool(string bool_name, bool value)
        {
            animator.SetBool(bool_name, value);
        }
    }
}
