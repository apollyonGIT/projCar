using UnityEngine;
using UnityEngine.UI;
using World.Helpers;
using TMPro;
using Commons;
using World.Audio;

namespace World.Progresss
{
    public class ProgressEventView : MonoBehaviour
    {
        #region Const
        private const float FIXED_TRANSFORM_Z = 10F;
        private const float BTN_Y_OFFSET_COEF = -0.1F;
        private const float BTN_Y_OFFSET_FIXED = 3.25F;
        #endregion

        public ProgressEvent pe;
        public Slider progress_slider;
        public Image icon;
        public TextMeshProUGUI cd_TX;
        public GameObject cd_TX_bg;
        public Button interact_btn;
        public Image board;
        public Image mark;

        public ProgressStationView station;

        private float btn_x_offset_coef;


        #region 触发按钮的逻辑相关
        private bool btn_interactable = false;



        #endregion

        public void Init(ProgressEvent pe)
        {
            var pos = pe.pos;
            this.pe = pe;
            transform.position = new Vector3(pos.x, pos.y, FIXED_TRANSFORM_Z);
            station.Init(pe.module);
            progress_slider.maxValue = pe.max_value;
            btn_x_offset_coef = 12.8f / Config.current.trigger_length - 1;

            interact_btn.onClick.AddListener(() =>
            {
                AudioSystem.instance.StopClip(Config.current.SE_encounter_radar);
            });
        }

        public void Destroy()
        {
            //清空module
            Character_Module_Helper.EmptyModule(station.module);
            Destroy(gameObject);
        }

        public void tick()
        {
            var pos = pe.pos;
            progress_slider.value = pe.current_value;
            transform.position = new Vector3(pos.x, pos.y, FIXED_TRANSFORM_Z);
            Vector3 delta = transform.position - (Vector3)WorldContext.instance.caravan_pos;
            interact_btn.image.rectTransform.anchoredPosition = new Vector2(delta.x * btn_x_offset_coef, delta.y * BTN_Y_OFFSET_COEF + BTN_Y_OFFSET_FIXED);

            if (WorldContext.instance.caravan_velocity.x > 0.1)
            {
                board.color = Color.gray;
                mark.color = Color.gray;
                interact_btn.interactable = false;
            }
            else
            {
                board.color = Color.white;
                mark.color = Color.white;
                interact_btn.interactable = true;
            }

            cd_TX.text = pe.cd_TX;
            cd_TX_bg.SetActive(cd_TX.text != null && cd_TX.text != "");

            station.tick();
        }

        public void AddValue()
        {
            pe.current_value = pe.max_value;
        }
    }
}
