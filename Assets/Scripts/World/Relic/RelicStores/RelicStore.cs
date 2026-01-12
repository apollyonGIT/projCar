using Foundations.MVVM;
using System.Collections.Generic;

namespace World.Relic.RelicStores
{
    public interface IRelicStoreView : IModelView<RelicStore>
    {
        void init();
    }

    public class RelicStore : Model<RelicStore, IRelicStoreView>
    {
        public List<Relic> relic_list = new();

        public void Init(List<Relic> relics)
        {
            relic_list = relics;

            foreach(var view in views)
            {
                view.init();
            }
        }

        public void Init()
        {

        }
    }
}
