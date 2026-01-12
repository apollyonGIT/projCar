using UnityEngine;

namespace World.Environments.Roads
{
    public class RoadView : MonoBehaviour
    {
        public Curve curve;
        public void init(Curve curve) {
            this.curve = curve;

            GetComponent<SpriteRenderer>().sprite = curve.curve_sprite;
        }

    }
}
