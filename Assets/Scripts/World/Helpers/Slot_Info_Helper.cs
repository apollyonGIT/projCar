using Commons;
using World.Caravans;
using Foundations;

namespace World.Helpers
{
    public class Slot_Info_Helper
    {
        static CaravanMgr m_caravanmgr;
        static CaravanMgr caravanmgr
        {
            get
            {
                if (m_caravanmgr == null)
                {
                    Mission.instance.try_get_mgr(Config.CaravanMgr_Name, out CaravanMgr cmgr);
                    m_caravanmgr = cmgr;
                }
                return caravanmgr;
            }
        }

        //===========================================================================================================

        public static Caravan TryGetCaravan()
        {
            return m_caravanmgr.cell;
        }
    }
}
