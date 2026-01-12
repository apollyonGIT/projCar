using UnityEngine;

namespace World.Devices.DeviceUiViews
{
    public class DevicePanelAttachment_Trigger_Press_Anim : DevicePanelAttachment_Trigger_Press
    {
        public Animator animator;
        public void SetAnim(float normalizedTime)
        {
            animator.enabled = true;
            animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, normalizedTime);
        }
    }
}
