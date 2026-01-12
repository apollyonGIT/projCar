using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Progresss
{
    public class ProgressView : MonoBehaviour, IProgressView
    {
        public Progress owner;

        public Transform content;
        public ProgressEventView prefab;
        public List<ProgressEventView> pev_list = new();
        void IModelView<Progress>.attach(Progress owner)
        {
            this.owner = owner;
        }

        void IModelView<Progress>.detach(Progress owner)
        {
            if (this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void IProgressView.notify_add_progress_event(ProgressEvent pe)
        {
            if(pe.record.dialogue_graph == null)
            {
                return;
            }
            var pev = Instantiate(prefab,content,false);
            pev.Init(pe);
            pev.gameObject.SetActive(true);
            pev_list.Add(pev);
        }

        void IProgressView.notify_arrived_progress_event(ProgressEvent pe)
        {
            
        }

        void IProgressView.notify_enter_progress_event(ProgressEvent pe)
        {
            
        }

        void IProgressView.notify_exit_progress_event(ProgressEvent pe)
        {
            
        }

        void IProgressView.notify_init()
        {
            
        }

        void IProgressView.notify_notice_encounter(float p, bool b)
        {
            
        }

        void IProgressView.notify_on_tick()
        {
            foreach(var pev in pev_list)
            {
                pev.tick();
            }
        }

        void IProgressView.notify_remove_progress_event(ProgressEvent pe)
        {
            for(int i = pev_list.Count - 1; i >= 0; i--)
            {
                if (pev_list[i].pe == pe)
                {
                    pev_list[i].Destroy();
                    pev_list.RemoveAt(i);
                }
            }
        }
    }
}
