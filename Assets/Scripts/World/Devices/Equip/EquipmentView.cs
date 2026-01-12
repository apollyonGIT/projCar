using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.Equip
{
    public class EquipmentView : MonoBehaviour, IEquipmentMgrView
    {
        public EquipmentMgr owner;

        public Transform content;
        public EquipView prefab;
        public List<EquipView> edv = new List<EquipView>();

        private const int default_equipview_cnt = 16;
        private const int default_one_row_cnt = 4;

        private void sort_equipview()
        {
            foreach (var d in edv)
            {
                Destroy(d.gameObject);
            }
            edv.Clear();


            for (int i = 0; i < default_equipview_cnt; i++)
            {
                var d = Instantiate(prefab, content, false);
                d.gameObject.SetActive(true);
                edv.Add(d);
            }

            var cnt = owner.devices.Count;

            if (cnt > default_equipview_cnt)
            {
                var need_add = cnt - default_equipview_cnt;
                var row = need_add / default_one_row_cnt;
                var remainder = need_add / default_one_row_cnt;

                if (remainder != 0)
                {
                    for (int i = 0; i < (row + 1) * default_one_row_cnt; i++)
                    {
                        var d = Instantiate(prefab, content, false);
                        d.gameObject.SetActive(true);
                        edv.Add(d);
                    }
                }
                else
                {
                    for (int i = 0; i < row * default_one_row_cnt; i++)
                    {
                        var d = Instantiate(prefab, content, false);
                        d.gameObject.SetActive(true);
                        edv.Add(d);
                    }
                }
            }
            for (int i = 0; i < owner.devices.Count; i++)
            {
                edv[i].gameObject.SetActive(true);
                edv[i].Init(owner.devices[i], owner);
            }
        }


        void IEquipmentMgrView.add_device(Device device)
        {
            sort_equipview();
        }

        void IModelView<EquipmentMgr>.attach(EquipmentMgr owner)
        {
            this.owner = owner;
        }

        void IModelView<EquipmentMgr>.detach(EquipmentMgr owner)
        {
            this.owner = null;
            Destroy(gameObject);
        }

        void IEquipmentMgrView.init()
        {
            for (int i = 0; i < default_equipview_cnt; i++)
            {
                var d = Instantiate(prefab, content, false);
                d.gameObject.SetActive(true);
                edv.Add(d);
            }

            var cnt = owner.devices.Count;

            if (cnt > default_equipview_cnt)
            {
                var need_add = cnt - default_equipview_cnt;
                var row = need_add / default_one_row_cnt;
                var remainder = need_add / default_one_row_cnt;

                if (remainder != 0)
                {
                    for (int i = 0; i < (row + 1) * default_one_row_cnt; i++)
                    {
                        var d = Instantiate(prefab, content, false);
                        d.gameObject.SetActive(true);
                        edv.Add(d);
                    }
                }
                else
                {
                    for (int i = 0; i < row * default_one_row_cnt; i++)
                    {
                        var d = Instantiate(prefab, content, false);
                        d.gameObject.SetActive(true);
                        edv.Add(d);
                    }
                }
            }
            for (int i = 0; i < owner.devices.Count; i++)
            {
                edv[i].gameObject.SetActive(true);
                edv[i].Init(owner.devices[i], owner);
            }
        }

        void IEquipmentMgrView.remove_device(Device device)
        {
            sort_equipview();
        }

        void IEquipmentMgrView.select_device(Device device)
        {
            foreach (var d in edv)
            {
                d.Select(device != null && d.data == device);
            }
        }

        void IEquipmentMgrView.tick()
        {

        }

        void IEquipmentMgrView.update_slot()
        {

        }
    }
}
