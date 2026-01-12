using Foundations;

namespace Assets.Scripts.World.Widgets
{
    public class Widget_Rag_Context:Singleton<Widget_Rag_Context>
    {
        public bool holding = false;
        public bool need_highlight
        {
            get
            {
                return !holding && panel_in_acid;
            }
        }
        public bool panel_in_acid = false;
    }
}
