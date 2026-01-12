using AutoCodes;
using System.Collections.Generic;
using System.Linq;

namespace World.Relic
{
    public class RelicMuseum
    {
        public Dictionary<uint, RelicStatus> relic_dic = new();

        public class RelicStatus
        {
            public int remain_cnt;
            public bool lock_state;
            public relic relic_info;
        }


        public void Init()
        {
            foreach (var single_rec in  relics.records)
            {
                var relic = single_rec.Value;

                var relic_struct = new RelicStatus
                {
                    remain_cnt = relic.max_num,
                    lock_state = !relic.unlocked_init, // 初始状态是否解锁
                    relic_info = relic
                };

                relic_dic.Add(relic.id,relic_struct);
            }
        }
        /// <summary>
        /// 存读档的时候用来更新relic状态 无视状态 直接获得 并且记录对解锁方面的影响
        /// </summary>
        /// <param name="relic_id"></param>
        public void GetRelic(uint relic_id)
        {
            relic_dic.TryGetValue(relic_id, out var relic_struct);
            if (relic_struct.relic_info != null && relic_struct.remain_cnt > 0 ) //此处无视解锁条件
            {
                relic_struct.remain_cnt--;

                if (relic_struct.relic_info.unlock_relic != null)
                {
                    foreach (var unlock_relic in relic_struct.relic_info.unlock_relic)
                    {
                        if (relic_dic.TryGetValue(unlock_relic, out var unlock_struct))
                        {
                            unlock_struct.lock_state = false; // 解锁
                            relic_dic[unlock_relic] = unlock_struct; // 更新字典
                        }
                    }
                }

                if (relic_struct.relic_info.lock_relic != null)
                {
                    foreach (var lock_relic in relic_struct.relic_info.lock_relic)
                    {
                        if (relic_dic.TryGetValue(lock_relic, out var lock_struct))
                        {
                            lock_struct.lock_state = true; // 锁定
                            relic_dic[lock_relic] = lock_struct; // 更新字典
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 只记录获取的这个遗物后 对解锁方面的影响，不关心记录遗物的获取
        /// </summary>
        /// <param name="relic_id"></param>
        public bool TryGetRelic(uint relic_id) {
            relic_dic.TryGetValue(relic_id, out var relic_struct);
            if(relic_struct.relic_info != null && relic_struct.remain_cnt > 0 && relic_struct.lock_state == false)
            {
                relic_struct.remain_cnt--;

                if(relic_struct.relic_info.unlock_relic != null)
                {
                    foreach (var unlock_relic in relic_struct.relic_info.unlock_relic)
                    {
                        if (relic_dic.TryGetValue(unlock_relic, out var unlock_struct))
                        {
                            unlock_struct.lock_state = false; // 解锁
                            relic_dic[unlock_relic] = unlock_struct; // 更新字典
                        }
                    }
                }

                if (relic_struct.relic_info.lock_relic != null)
                {
                    foreach (var lock_relic in relic_struct.relic_info.lock_relic)
                    {
                        if (relic_dic.TryGetValue(lock_relic, out var lock_struct))
                        {
                            lock_struct.lock_state = true; // 锁定
                            relic_dic[lock_relic] = lock_struct; // 更新字典
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public bool CheckRelic(uint relic_id)
        {
            return relic_dic.TryGetValue(relic_id, out var relic_struct) &&
                   relic_struct.relic_info != null &&
                   relic_struct.remain_cnt > 0 &&
                   !relic_struct.lock_state;
        }
    }
}
