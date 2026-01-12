using System;
using System.Collections.Generic;

namespace Foundations.DialogGraphs
{
    public class DialogGraph_Utility
    {
        public static void read_asset(DialogGraphAsset asset, out Dictionary<string, IDialogNode_Coder> coders_dic, out string entry_key)
        {
            coders_dic = new();
            entry_key = "";

            var asset_edges = (Dictionary<string, Edge_Data>)EX_Utility.byte2object(asset.edges_data);
            var asset_nodes = (Dictionary<string, DialogNode_Data>)EX_Utility.byte2object(asset.nodes_data);

            foreach (var (_, connect) in asset_edges)
            {
                var i_node_GUID = connect.i_node_GUID;
                var o_node_GUID = connect.o_node_GUID;
                var o_index = connect.o_portIndex;

                if (connect.is_entry)
                {
                    entry_key = i_node_GUID;

                    if (!coders_dic.ContainsKey(i_node_GUID))
                    {
                        IDialogNode_Coder coder = create_coder(i_node_GUID, asset_nodes);
                        upd_node(coders_dic, i_node_GUID, coder);
                    }

                    continue;
                }

                IDialogNode_Coder o_coder;
                if (!coders_dic.ContainsKey(o_node_GUID))
                {
                    o_coder = create_coder(o_node_GUID, asset_nodes);
                    upd_node(coders_dic, o_node_GUID, o_coder);
                }
                else
                {
                    o_coder = coders_dic[o_node_GUID];
                }

                IDialogNode_Coder i_coder;
                if (!coders_dic.ContainsKey(i_node_GUID))
                {
                    i_coder = create_coder(i_node_GUID, asset_nodes);
                    upd_node(coders_dic, i_node_GUID, i_coder);
                }
                else
                {
                    i_coder = coders_dic[i_node_GUID];
                }

                o_coder.outputs.Add(o_index, () => { return i_coder.do_input(); });
            }

            foreach (var (key, coder) in coders_dic)
            {
                asset_nodes.TryGetValue(key, out var node_data);
                coder.notify_on_init(node_data);
            }
        }


        static void upd_node(Dictionary<string, IDialogNode_Coder> coders_dic, string key, IDialogNode_Coder coder)
        {
            if (!coders_dic.ContainsKey(key))
            {
                coders_dic.Add(key, coder);
                return;
            }

            coders_dic[key] = coder;
        }


        static IDialogNode_Coder create_coder(string node_guid, Dictionary<string, DialogNode_Data> asset_nodes)
        {
            asset_nodes.TryGetValue(node_guid, out var node_data);

            return (IDialogNode_Coder)Activator.CreateInstance(node_data.coder_type);
        }
    }
}

