using Addrs;
using Commons;
using Foundations.MVVM;
using Spine;
using UnityEngine;
using UnityEngine.UI;
using World.CaravanBoards;
using World.Helpers;
using World.Ui;
using World.Widgets;

namespace World.Caravans
{
    public interface IUiFix
    {
        bool Fix();
    }

    public class CaravanUiView : MonoBehaviour, ICaravanView, IUiView
    {
        private const int HP_BAR_SCALE_GRIDS_COUNT = 3;
        private const int HP_BAR_HP_PER_SCALE_GRID = 100;
        private const float HP_BAR_CONVERT_COEF = 1f / (HP_BAR_SCALE_GRIDS_COUNT * HP_BAR_HP_PER_SCALE_GRID);

        public Caravan owner;

        public Slider hp;
        public Slider true_hp;
        public Image true_hp_img;

        public CaravanFixView cfv;

        public CaravanStationView fix;

        private const float hp_change_speed = 5;
        private float current_view_hp;
        Skeleton ICaravanView.sk => throw new System.NotImplementedException();

        Vector2 IUiView.pos => transform.position;

        public CaravanHoveringInfo chi;

        public CaravanLeverPin pin;

        public CaravanLever lever;
        public BellowView blower;
        public SpeedLEDView led;
        public Extinguisher extinguiser;
        public Rag rag;

        public Image caravan_hp_img;
        public Slider caravan_hp_slider;

        public void Brake()
        {
            owner.Brake();
        }

        public void Push()
        {
            Widget_PushCar_Context.instance.PushCaravan();
        }

        void IModelView<Caravan>.attach(Caravan owner)
        {
            this.owner = owner;

            hp.maxValue = WorldContext.instance.caravan_hp_max;
            hp.value = WorldContext.instance.caravan_hp;
            true_hp.maxValue = WorldContext.instance.caravan_hp_max;
            true_hp.value = WorldContext.instance.caravan_hp;
            true_hp_img.pixelsPerUnitMultiplier = WorldContext.instance.caravan_hp * HP_BAR_CONVERT_COEF;

            fix.Init(Widget_Fix_Context.instance.fix_module);

            Ui_Pos_Helper.ui_views.Add(this);

            chi.Init();
        }

        void IModelView<Caravan>.detach(Caravan owner)
        {
            if (this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void ICaravanView.notify_on_hurt()
        {

        }

        void ICaravanView.notify_on_tick()
        {

            var wctx = WorldContext.instance;
            true_hp.value = WorldContext.instance.caravan_hp;
            if (current_view_hp <= WorldContext.instance.caravan_hp)
            {
                current_view_hp = WorldContext.instance.caravan_hp;
                hp.value = current_view_hp;
            }
            else
            {
                current_view_hp -= hp_change_speed * Config.PHYSICS_TICK_DELTA_TIME;
                hp.value = current_view_hp;
            }

            set_hp();
            update_module();

            pin.tick();
            lever.tick();
            blower.tick();
            led.tick();
            extinguiser.tick();
            rag.tick();
        }

        private void set_hp()
        {
            caravan_hp_slider.value = WorldContext.instance.caravan_hp;
            caravan_hp_slider.maxValue = WorldContext.instance.caravan_hp_max;

            switch((float)WorldContext.instance.caravan_hp/WorldContext.instance.caravan_hp_max)
            {
                case > 0.5f:
                    caravan_hp_img.color = Color.green;
                    break;
                case > 0.2f:
                    caravan_hp_img.color = Color.yellow;
                    break;
                default:
                    caravan_hp_img.color = Color.red;
                    break;
            }
        }

        private void update_module()
        {
            cfv.tick();

            if (Character_Module_Helper.GetModule(fix.module) != null)
            {
                Addressable_Utility.try_load_asset(Character_Module_Helper.GetModule(fix.module).desc.portrait_small, out Sprite s);
                fix.character_image.sprite = s;
                fix.character_image.color = new Color(1, 1, 1, 1f);    //显示角色头像
            }
            else
            {
                fix.character_image.sprite = null;
                fix.character_image.color = new Color(1, 1, 1, 0f);
            }
        }

        void ICaravanView.notify_on_tick1()
        {

        }
    }
}
