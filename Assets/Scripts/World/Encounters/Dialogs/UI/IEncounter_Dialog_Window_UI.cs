using UnityEngine;

namespace World.Encounters.Dialogs
{
    public interface IEncounter_Dialog_Window_UI
    {
        void change(params object[] args);

        public GameObject gameObject { get; }
    }
}

