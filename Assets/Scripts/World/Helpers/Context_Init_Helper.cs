using World.Progresss;
using World.Widgets;

namespace World.Helpers
{
    public class Context_Init_Helper
    {
        public static void init_diy_context()
        {
            Widget_DrivingLever_Context._init();
            Widget_DrivingLever_Context.instance.attach();

            Widget_Fix_Context._init();
            Widget_Fix_Context.instance.attach();

            Widget_Blower_Context._init();
            Widget_Blower_Context.instance.attach();

            Widget_PushCar_Context._init();
            Widget_PushCar_Context.instance.attach();

            Progress_Context._init();
            Progress_Context.instance.attach();
        }
    }
}

