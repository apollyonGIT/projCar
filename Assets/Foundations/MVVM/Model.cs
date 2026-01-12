using System.Collections.Generic;
using System.Linq;

namespace Foundations.MVVM
{
    public class Model<T, V> where V : IModelView<T> where T : class
    {
        public LinkedList<V> views = new();

        //==================================================================================================

        public void add_view(V view)
        {
            view.attach(this as T);
            views.AddLast(view);
        }


        public void remove_view(V view)
        {
            view.detach(this as T);
            views.Remove(view);
        }


        public void remove_all_views()
        {
            var count = views.Count;

            for (int i = 0; i < count; i++)
            {
                remove_view(views.Last.Value);
            }
        }
    }


    public interface IModelView<T>
    {
        void attach(T owner);

        void detach(T owner);
    }
}


