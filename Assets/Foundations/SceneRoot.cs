using System.Linq;
using UnityEngine;

namespace Foundations
{
    public class SceneRoot<T> : MonoBehaviourSingleton<T> where T : SceneRoot<T>
    {
        public Camera mainCamera;
        public Camera uiCamera;
        public Canvas uiRoot;
        public Transform monoRoot;

        public Producer[] producers;

        //==================================================================================================

        protected void init_producers()
        {
            if (producers == null) return;

            int i = 0;
            foreach (var t in producers.Where(t => t != null))
            {
                t.init(i++);
            }
        }


        protected void fini_producers()
        {
            if (producers == null) return;

            foreach (var t in producers.Where(t => t.imgr != null))
            {
                t.imgr.fini();
            }
        }


        protected void call_producers()
        {
            if (producers == null) return;

            foreach (var t in producers.Where(t => t != null))
            {
                t.call();
            }
        }


        protected override void on_init()
        {
            init_producers();
        }


        protected override void on_fini()
        {
            fini_producers();
        }
    }
}

