using Foundations;

namespace World.Widgets
{
    public class Widget_Extinguisher_Context : Singleton<Widget_Extinguisher_Context>
    {
        public bool holding = false;
        public bool need_highlight
        {
            get
            {
                return !holding && panel_in_fire;
            }
        }
        public bool panel_in_fire = false;
    }
}
