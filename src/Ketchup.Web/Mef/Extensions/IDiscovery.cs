using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace Microsoft.ComponentModel.Composition.Extensions
{
    public interface IDiscovery
    {
        IEnumerable<ComposablePartDefinition> BuildPartDefinitions(IEnumerable<Type> types);
    }
}
