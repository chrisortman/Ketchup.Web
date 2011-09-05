using System;

namespace Ketchup.Web.Mef.Extensions
{
    /// <summary>
    /// Assembly level attribute that points to a discovery implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DiscoveryAttribute : Attribute
    {
        public DiscoveryAttribute(Type discovery)
        {
            if (!typeof(IDiscovery).IsAssignableFrom(discovery))
            {
                throw new ArgumentException("discovery type must implement IDiscovery", "discovery");
            }

            this.DiscoveryMethod = discovery;
        }

        public Type DiscoveryMethod { get; private set; }
    }
}
