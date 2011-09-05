using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microsoft.ComponentModel.Composition.WebExtensions
{
    [ControlBuilder(typeof(MefAwareControlBuilder))]
    public abstract class MefWebControl : WebControl
    {
    }
}
