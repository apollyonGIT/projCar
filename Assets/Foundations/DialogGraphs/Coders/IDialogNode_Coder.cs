using System;
using System.Collections.Generic;

namespace Foundations.DialogGraphs
{
    public interface IDialogNode_Coder
    {
        void notify_on_init(params object[] args);

        object do_input(params object[] args);

        object do_output(int index, params object[] args);

        Func<object[], object> input { get; set; }

        Dictionary<int, Func<object>> outputs { get; set; }

        Dictionary<string, object> fields { get; set; }

        string uname { get; }

        string key_name { get; }
    }
}

