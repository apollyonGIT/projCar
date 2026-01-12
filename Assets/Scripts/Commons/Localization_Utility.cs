
using AutoCodes;

namespace Commons
{
    public static class Localization_Utility 
    {
        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string get_localization(string id)
        {
            table_l10ns.TryGetValue(id, out var value);
            if(value!=null)
                return value.CN;
            return id;
        }


        public static string get_localization_dialog(string id)
        {
            dialog_l10ns.TryGetValue(id, out var value);
            if (value != null)
                return value.CN;
            return id;
        }
    }
}

