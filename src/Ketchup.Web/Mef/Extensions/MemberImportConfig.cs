using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Reflection;
using Ketchup.Web.Mef.Extensions.Internal;

namespace Ketchup.Web.Mef.Extensions
{
    public class MemberImportConfig
    {
        private MemberImportConfig(Type memberType)
        {
            var elementType = memberType.GetCollectionElementType();

            if (elementType != null)
            {
                this.ContractName = AttributedModelServices.GetContractName(elementType);
                this.RequiredTypeIdentity = AttributedModelServices.GetTypeIdentity(elementType);
            }
            else
            {
                this.ContractName = AttributedModelServices.GetContractName(memberType);
                this.RequiredTypeIdentity = AttributedModelServices.GetTypeIdentity(memberType);
            }

            this.RequiredMetadata = new List<KeyValuePair<string, Type>>();
        }

        internal MemberImportConfig(ParameterInfo paramInfo)
            : this(paramInfo.ParameterType)
        {
            this.Target = paramInfo.Member;
        }

        internal MemberImportConfig(MemberInfo member)
            : this(member.UnderlyingMemberType())
        {
            this.Target = member;
        }

        internal MemberInfo Target { get; private set; }
        internal string ContractName { get; private set; }
        internal string RequiredTypeIdentity { get; private set; }
        internal List<KeyValuePair<string,Type>> RequiredMetadata { get; private set; }
        internal ImportCardinality Cardinality { get; private set; }
        internal CreationPolicy RequiredCreationPolicy { get; private set; }
        internal bool IsRecomposable { get; private set; }

    	public MemberImportConfig Contract(MemberInfo member)
        {
            Type memberType = member.UnderlyingMemberType();
            this.Contract(AttributedModelServices.GetContractName(memberType));
            return this;
        }

    	public MemberImportConfig Contract(string name)
        {
            this.ContractName = name;
            return this;
        }

        public MemberImportConfig ContractType(Type type)
        {
            this.RequiredTypeIdentity = AttributedModelServices.GetTypeIdentity(type);
            return this;
        }

    	public MemberImportConfig RequiresMetadata(string key, Type metadataType)
        {
            RequiredMetadata.Add(new KeyValuePair<string, Type>(key, metadataType));
            return this;
        }

    	public MemberImportConfig RequiresCreationPolicy(CreationPolicy cp)
        {
            RequiredCreationPolicy = cp;
            return this;
        }

    	public MemberImportConfig Recomposable()
        {
            IsRecomposable = true;
            return this;
        }

        public MemberImportConfig Required()
        {
            Cardinality = ImportCardinality.ExactlyOne;
            return this;
        }

        public MemberImportConfig Optional()
        {
            Cardinality = ImportCardinality.ZeroOrOne;
            return this;
        }

        public MemberImportConfig Many()
        {
            Cardinality = ImportCardinality.ZeroOrMore;
            return this;
        }
    }
}
