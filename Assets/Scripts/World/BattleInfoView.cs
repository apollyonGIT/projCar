using UnityEngine;
using World.Devices;
using World.Relic;

namespace World
{
    public class BattleInfoView : MonoBehaviour
    {
        public RelicMgrView rv;
        public DeviceDmgInfoPanel deviceDmgInfoPanel;


        public void Init()
        {
            rv.Init();
            deviceDmgInfoPanel.ResetPanel();
        }
    }
}
