using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Ketchup.Web.Mef
{
    public static class AttributedModelServicesExtensions
    {
        public static IDictionary<string,object> MetadataWithTypeIdentity(Type type)
        {
            var metadata = new Dictionary<string, object>();

            metadata["ExportTypeIdentity"] = AttributedModelServices.GetTypeIdentity(type);

            return metadata;
        }
    }
}
