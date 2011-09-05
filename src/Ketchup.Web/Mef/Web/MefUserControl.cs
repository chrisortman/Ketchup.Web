using System.Web.UI;

namespace Ketchup.Web.Mef.Web
{
    [ControlBuilder(typeof(MefAwareControlBuilder))]
    public abstract class MefUserControl : UserControl
    {
    }
}
