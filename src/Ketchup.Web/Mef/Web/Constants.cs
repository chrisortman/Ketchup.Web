using System.ComponentModel.Composition;
using System.Web.Mvc;

namespace Ketchup.Web.Mef.Web
{
    public static class Constants
    {
        public const string ControllerNameMetadataName = "Name";
        public const string ControllerNamespaceMetadataName = "controller_ns";
        public const string ExportedTypeIdentityMetadataName = "ExportTypeIdentity";
        public static readonly string ControllerContract = AttributedModelServices.GetContractName(typeof(IController));
        public static readonly string ControllerTypeIdentity = AttributedModelServices.GetTypeIdentity(typeof(IController));
    }
}
