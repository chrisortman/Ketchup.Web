using System;
using System.ComponentModel.Composition;

namespace CustomerPortal.Infrastructure.Composition
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