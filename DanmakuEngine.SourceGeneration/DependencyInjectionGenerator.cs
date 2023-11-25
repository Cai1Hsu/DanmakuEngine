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

        private readonly Dictionary<Accessibility, string> accessbilityMap = new Dictionary<Accessibility, string>()
        {
            { Accessibility.NotApplicable, "" },
            { Accessibility.Public, "public" },
            { Accessibility.Private, "private" },
            { Accessibility.Internal, "internal" },
            { Accessibility.Protected, "protected" },
        };

        private string AccessibilityToString(Accessibility accessibility)
        {
            if (accessbilityMap.TryGetValue(accessibility, out string str))
            {
                return str;
            }

            return "private";
        }

        private string GenerateMethodForInjection(INamedTypeSymbol classSymbol, List<ISymbol> members)
        {
            if (!classSymbol.CanBeReferencedByName)
            {
                throw new Exception($"The class {classSymbol.Name} cannot be declared as partial.");
            }

            string nsName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;
            string accessibility = AccessibilityToString(classSymbol.DeclaredAccessibility);

            string nsDeclaration = $"namespace {nsName}";

            string abstract_modifier = classSymbol.IsAbstract ? "abstract " : "";
            string static_modifier = classSymbol.IsStatic ? "static " : "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(usings);
            sb.AppendLine($"{nsDeclaration}");
            sb.AppendLine($"{{");
            sb.AppendLine($"    {accessibility} {abstract_modifier} {static_modifier} partial class {className} : IInjectable");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        {injection_method}");
            sb.AppendLine($"        {{");

            const string indent = "            ";

            foreach (var m in members)
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
                    continue;

                sb.Append(indent);

                var code = CodeForGetByType(memberName, typeName);

                sb.AppendLine(code);
            }

            // end for injection method
            sb.AppendLine($"        }}");
            // end for class declaration
            sb.AppendLine($"    }}");
            // end for namespace declaration
            sb.AppendLine($"}}");

            return sb.ToString();
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

                        var code = GenerateMethodForInjection(classSymbol, membersToInject);
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