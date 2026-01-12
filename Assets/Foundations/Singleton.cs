using UnityEngine;

namespace Foundations
{
    public class Singleton<T> where T : new()
    {
        public static T instance => m_instance ?? _init();
        static T m_instance;

        //==================================================================================================

        public static T _init()
        {
            m_instance = new();
            return m_instance;
        }
    }


    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        public static T instance { get; private set; }

        //==================================================================================================

        private void Awake()
        {
            instance = (T)this;
            on_init();
        }


        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                on_fini();
            }
        }


        protected virtual void on_init()
        {
        }


        protected virtual void on_fini()
        {
        }
    }
}

