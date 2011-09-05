using System.Web.UI;
using System.Web.UI.WebControls;

namespace Ketchup.Web.Mef.Web
{
    [ControlBuilder(typeof(MefAwareControlBuilder))]
    public abstract class MefWebControl : WebControl
    {
    }
}
