using System.Collections.Generic;

namespace Foundations
{
    public class Share_DS : Singleton<Share_DS>
    {
        Dictionary<string, object> m_obj_dic = new();

        //==================================================================================================

        public void add<T>(string k, T v)
        {
            EX_Utility.dic_cover_add(ref m_obj_dic, k, v);
        }


        public void remove(string k)
        {
            if (m_obj_dic.ContainsKey(k))
                m_obj_dic.Remove(k);
        }


        public bool try_get_value<T>(string k, out T v)
        {
            v = default;

            var ret = m_obj_dic.TryGetValue(k, out var obj);
            if (obj is not T || !ret) return false;

            v = (T)obj;
            return true;
        }
    }
}

