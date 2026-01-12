using AutoCodes;
using Commons;
using Foundations.Excels;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;
using World.Devices.Device_AI;
using World.Helpers;

namespace World.Progresss
{
    public class ProgressEvent
    {
        public float trigger_progress;
        public bool notice_triggered = false;
        public bool notice_arrived = false;

        public bool need_remove = false;

        public event_site record;

        public ProgressModule module;
        public int current_value = 0 ;
        public int max_value = 100;

        public Vector2 pos;

        public string cd_TX => launch_cd == 0 ? "" : $"{Mathf.CeilToInt((float)(launch_cd) / Config.PHYSICS_TICKS_PER_SECOND)}秒后获得{launch_loot_name}";
        public int launch_cd = 0;
        public string launch_loot_name = "";
        public Request lanuch_req;


        public ProgressEvent()
        { 
        }


        public ProgressEvent(float v,event_site record,Vector2 pos)
        {
            trigger_progress = v;
            this.record = record;
            this.pos = pos;
        }

        public void Enter()
        {
            notice_triggered = true;

            switch (record.site_type.value)
            {
                case Site_Type.EN_Site_Type.Monster:
                    //规则：指定范围内选取relic，考虑权重
                    var relic_raw_dic = record.monster_relic_list;
                    List<uint> relic_list = new();

                    foreach (var (_relic_id, count) in relic_raw_dic)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            //规则：剔除不满足条件的relic
                            if (!Drop_Relic_Helper.CheckRelic(_relic_id)) continue;

                            relic_list.Add(_relic_id);
                        }
                    }

                    var index = Random.Range(0, relic_list.Count);
                    var relic_id = relic_list[index];                    

                    Special_Enemy_Helper.add_enemy_car_by_encounter(relic_id);

                    break;
            }
        }

        Vector2 m_velocity_min = new(0, 0);
        Vector2 m_velocity_max = new(3, 3);

        public void Arrived()
        {
            notice_arrived = true;

            switch (record.site_type.value)
            {
                case Site_Type.EN_Site_Type.Monster:
                    break;
                case Site_Type.EN_Site_Type.Loot:
                    
                    //for(int i = 0; i < record.loot_list.Item2;i++)
                    //{
                    //    Vector2 velocity = new(Random.Range(m_velocity_min.x, m_velocity_max.x), Random.Range(m_velocity_min.y, m_velocity_max.y));
                    //    Drop_Loot_Helper.drop_loot(record.loot_list.Item1, new Vector2(WorldContext.instance.caravan_pos.x, 6), velocity);
                    //}
                    break;
                default:
                    break;
            }
        }

        public void tick()
        {
            if (current_value >= max_value)
            {
                CompleteJob();
                current_value = 0;
            }

            module.tick();
        }

        private void CompleteJob()
        {
            switch (record.site_type.value) 
            {
                case Site_Type.EN_Site_Type.Dialog:
                    Encounters.Dialogs.Encounter_Dialog.instance.init(this, "Dialog_Window");
                    break;
                case Site_Type.EN_Site_Type.Monster:
                    break;
                case Site_Type.EN_Site_Type.Loot:
                    break;
                default:
                    break;
            }
            
        }

        public void Exit()
        {
            need_remove = true;

            switch (record.site_type.value)
            {
                case Site_Type.EN_Site_Type.Dialog:
                    Encounters.Dialogs.Encounter_Dialog.instance.fini(null);
                    break;
                case Site_Type.EN_Site_Type.Monster:
                    break;
                case Site_Type.EN_Site_Type.Loot:
                    break;
                default:
                    break;
            }
        }
    }
}
