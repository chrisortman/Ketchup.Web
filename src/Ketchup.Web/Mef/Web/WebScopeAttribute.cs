using System;
using System.ComponentModel.Composition;

namespace Ketchup.Web.Mef.Web
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [MetadataAttribute]
    public class WebScopeAttribute : Attribute
    {
        public WebScopeAttribute(WebScopeMode mode)
        {
            Mode = mode;
        }

        public WebScopeMode Mode { get; private set; }
    }
}