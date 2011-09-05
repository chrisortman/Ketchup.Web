using System;
using System.ComponentModel.Composition;

namespace Ketchup.Web.Mef
{
    [MetadataAttribute]
    public class ExperimentalAttribute : Attribute
    {
        public ExperimentalAttribute()
        {
            IsExperimental = true;
        }

        public bool IsExperimental { get; set; }
    }
}