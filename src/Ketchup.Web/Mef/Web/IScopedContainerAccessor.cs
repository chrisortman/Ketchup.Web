using System.ComponentModel.Composition.Hosting;

namespace Microsoft.ComponentModel.Composition.Extensions.Web
{
    public interface IScopedContainerAccessor
    {
        CompositionContainer GetRequestLevelContainer();
    }
}