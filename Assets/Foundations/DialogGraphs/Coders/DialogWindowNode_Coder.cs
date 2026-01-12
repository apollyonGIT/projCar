using System;
using System.Collections.Generic;

namespace Foundations.DialogGraphs
{
    public class DialogWindowNode_Coder : IDialogNode_Coder
    {
        Dictionary<int, Func<object>> m_outputs = new();
        Func<object[], object> m_input;
        Dictionary<string, object> m_fields = new();

        Dictionary<int, Func<object>> IDialogNode_Coder.outputs { get => m_outputs; set => m_outputs = value; }
        Func<object[], object> IDialogNode_Coder.input { get => m_input; set => m_input = value; }
        Dictionary<string, object> IDialogNode_Coder.fields { get => m_fields; set => m_fields = value; }

        string IDialogNode_Coder.uname => m_uname;
        string m_uname = "";

        string IDialogNode_Coder.key_name => "DialogWindowNode";

        //==================================================================================================

        void IDialogNode_Coder.notify_on_init(params object[] args)
        {
            var data = (DialogNode_Data)args[0];
            m_uname = data.uname;

            m_fields.Add("title", EX_Utility.dic_safe_getValue(ref data.fields, "titleText", ""));
            m_fields.Add("content", EX_Utility.dic_safe_getValue(ref data.fields, "dialogText", ""));
            m_fields.Add("console", EX_Utility.dic_safe_getValue(ref data.fields, "consoleText", ""));

            List<(string, Func<object>)> output_ac_list = new();
            int index = 0;
            foreach (var port in data.ports)
            {
                if (m_outputs.TryGetValue(index, out var ac))
                {
                    output_ac_list.Add((port.portName, ac));
                }

                index++;
            }
            m_fields.Add("output_ac_list", output_ac_list);
        }


        object IDialogNode_Coder.do_output(int index, params object[] args)
        {
            return m_outputs[index]?.Invoke();
        }


        object IDialogNode_Coder.do_input(params object[] args)
        {
            return m_input?.Invoke(new object[] { 
                m_uname,
                m_fields["title"], 
                m_fields["content"],
                m_fields["output_ac_list"],
                m_fields["console"]
            });
        }
    }
}

