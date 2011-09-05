using System;
using System.CodeDom;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Web.UI;

namespace Microsoft.ComponentModel.Composition.WebExtensions
{
    public class MefAwareControlBuilder : ControlBuilder
    {
        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, 
            CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, 
            CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
        {
            CodeAssignStatement assignment = null;

            foreach(CodeStatement statement in buildMethod.Statements)
            {
                // looks for assignment:
                // @__ctrl = new global::WebFormsWithMef.Controls.HelloControl();

                assignment = statement as CodeAssignStatement;

                if (assignment != null) break;
            }

            if (assignment == null) // "can't" happen
                throw new ArgumentException();

            ReplaceAssignmentByContainerCall(buildMethod, assignment);
        }

        private void ReplaceAssignmentByContainerCall(CodeMemberMethod buildMethod, CodeAssignStatement assignment)
        {
            var typeRef = ((CodeObjectCreateExpression)assignment.Right).CreateType;
            var i = 0;

            // TypeCatalog catalog;
            buildMethod.Statements.Insert(i++,
                new CodeVariableDeclarationStatement(typeof(ComposablePartCatalog), "catalog"));

            // CompositionContainer container;
            buildMethod.Statements.Insert(i++,
                new CodeVariableDeclarationStatement(typeof(CompositionContainer), "container"));

            // catalog = new TypeCatalog();
            buildMethod.Statements.Insert(i++, 
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("catalog"),
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(ControlUtility)), "CreateCatalog"),
                        new CodeTypeOfExpression(typeRef))));

            // var container = new CompositionContainer(catalog, EnvironmentContainerAccessor.Container);
            buildMethod.Statements.Insert(i++, 
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("container"),
                    new CodeObjectCreateExpression(typeof(CompositionContainer),
                                                    new CodeVariableReferenceExpression("catalog"),
                                                    new CodePropertyReferenceExpression(
                                                        new CodeTypeReferenceExpression(typeof(ControlUtility)), "Container"))));

             buildMethod.Statements.Insert(i++, 
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("catalog"),
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(ControlUtility)), "InspectContainer"),
                        new CodeVariableReferenceExpression("container"))));

            // @__ctrl = container.GetExportedValue<Hello>();
            var index = buildMethod.Statements.IndexOf(assignment);
            buildMethod.Statements.Remove(assignment);

            buildMethod.Statements.Insert(index, 
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("@__ctrl"),
                    new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(
                                                    new CodeVariableReferenceExpression("container"), "GetExportedValue", new CodeTypeReference(typeRef.BaseType)))
                    ));
        }
    }
}
