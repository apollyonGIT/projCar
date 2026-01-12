using System.Collections.Generic;
using UnityEngine;

namespace Foundations.DialogGraphs
{
    [CreateAssetMenu(fileName = "dg", menuName = "DIY_Graph/DialogGraph")]
    public class DialogGraphAsset : ScriptableObject
    {
        //public Dictionary<string, DialogNode_Data> nodes = new();
        //public Dictionary<string, Edge_Data> edges = new();

        public byte[] nodes_data;
        public byte[] edges_data;
    }
}