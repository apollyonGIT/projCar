using Foundations;

namespace Camp.Techs
{
    public class TechPD : Producer
    {
        public TechView model;
        TechView view;

        public override IMgr imgr => mgr;
        TechMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new("TechMgr", priority);

            Tech cell = new();
            mgr.cell = cell;

            view = Instantiate(model, transform);
            view.gameObject.SetActive(false);

            cell.add_view(view);
        }


        public override void call()
        {
            view.gameObject.SetActive(true);
        }
    }
}