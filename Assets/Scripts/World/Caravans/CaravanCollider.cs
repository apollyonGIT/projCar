using Foundations;
using UnityEngine;
using World.Enemys;
using World.Helpers;
namespace World.Caravans
{
    public class CaravanCollider : MonoBehaviour
    {
        public CaravanView caravan_view;

        //==================================================================================================

        public bool valid_is_player()
        {
            var caravan = caravan_view.owner;

            if (!Mission.instance.try_get_mgr("CaravanMgr", out CaravanMgr cmgr))
                return false;

            return caravan == cmgr.cell;
        }


        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (!valid_is_player()) return;

            if (collision.TryGetComponent<EnemyHitbox>(out var eb))
            {
                var t = eb.view.owner;
                Caravan_Collide_Helper.crush_to_enemy(caravan_view.owner, t);
            }
        }


        public void OnTriggerExit2D(Collider2D collision)
        {
            if (!valid_is_player()) return;
        }
    }
}
