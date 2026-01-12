using Commons;
using Foundations.MVVM;
using Foundations.Tickers;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace World.Caravans
{
    public class CaravanView : MonoBehaviour, ICaravanView
    {
        public SkeletonAnimation anim;
        public Renderer anim_renderer;
        Skeleton ICaravanView.sk => anim.skeleton;

        public Caravan owner;

        public GameObject diascope_camera;

        //==================================================================================================

        void IModelView<Caravan>.attach(Caravan owner)
        {
            this.owner = owner;

            owner.load_caravan_slots(this);
        }


        void IModelView<Caravan>.detach(Caravan owner)
        {
            this.owner = null;
        }


        void ICaravanView.notify_on_tick1()
        {
            transform.localPosition = owner.view_pos;
            transform.localRotation = owner.view_rotation;

            anim.timeScale = Mathf.Min(2f,0.6f + WorldContext.instance.caravan_velocity.magnitude / 4f);

            var anim_info = owner.anim_info;
            if (anim.AnimationName != anim_info.anim_name)
            {
                anim.state.SetAnimation(0, anim_info.anim_name, anim_info.loop);
            }

            anim.Update(Config.PHYSICS_TICK_DELTA_TIME);
        }


        void ICaravanView.notify_on_hurt()
        {
            foreach (var req in Request_Helper.query_request("caravan_hurt_fillPhase_stop"))
            {
                req.interrupt();
            }

            anim_renderer.material.SetFloat("_FillPhase", 0.8f);
            Request_Helper.delay_do("caravan_hurt_fillPhase_stop", 40, (_) => { anim_renderer.material.SetFloat("_FillPhase", 0f); });
        }


        public void remove_diascope_camera()
        {
            if (diascope_camera != null)
                DestroyImmediate(diascope_camera);
        }


        void ICaravanView.notify_on_tick()
        {
            
        }


        private void OnDestroy()
        {
            foreach (var req in Request_Helper.query_request("caravan_hurt_fillPhase_stop"))
            {
                req.interrupt();
            }
        }
    }
}

