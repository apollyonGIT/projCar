using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commons
{
    public class Attributes
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
        public class DetailAttribute : Attribute
        {
            public string detail { get; }
            public DetailAttribute(string name)
            {
                this.detail = name;
            }
        }


        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        public class ExprConstAttribute : Attribute
        {
            public string name { get; }
            public ExprConstAttribute() { }
            public ExprConstAttribute(string name) { this.name = name; }
        }
    }
}

