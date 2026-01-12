using UnityEngine;

namespace World.Environments
{
    public class EnvironmentObjView : MonoBehaviour {

        public float depth;
        public EnvironmentSingleObj data;
        public Vector3 default_pos;
        public void init(float depth, EnvironmentSingleObj data,Vector3 dp) {
            this.depth = depth;
            this.data = data;
            this.default_pos = dp;
        }
    }
}
