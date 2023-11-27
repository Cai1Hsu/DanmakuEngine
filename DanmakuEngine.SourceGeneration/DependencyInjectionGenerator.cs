using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace DanmakuEngine.SourceGeneration
{
    [Generator(LanguageNames.CSharp)]
    public class DependencyInjectionGenerator : ISourceGenerator
    {
        const string usings = @"// <auto-generated/>
using DanmakuEngine.Dependency;
";

        const string injection_method = @"public void AutoInject()";

        private string CodeForGetByType(string member, ITypeSymbol type)
        {
            var fullyQualifiedTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return $@"{member} = DependencyContainer.Get<{fullyQualifiedTypeName}>();";
        }

        private string HandleClass(INamedTypeSymbol classSymbol, List<ISymbol> members)
        {
            if (!classSymbol.CanBeReferencedByName)
            {
                throw new Exception($"The class {classSymbol.Name} cannot be declared as partial.");
            }

            string nsName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;
            string accessibility = GeneratorHelper.AccessibilityToString(classSymbol.DeclaredAccessibility);

            string nsDeclaration = $"namespace {nsName}";

            string abstract_modifier = classSymbol.IsAbstract ? "abstract " : "";
            string static_modifier = classSymbol.IsStatic ? "static " : "";

            StringBuilder codeBuilder = new StringBuilder();
            codeBuilder.AppendLine(usings);
            codeBuilder.AppendLine($"{nsDeclaration}");
            codeBuilder.AppendLine($"{{");
            codeBuilder.AppendLine($"    {accessibility} {abstract_modifier} {static_modifier} partial class {className} : IInjectable");
            codeBuilder.AppendLine($"    {{");
            codeBuilder.AppendLine($"        {injection_method}");
            codeBuilder.AppendLine($"        {{");

            string indent = GeneratorHelper.GetIndent(4, 3);

            foreach (var m in members)
            {
                HandleMember(m, codeBuilder, indent);
            }

            // end for injection method
            codeBuilder.AppendLine($"        }}");
            // end for class declaration
            codeBuilder.AppendLine($"    }}");
            // end for namespace declaration
            codeBuilder.AppendLine($"}}");

            return codeBuilder.ToString();
        }

        private void HandleMember(ISymbol m, StringBuilder codeBuilder, string indent)
        {
            string memberName;
            ITypeSymbol typeName;

            if (m is IFieldSymbol field)
            {
                memberName = field.Name;
                typeName = field.Type;
            }
            else if (m is IPropertySymbol property)
            {
                memberName = property.Name;
                typeName = property.Type;
            }
            else
                return;

            codeBuilder.Append(indent);

            var code = CodeForGetByType(memberName, typeName);

            codeBuilder.AppendLine(code);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            HandleNamespace(context.Compilation.Assembly.GlobalNamespace, context);
        }

        private void HandleNamespace(INamespaceSymbol ns, GeneratorExecutionContext context)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol nestedNs)
                {
                    HandleNamespace(nestedNs, context);
                }
                else if (member is INamedTypeSymbol classSymbol)
                {
                    var membersToInject = classSymbol.GetMembers()
                        .Where(m => (m is IFieldSymbol || m is IPropertySymbol) && m.GetAttributes().Any(a => a.AttributeClass!.Name.Contains("Inject")))
                        .ToList();

                    if (membersToInject.Any())
                    {

                        var code = HandleClass(classSymbol, membersToInject);
                        context.AddSource($"{classSymbol.Name}_Injection.g.cs", SourceText.From(code, Encoding.UTF8));
                    }
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}