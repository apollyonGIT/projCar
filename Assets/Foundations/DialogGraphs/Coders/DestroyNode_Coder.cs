using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Foundations.DialogGraphs
{
    public class DestroyNode_Coder : IDialogNode_Coder
    {
        public Dictionary<string, IDialogNode_Coder> coders = new();

        Dictionary<int, Func<object>> m_outputs = new();
        Func<object[], object> m_input;
        Dictionary<string, object> m_fields = new();

        Dictionary<int, Func<object>> IDialogNode_Coder.outputs { get => m_outputs; set => m_outputs = value; }
        Func<object[], object> IDialogNode_Coder.input { get => m_input; set => m_input = value; }
        Dictionary<string, object> IDialogNode_Coder.fields { get => m_fields; set => m_fields = value; }

        string IDialogNode_Coder.uname => m_uname;
        string m_uname = "";

        string IDialogNode_Coder.key_name => "DestroyNode";

        //==================================================================================================

        void IDialogNode_Coder.notify_on_init(params object[] args)
        {
            var data = (DialogNode_Data)args[0];
            m_uname = data.uname;

            m_fields.Add("target_uname", (string)EX_Utility.dic_safe_getValue(ref data.fields, "target_uname", ""));
        }


        object IDialogNode_Coder.do_output(int index, params object[] args)
        {
            return null;
        }


        object IDialogNode_Coder.do_input(params object[] args)
        {
            var target_uname = (string)m_fields["target_uname"];

            if (target_uname == "")
                return m_input?.Invoke(null);

            if (target_uname.Contains("@"))
            {
                Share_DS.instance.try_get_value(target_uname, out target_uname);
                Share_DS.instance.remove(target_uname);
            }

            var coder_info = coders.Where(t => t.Value.uname == target_uname);
            if (coder_info == null)
            {
                Debug.LogError("DestoryNode查询目标uname失败");
                return m_input?.Invoke(null);
            }

            var target_coder = coder_info.First().Value;
            return m_input?.Invoke(new object[] { target_coder });
        }
    }
}

