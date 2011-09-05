using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq.Expressions;

namespace Ketchup.Web.Mef.Extensions
{
    public class NonGenericSinglePartRegistration : BasePartRegistration<NonGenericSinglePartRegistration>
    {
        private Type _implementation;

        public NonGenericSinglePartRegistration(Type implementation)
        {
            _implementation = implementation;
        }

        public NonGenericSinglePartRegistration Exporting<TService>()
        {
            var expConfig = CreateMemberExportConfig(_implementation, typeof(TService));

            this.ExportConfigs.Add(expConfig);
            
            return this;
        }

        public NonGenericSinglePartRegistration Exporting<TService>( Action<MemberExportConfig, Type> config)
        {
            var expConfig = CreateMemberExportConfig(_implementation, typeof(TService));

            config(expConfig, _implementation);

            this.ExportConfigs.Add(expConfig);

            return this;
        }

        //public NonGenericSinglePartRegistration ImportingMember(Type implementation,
        //    Expression<Func<TImplementation, object>> propertyOrField)
        //{
        //    var importConfig = CreateMemberImportConfig(propertyOrField);

        //    ImportConfigs.Add(importConfig);

        //    return this;
        //}

        //public SinglePartRegistration<TImplementation> ImportingMember(
        //    Expression<Func<TImplementation, object>> propertyOrField,
        //    Action<MemberImportConfig> config)
        //{
        //    var importConfig = CreateMemberImportConfig(propertyOrField);

        //    config(importConfig);
            
        //    ImportConfigs.Add(importConfig);

        //    return this;
        //}

        internal override bool IsMultipleRegistration { get { return false; } }

        internal override ComposablePartDefinition CreatePartFor(Type type, Convention convention)
        {
            if (type != null)
                throw new ArgumentException("Type must be null", "type");

            type = _implementation;

            var partType = _implementation;
            bool isDisposalRequired = typeof(IDisposable).IsAssignableFrom(partType);
            var exports = new List<ExportDefinition>();
            var imports = new List<ImportDefinition>();
            var metadata = new Dictionary<string, object>();

            CreateExportDefinitions(type, convention, exports);
            CreateImportDefinitions(type, convention, imports);

            return ReflectionModelServices.CreatePartDefinition(
                new Lazy<Type>(() => partType),
                isDisposalRequired,
                new Lazy<IEnumerable<ImportDefinition>>(() => imports),
                new Lazy<IEnumerable<ExportDefinition>>(() => exports),
                new Lazy<IDictionary<string, object>>(() => metadata),
                null);
        }

        internal override NonGenericSinglePartRegistration GetMostDerived()
        {
            return this;
        }

        public override bool IsFor(Type implementation)
        {
            return _implementation == implementation;
        }
    }
    public class SinglePartRegistration<TImplementation> : BasePartRegistration<SinglePartRegistration<TImplementation>>
    {
        public SinglePartRegistration<TImplementation> Exporting<TService>()
        {
            var expConfig = CreateMemberExportConfig(typeof(TImplementation), typeof(TService));

            this.ExportConfigs.Add(expConfig);

            return this;
        }

        public override bool IsFor(Type implementation)
        {
            return typeof (TImplementation) == implementation;
        }

        public SinglePartRegistration<TImplementation> Exporting<TService>(Action<MemberExportConfig, Type> config)
        {
            var expConfig = CreateMemberExportConfig(typeof(TImplementation), typeof(TService));

            config(expConfig, typeof(TImplementation));

            this.ExportConfigs.Add(expConfig);

            return this;
        }

        public SinglePartRegistration<TImplementation> ImportingMember(
            Expression<Func<TImplementation, object>> propertyOrField)
        {
            var importConfig = CreateMemberImportConfig(propertyOrField);

            ImportConfigs.Add(importConfig);

            return this;
        }

        public SinglePartRegistration<TImplementation> ImportingMember(
            Expression<Func<TImplementation, object>> propertyOrField,
            Action<MemberImportConfig> config)
        {
            var importConfig = CreateMemberImportConfig(propertyOrField);

            config(importConfig);
            
            ImportConfigs.Add(importConfig);

            return this;
        }

        internal override bool IsMultipleRegistration { get { return false; } }

        internal override ComposablePartDefinition CreatePartFor(Type type, Convention convention)
        {
            if (type != null)
                throw new ArgumentException("Type must be null", "type");

            var partType = typeof(TImplementation);
            bool isDisposalRequired = typeof(IDisposable).IsAssignableFrom(partType);
            var exports = new List<ExportDefinition>();
            var imports = new List<ImportDefinition>();
            var metadata = new Dictionary<string, object>();

            CreateExportDefinitions(type, convention, exports);
            CreateImportDefinitions(type, convention, imports);

            return ReflectionModelServices.CreatePartDefinition(
                new Lazy<Type>(() => partType),
                isDisposalRequired,
                new Lazy<IEnumerable<ImportDefinition>>(() => imports),
                new Lazy<IEnumerable<ExportDefinition>>(() => exports),
                new Lazy<IDictionary<string, object>>(() => metadata),
                null);
        }

        internal override SinglePartRegistration<TImplementation> GetMostDerived()
        {
            return this;
        }
    }
}
