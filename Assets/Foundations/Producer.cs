using UnityEngine;

namespace Foundations
{
    public abstract class Producer : MonoBehaviour
    {
        public abstract IMgr imgr { get; }

        //==================================================================================================

        public abstract void init(int priority);

        public abstract void call();
    }
}

