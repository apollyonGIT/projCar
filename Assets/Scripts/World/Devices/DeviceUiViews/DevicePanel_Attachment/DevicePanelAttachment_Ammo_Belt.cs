using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 弹链式弹药指示器。
    /// 支持显示多个弹链，需要手动配置各个弹链的“视觉数量”。
    /// 可以挂载在Prefab任意节点下。
    /// </summary>
    public class DevicePanelAttachment_Ammo_Belt : DevicePanelAttachment
    {
        public List<Image> ammo_belts;
        public List<int> ammo_belt_capacity;

        // -------------------------------------------------------------------------------

        private int ammo_visual_num_total;
        private int ammo_num_last_check;
        private bool multiple_belt;
        private int current_ammo_belt_showed;

        // ===============================================================================

        public override void Init(List<Action> action = null)
        {
            ammo_visual_num_total = ammo_belt_capacity.Sum();
            ammo_num_last_check = ammo_visual_num_total;
            multiple_belt = ammo_belts.Count > 1;
        }

        protected override void Init_Actions(List<Action> action)
        {
            
        }   

        // ===============================================================================

        /// <summary>
        /// 需要每帧持续调用这一函数，更新弹药指示器的显示。
        /// </summary>
        /// <param name="shooter_current_ammo"></param>
        /// <param name="shooter_max_ammo"></param>
        /// <returns>返回值：当前显示第i个弹夹</returns>
        public int Update_Ammo_Belt_View(int shooter_current_ammo, int shooter_max_ammo)
        {
            if (ammo_num_last_check == shooter_current_ammo)
                return current_ammo_belt_showed;

            ammo_num_last_check = shooter_current_ammo;

            if (multiple_belt)
            {
                int previous_clip_visual_count = 0;
                current_ammo_belt_showed = 0;
                for (int i = 0; i < ammo_belts.Count; i++)
                {
                    if (update_view(i, previous_clip_visual_count, shooter_current_ammo, shooter_max_ammo))
                        current_ammo_belt_showed++;
                    previous_clip_visual_count += ammo_belt_capacity[i];
                }
                current_ammo_belt_showed--;
            }
            else
            {
                update_view(shooter_current_ammo, shooter_max_ammo);
                current_ammo_belt_showed = 0;
            }
            return current_ammo_belt_showed;
        }

        // ===============================================================================

        private bool update_view(int ammo_belt_index, int previous_clip_visual_count, int shooter_current_ammo, int shooter_max_ammo)
        {
            var total_ammo_to_show = shooter_current_ammo * ammo_visual_num_total / shooter_max_ammo;
            var ammo_to_show = total_ammo_to_show - previous_clip_visual_count;
            ammo_belts[ammo_belt_index].fillAmount = (float)ammo_to_show / ammo_belt_capacity[ammo_belt_index];
            return ammo_to_show >= 0;
        }

        private void update_view(int shooter_current_ammo, int shooter_max_ammo)
        {
            ammo_belts[0].fillAmount = shooter_current_ammo * ammo_belt_capacity[0] / shooter_max_ammo * (1f / ammo_belt_capacity[0]);
        }
    }
}
