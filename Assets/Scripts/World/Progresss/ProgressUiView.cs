using Addrs;
using Commons;
using Foundations;
using Foundations.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Audio;

namespace World.Progresss
{
    public class ProgressUiView : MonoBehaviour, IProgressView
    {
        #region CONST
        private const float NOTICE_POS_Y_END = -600F;
        private const float NOTICE_POS_Y_DELTA = 200f;

        private const float NOTICE_SIZE_END = 200F;
        private const float NOTICE_SIZE_DELTA = -50F;
        #endregion

        Progress owner;

        public Slider progress_slider;
        public TextMeshProUGUI progress_text;

        public Image notice;
        public TextMeshProUGUI notice_text;
        public Image alarm_danger;

        public GameObject plot_prefab_vital;
        public GameObject plot_prefab_trivial;

        public TextMeshProUGUI lv_info_sceneName;
        public TextMeshProUGUI lv_info_index;
        public Image lv_info_bg;

        private bool audio_triggered = false;

        //==================================================================================================

        void IModelView<Progress>.attach(Progress owner)
        {
            this.owner = owner;

            var ctx = WorldContext.instance;
            var r_game_world = ctx.r_game_world;
            var ds = Share_DS.instance;

            lv_info_sceneName.text = Localization_Utility.get_localization(r_game_world.world_name);

            ds.try_get_value(Game_Mgr.key_scene_index, out int scene_index);
            lv_info_index.text = $"{scene_index}";

            Addressable_Utility.try_load_asset<Sprite>(r_game_world.UI_level_index_bg, out var sprite);
            lv_info_bg.sprite = sprite;
        }


        void IModelView<Progress>.detach(Progress owner)
        {
            this.owner = null;
        }


        void IProgressView.notify_on_tick()
        {
            //progress_slider.value = owner.current_progress;
            progress_slider.value = WorldContext.instance.scene_progress_rate;

            var distance_remaining = (owner.total_progress - owner.current_progress) * 2;  // unit to meters = 2.0;
            progress_text.text = $"{distance_remaining:F0}";
        }

        void IProgressView.notify_notice_encounter(float p, bool b)
        {
            if (!b)
            {
                notice.gameObject.SetActive(false);
                progress_text.color = Color.white;

                return;
            }

            if (p - owner.current_progress < Config.current.notice_length_1 && p - owner.current_progress > Config.current.notice_length_2)
            {
                notice.gameObject.SetActive(true);
                var distance_remaining_noticed = p - owner.current_progress;
                notice_text.text = $"{distance_remaining_noticed * 2:F1} m";     // unit to meters = 2.0;

                var t = 1 - Mathf.Exp(-(distance_remaining_noticed - Config.current.notice_length_2) * 0.01f);

                var y = NOTICE_POS_Y_END + t * NOTICE_POS_Y_DELTA;
                notice.rectTransform.anchoredPosition = new Vector2(0, y);

                var size = NOTICE_SIZE_END + t * NOTICE_SIZE_DELTA;

                notice.rectTransform.sizeDelta = new Vector2(size, notice.rectTransform.sizeDelta.y);
                progress_text.color = Color.red;

                alarm_danger.color = new Color(1, 1, 1, 1 - t);

                if (audio_triggered == false)
                {
                    audio_triggered = true;
                    AudioSystem.instance.PlayClip(Config.current.SE_encounter_radar, true);
                }
                else
                {
                    AudioSystem.instance.SetClipPitch(Config.current.SE_encounter_radar, (3 - t) * 0.5F);
                }
            }
        }

        void IProgressView.notify_add_progress_event(ProgressEvent pe)
        {

        }

        void IProgressView.notify_remove_progress_event(ProgressEvent pe)
        {

        }

        void IProgressView.notify_init()
        {
            int progress_bar_length = Mathf.Min(100 + 4 * (int)(owner.total_progress * 0.075f), 2000);
            float progress_bar_height = progress_slider.GetComponent<RectTransform>().sizeDelta.y;
            progress_slider.GetComponent<RectTransform>().sizeDelta = new Vector2(progress_bar_length, progress_bar_height);
            progress_slider.maxValue = owner.total_progress;

            foreach (var p in owner.single_plots)
            {
                var percent = p.trigger_progress / owner.total_progress;
                var plot = Instantiate(p.ui_visible ? plot_prefab_vital : plot_prefab_trivial, progress_slider.transform);
                plot.GetComponent<RectTransform>().anchoredPosition = new Vector2(percent * progress_slider.GetComponent<RectTransform>().sizeDelta.x - progress_slider.GetComponent<RectTransform>().sizeDelta.x / 2, 0);
                plot.gameObject.SetActive(true);
            }
        }

        void IProgressView.notify_enter_progress_event(ProgressEvent pe)
        {

            alarm_danger.color = new Color(1, 1, 1, 1);
        }

        void IProgressView.notify_exit_progress_event(ProgressEvent pe)
        {
            alarm_danger.color = Color.clear;
            audio_triggered = false;

            AudioSystem.instance.StopClip(Config.current.SE_encounter_radar);
        }

        void IProgressView.notify_arrived_progress_event(ProgressEvent pe)
        {
            
        }
    }
}

