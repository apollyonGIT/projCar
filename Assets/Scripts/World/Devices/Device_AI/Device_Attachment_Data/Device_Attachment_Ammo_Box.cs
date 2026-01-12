using System;


namespace World.Devices.Device_AI
{
    public class Device_Attachment_Ammo_Box : Device_Attachment
    {
        public Device_Attachment_Ammo_Box(int reload_count_max)
        {
            reloadCountRemaining_max = reload_count_max;
            reloadCountRemaining_current = reload_count_max;
        }

        // ===============================================================================

        public enum AmmoBoxStage
        {
            Unopened,
            Open_Left,
            Fully_Open,
            Empty,
        }

        public Action on_reload_banned;

        // -------------------------------------------------------------------------------

        private AmmoBoxStage ammoBoxStage = AmmoBoxStage.Unopened;

        private int reloadCountRemaining_max;
        private int reloadCountRemaining_current;

        /// <summary>
        /// <para> 每次在逻辑打开弹药箱或装填子弹成功后，调用一次。</para>
        /// <para> 不应当在UI中调用，否则会使装填阶段+1 </para>
        /// </summary>
        /// <returns>返回此次装填是否会使弹药得到补充</returns>
        public bool Use_Ammo_Box(bool reload_banned_by_external_condition = true)
        {
            switch (ammoBoxStage)
            {
                case AmmoBoxStage.Unopened:
                    ammoBoxStage = AmmoBoxStage.Open_Left;
                    return false;
                case AmmoBoxStage.Open_Left:
                    ammoBoxStage = AmmoBoxStage.Fully_Open;
                    return false;
                case AmmoBoxStage.Fully_Open:
                    if (reload_banned_by_external_condition)
                    {
                        reloadCountRemaining_current--;
                        if (reloadCountRemaining_current <= 0)
                            ammoBoxStage = AmmoBoxStage.Empty;
                        return true;
                    }
                    on_reload_banned?.Invoke();
                    return false;
                case AmmoBoxStage.Empty:
                    reloadCountRemaining_current = reloadCountRemaining_max;
                    ammoBoxStage = AmmoBoxStage.Unopened; // Reset to unopened when empty
                    return false;
                default:
                    return false;
            }
        }

        public AmmoBoxStage GetAmmoBoxStage(out int reloadCountRemaining)
        {
            reloadCountRemaining = reloadCountRemaining_current;
            return ammoBoxStage;
        }
    }

}