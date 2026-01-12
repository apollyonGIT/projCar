using System.Collections.Generic;

namespace Foundations
{
    public interface IMgr
    {
        public string name { get; }
        public int priority { get; }
        void init(params object[] args);
        void fini();
    }


    public class Mission : Singleton<Mission>
    {
        Dictionary<string, IMgr> m_mgrs_dic = new();
        public Dictionary<string, IMgr> mgr_dic => m_mgrs_dic;

        //==================================================================================================

        public void attach_mgr(string name, IMgr mgr)
        {
            EX_Utility.dic_cover_add(ref m_mgrs_dic, name, mgr);
        }


        public void detach_mgr(string name)
        {
            EX_Utility.dic_safe_remove(ref m_mgrs_dic, name);
        }


        public bool try_get_mgr(string name, out IMgr mgr)
        {
            return m_mgrs_dic.TryGetValue(name, out mgr);
        }


        public bool try_get_mgr<T>(string name, out T mgr) where T : class, IMgr
        {
            mgr = null;
            if (!try_get_mgr(name, out var imgr)) return false;

            mgr = imgr as T;
            return true;
        }


        public void remove_all_mgr()
        {
            m_mgrs_dic.Clear();
        }
    }
}

