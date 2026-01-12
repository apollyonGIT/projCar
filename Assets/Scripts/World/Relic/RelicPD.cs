using Commons;
using Foundations;
using System.Linq;

namespace World.Relic
{
    public class RelicPD : Producer
    {
        public override IMgr imgr => mgr;
        RelicMgr mgr;

        public RelicMgrView rv;

        //因为战斗信息那块设定没有敲定 所以是临时
        public RelicMgrView temp_rv;

        public override void call()
        {
            
        }

        public override void init(int priority)
        {
            if (Saves.Save_DS.instance.need_load_device)
                use_current_data(priority);
            else
                use_default_data(priority);
        }


        void use_current_data(int priority)
        {
            mgr = new(Config.RelicMgr_Name, priority);
            mgr.add_view(rv);
            mgr.add_view(temp_rv);

            var datas = Saves.Save_DS.instance.relic_datas;

            //规则：禁测试relic，临时
            foreach (var relic_id in datas.Where(t => t != 93))
            {
                mgr.CreateRelicAndAddDirectly($"{relic_id}");
            }
        }


        void use_default_data(int priority)
        {
            mgr = new(Config.RelicMgr_Name, priority);
            mgr.add_view(rv);
            mgr.add_view(temp_rv);
        }
    }
}
