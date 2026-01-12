using Addrs;
using Commons;
using Foundations;

namespace World.Caravans
{
    public class CaravanPD : Producer
    {
        public override IMgr imgr => mgr;
        CaravanMgr mgr;

        public CaravanUiView cuv;

        uint caravan_id => Config.current.caravan_id;

        //==================================================================================================

        public override void init(int priority)
        {
            if (Saves.Save_DS.instance.need_load_device)
                use_current_data(priority);
            else
                use_default_data(priority);

            var cell = mgr.cell;

            Addressable_Utility.try_load_asset(cell._desc.view, out CaravanView model);
            var view = Instantiate(model, transform);
            cell.add_view(view);

            cell.add_view(cuv);
        }


        public void use_current_data(int priority)
        {
            var ctx = WorldContext.instance;
            var datas = Saves.Save_DS.instance.caravan_datas;

            mgr = new("CaravanMgr", priority);
            mgr.cell = new($"{(uint)datas[0]}");

            ctx.caravan_hp = (int)datas[1];
            ctx.caravan_hp_max = (int)datas[2];
        }


        public void use_default_data(int priority)
        {
            mgr = new("CaravanMgr", priority);
            mgr.cell = cell();
        }


        public override void call()
        {
        }


        public Caravan cell()
        {
            return new($"{caravan_id}");
        }
    }
}