using UnityEngine;

namespace World.Enemys
{
    public class EnemyBuffView : MonoBehaviour
    {
        public GameObject[] buffs;

        EnemyView view;

        //==================================================================================================

        private void OnEnable()
        {
            view = transform.parent.GetComponent<EnemyView>();

            view.tick1_outter += tick;
        }


        private void OnDisable()
        {
            view.tick1_outter -= tick;
        }


        void tick()
        {
            transform.rotation = Quaternion.identity;

            var buff_list = view.owner.buff_list;
            foreach (var buff_go in buffs)
            {
                buff_go.SetActive(buff_list.Contains(buff_go.name));
            }
        }
    }
}

