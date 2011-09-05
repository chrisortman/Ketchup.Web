using System.Web.UI;

namespace Microsoft.ComponentModel.Composition.WebExtensions
{
    [ControlBuilder(typeof(MefAwareControlBuilder))]
    public abstract class MefUserControl : UserControl
    {
    }
}
