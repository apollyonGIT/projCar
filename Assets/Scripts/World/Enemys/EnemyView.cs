using Commons;
using Foundations.MVVM;
using Foundations.Tickers;
using Spine.Unity;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace World.Enemys
{
    public class EnemyView : MonoBehaviour, IEnemyView
    {
        public SkeletonAnimation anim;
        Renderer m_anim_renderer;

        public Animator animator;

        public Enemy owner;
        public Action tick1_outter;

        public GameObject pre_aim;
        public GameObject aim;
        public SpriteRenderer outrange_aim;

        //==================================================================================================

        void IModelView<Enemy>.attach(Enemy owner)
        {
            this.owner = owner;

            if (anim == null) return;
            owner.bones = anim.skeleton.Bones.ToDictionary(k => k.Data.Name, v => v);

            var anim_info = owner.anim_info;
            if (anim_info == null) return;

            var anim_name = (string)anim_info["anim_name"];
            var anim_loop = (bool)anim_info["loop"];

            set_anim(anim_name, anim_loop);

            m_anim_renderer = anim.transform.GetComponent<MeshRenderer>();
        }


        void IModelView<Enemy>.detach(Enemy owner)
        {
            this.owner = null;

            DestroyImmediate(gameObject);
        }


        void IEnemyView.notify_on_tick1()
        {
            transform.localPosition = calc_pos();
            transform.localScale = Vector3.one * owner.battle_ctx.enemy_scale_factor;

            if (anim != null)
            {
                transform.localRotation = owner.view_rotation;

                anim.skeleton.ScaleX = owner.view_scaleX;
                anim.skeleton.ScaleY = owner.view_scaleY;

                var anim_info = owner.anim_info;
                if (anim_info != null)
                {
                    var anim_name = (string)anim_info["anim_name"];
                    var anim_loop = (bool)anim_info["loop"];

                    if (anim.AnimationName != anim_name)
                    {
                        anim.state.SetAnimation(0, anim_name, anim_loop);
                    }
                }

                anim.Update(Config.PHYSICS_TICK_DELTA_TIME);
            }
            else
            {
                var _localScale = transform.localScale;
                _localScale.x *= owner.view_pic_scaleX;
                transform.localScale = _localScale;
            }

            if (animator != null)
            {
                var anim_info = owner.anim_info;
                if (anim_info != null)
                {
                    var anim_name = (string)anim_info["anim_name"];
                    animator.SetTrigger(anim_name);
                }
            }

            tick1_outter?.Invoke();
        }


        void IEnemyView.notify_on_hurt()
        {
            var req_name = $"enemy_{owner.GUID}_hurt_fillPhase_stop";

            foreach (var req in Request_Helper.query_request(req_name))
            {
                req.interrupt();
            }

            if (m_anim_renderer == null) return;

            m_anim_renderer.material.SetFloat("_FillPhase", 1f);
            Request_Helper.delay_do(req_name, 15, (_) => 
            {
                if (m_anim_renderer == null) return;
                m_anim_renderer.material.SetFloat("_FillPhase", 0f); 
            });
        }


        void IEnemyView.notify_on_pre_aim(bool ret)
        {
            if (pre_aim == null)
                return;
            pre_aim.SetActive(ret);
        }

        void IEnemyView.notify_on_aim(bool ret)
        {
            if(aim!=null)
                aim.SetActive(ret);
        }

        void IEnemyView.notify_on_outrange_aim(bool ret)
        {
            if (outrange_aim == null)
                return;

            outrange_aim.gameObject.SetActive(ret);
            if (ret)
            {
                StartCoroutine(Iblink());
            }
            else
            {
                StopCoroutine(Iblink());
            }
        }

        IEnumerator Iblink()
        {
            var color = outrange_aim.color;
            while (true)
            {
                outrange_aim.color = new Color(color.r, color.g, color.b, 0.5f);
                yield return new WaitForSeconds(0.1f);
                outrange_aim.color = new Color(color.r, color.g, color.b, 0.9f);
                yield return new WaitForSeconds(0.1f);
            }
        }


		void set_anim(string anim_name, bool anim_loop)
        {
            var entry = anim.state.SetAnimation(0, anim_name, anim_loop);
            entry.TrackTime = Config.PHYSICS_TICK_DELTA_TIME * UnityEngine.Random.Range(0, 100);
            anim.Update(0);
        }


        Vector3 calc_pos()
        {
            Vector3 v3 = owner.view_pos;

            var index = transform.GetSiblingIndex();
            var coef = 1e-8f;
            v3.z = index * coef;

            return v3;
        }


        void IEnemyView.notify_on_change_color(Color color)
        {
            anim.skeleton.SetColor(color);
        }
    }
}

