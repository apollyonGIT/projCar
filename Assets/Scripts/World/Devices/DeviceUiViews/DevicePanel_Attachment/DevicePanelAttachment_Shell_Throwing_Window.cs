using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 子弹抛壳窗。
    /// 可以通过Set_Ammo_View()来设置抛壳窗中弹药的显示状态。
    /// </summary>
    public class DevicePanelAttachment_Shell_Throwing_Window : DevicePanelAttachment
    {
        public Image img_ammo_in_window;
        public Sprite bullet, shell;
        public SubAttachment_Throwing_Shell shell_throwing;

        // -------------------------------------------------------------------------------

        private List<SubAttachment_Throwing_Shell> shells = new();
        private int ammo_in_window_state = 0; // 0 = 不会有弹药，1 = 空，2 = 有弹药，3 = 有弹壳

        // ===============================================================================

        protected override void Init_Actions(List<Action> action = null)
        {

        }

        // ===============================================================================

        public void Update_Window_Ammo_View(int state)
        {
            img_ammo_in_window.gameObject.SetActive(state >= 2);
            switch (state)
            {
                case 3:
                    img_ammo_in_window.sprite = shell;
                    break;
                case 2:
                    img_ammo_in_window.sprite = bullet;
                    break;
                default:
                    break;
            }
            ammo_in_window_state = state;
        }

        public void New_Bullet_Loaded(int state)
        {
            switch (state)
            {
                case 3:
                    throw_shell(shell);
                    break;
                case 2:
                    throw_shell(bullet);
                    break;
                default:
                    break;
            }
        }

        public bool Has_Alive_Throwing_Shell()
        {
            return shells.Count > 0;
        }

        public void Per_Tick_For_Throwing_Shells()
        {
            for (int i = shells.Count - 1; i >= 0; i--)
            {
                var shell = shells[i];
                if (shell.isDead())
                {
                    Destroy(shell.gameObject);
                    shells.RemoveAt(i);
                    continue;
                }
                shell.tick();
            }
        }

        // ===============================================================================

        private void throw_shell(Sprite sprite)
        {
            var shell = Instantiate(shell_throwing, GetComponent<RectTransform>(), false);
            shell.Init(sprite, 240, 2f);//img_ammo_in_window.rectTransform.anchoredPosition);
            shell.gameObject.SetActive(true);
            shells.Add(shell);
        }
    }
}