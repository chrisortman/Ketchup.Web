using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Configuration;
using System.Linq;

namespace Ketchup.Web.Mef
{
    public class ConfigFileExportProvider : ExportProvider
    {
        private List<StringValueExportDefinition> _definitions;

        private class StringValueExportDefinition : ExportDefinition
        {
            public StringValueExportDefinition(string contractName,string value) : 
                base(contractName,new Dictionary<string, object>() {{CompositionConstants.ExportTypeIdentityMetadataName,AttributedModelServices.GetTypeIdentity(typeof(string))}})
            {
                Value = value;
            }

            public string Value { get; set; }
        }
        public ConfigFileExportProvider(string settingPrefix = "")
        {
            _definitions =new List<StringValueExportDefinition>();

            Func<string, bool> doesSettingApply = settingName =>
            {
                if (settingPrefix == "")
                {
                    return true;
                }
                else
                {
                    return settingName.StartsWith(settingPrefix);
                }
            };

            foreach (var setting in ConfigurationManager.AppSettings.AllKeys)
            {
                if (doesSettingApply(setting))
                {
                    _definitions.Add(
                        new StringValueExportDefinition(contractName: setting.Substring(settingPrefix.Length),
                                                        value: ConfigurationManager.AppSettings[setting]));
                }
            }
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            var constraint = definition.Constraint.Compile();
            var foundExports = from d in _definitions
                               where constraint(d)
                               select new Export(d, () => d.Value);

            if (definition.Cardinality == ImportCardinality.ZeroOrMore)
            {
                return foundExports.ToList();
            }
            else if (foundExports.Count() == 1)
            {
                return foundExports.ToList();
            }
            else
            {
                return Enumerable.Empty<Export>();
            }
        }
    }
}