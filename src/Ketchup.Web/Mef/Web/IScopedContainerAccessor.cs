using System.ComponentModel.Composition.Hosting;

namespace Ketchup.Web.Mef.Web
{
    public interface IScopedContainerAccessor
    {
        CompositionContainer GetRequestLevelContainer();
    }
}