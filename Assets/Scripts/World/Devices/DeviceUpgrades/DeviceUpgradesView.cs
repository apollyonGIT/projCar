using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.DeviceUpgrades
{
    public class DeviceUpgradesView : MonoBehaviour
    {
        public Transform content;
        public DeviceUpgradeView prefab;
        public DeviceUpgradeInfoView infoView;

        public List<DeviceUpgradeView> duv_list = new();
        public Device own_device;

        public bool IsReadOnly;

        public void Init(Device device)
        {
            own_device = device;

            foreach(var upgrade in device.upgrades)
            {
                var upgrade_view = Instantiate(prefab, content, false);
                upgrade_view.gameObject.SetActive(true);
                upgrade_view.Init(upgrade);

                duv_list.Add(upgrade_view);
            }

            if (duv_list.Count != 0)
            {
                infoView.Init(duv_list[0].data);
            }
        }

        public void Update()
        {
            foreach (var upgrade in duv_list)
            {
                upgrade.tick();
            }

            infoView.tick();
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
