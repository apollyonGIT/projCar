using Foundations.MVVM;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace World.BackPack
{
    public class BackPackMgrView : MonoBehaviour, IBackPackMgrView
    {
        public BackPackMgr owner;
        public Transform content;

        public BackPackSlotView slot_prefab;
        public List<BackPackSlotView> slot_views = new List<BackPackSlotView>();

        public TextMeshProUGUI coin_text;

        void IBackPackMgrView.add_slot(BackPackSlot slot)
        {
            var slot_view = Instantiate(slot_prefab, content,false);
            slot_view.Init(slot);

            slot_view.gameObject.SetActive(true);
            slot_views.Add(slot_view);
        }

        void IModelView<BackPackMgr>.attach(BackPackMgr owner)
        {
            this.owner = owner;
        }

        void IModelView<BackPackMgr>.detach(BackPackMgr owner)
        {
            if(this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void IBackPackMgrView.remove_slot(BackPackSlot slot)
        {
            for(int i = slot_views.Count - 1; i >= 0; i--)
            {
                if (slot_views[i].data == slot)
                {
                    Destroy(slot_views[i].gameObject);
                    slot_views.RemoveAt(i);
                }
            }
        }

        void IBackPackMgrView.select_slot(BackPackSlot slot)
        {
            foreach(var slot_view in slot_views)
            {
                if(slot_view.data == slot)
                {
                    slot_view.select.gameObject.SetActive(true);
                    return;
                }
            }
        }

        void IBackPackMgrView.tick()
        {
            foreach (var slot_view in slot_views)
            {
                slot_view.tick();
            }

            coin_text.text = owner.coin_count.ToString();   
        }

        void IBackPackMgrView.unselect_slot(BackPackSlot slot)
        {
            foreach (var slot_view in slot_views)
            {
                if (slot_view.data == slot)
                {
                    slot_view.select.gameObject.SetActive(false);
                    return;
                }
            }
        }
    }
}
