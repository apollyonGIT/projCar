using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.Equip
{
    public class EquipDeviceJobView : MonoBehaviour
    {
        public Transform job_content;

        public EquipSingleJob job_prefab;

        public List<EquipSingleJob> jobs;
        public void Init(DeviceSlot slot)
        {
            foreach (var job in jobs)
            {
                Destroy(job.gameObject);
            }

            jobs.Clear();

            for(var i = 0; i < slot.width; i++)
            {
                var job_view = Instantiate(job_prefab, job_content);
                job_view.gameObject.SetActive(true);
                jobs.Add(job_view);
            }

            if (slot.slot_device != null)
            {
                for (var i = 0; i < slot.slot_device.desc.size; i++)
                {
                    jobs[i].SetHighlight();
                }
            }
        }


        public void Init(Device device)
        {
            foreach (var job in jobs)
            {
                Destroy(job.gameObject);
            }

            jobs.Clear();

            for (var i = 0; i < device.desc.size; i++)
            {
                var job_view = Instantiate(job_prefab, job_content);
                job_view.gameObject.SetActive(true);
                job_view.SetHighlight();
                jobs.Add(job_view);
            }
        }

    }
}
