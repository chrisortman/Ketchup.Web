using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace Ketchup.Web.Mef.Extensions
{
    public interface IDiscovery
    {
        IEnumerable<ComposablePartDefinition> BuildPartDefinitions(IEnumerable<Type> types);
    }
}
