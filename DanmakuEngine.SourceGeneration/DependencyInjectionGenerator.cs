using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;

namespace DanmakuEngine.SourceGeneration
{
    [Generator(LanguageNames.CSharp)]
    public class DependencyInjectionGenerator : ISourceGenerator
    {
        private const string iinjectable_interface = @"IInjectable";

        private const string inject_attribute = @"InjectAttribute";

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        const string name_extension = @"Injection";

        const string usings = @"// <auto-generated/>

// if you encounter errors saying that 'another partial declaration of this type exists'
// You should check if you have declared the class as partial 

using DanmakuEngine.Dependency;
";

        const string inject_method_name = @"AutoInject";

        const string injection_method_virtual = $@"public virtual void {inject_method_name}()";

        const string injection_method_override = $@"public override void {inject_method_name}()";

        private string CodeForGetByType(string member, ITypeSymbol type)
        {
            var fullyQualifiedTypeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return $@"{member} = DependencyContainer.Get<{fullyQualifiedTypeName}>();";
        }

        private string HandleClass(INamedTypeSymbol classSymbol, List<ISymbol> members)
        {
            // if (!classSymbol.CanBeReferencedByName)
            // {
            //     throw new Exception($"The class {classSymbol.Name} must be declared as partial.");
            // }

            string nsName = classSymbol.ContainingNamespace.ToDisplayString();
            string className = classSymbol.Name;

            string nsDeclaration = $"namespace {nsName}";

            string abstract_modifier = classSymbol.IsAbstract ? "abstract " : "";
            string static_modifier = classSymbol.IsStatic ? "static " : "";

            string injection_method;
            bool isOverride = false;
            if (classSymbol.BaseType is not null && classSymbol.BaseType.GetMembers(inject_method_name).Any())
            {
                injection_method = injection_method_override;
                isOverride = true;
            }
            else
            {
                injection_method = injection_method_virtual;
            }

            StringBuilder codeBuilder = new StringBuilder();
            codeBuilder.AppendLine(usings);
            codeBuilder.AppendLine($"{nsDeclaration}");
            codeBuilder.AppendLine($"{{");
            codeBuilder.AppendLine($"    " + $"{abstract_modifier} {static_modifier} partial class {className} : {iinjectable_interface}".TrimStart());
            codeBuilder.AppendLine($"    {{");
            codeBuilder.AppendLine($"        {injection_method}");
            codeBuilder.AppendLine($"        {{");

            string indent = GeneratorHelper.GetIndent(4, 3);

            // inject base class
            if (isOverride)
            {
                codeBuilder.AppendLine(indent + $"base.{inject_method_name}();");

                codeBuilder.AppendLine();

                codeBuilder.AppendLine($"#if DEBUG");
                codeBuilder.AppendLine(indent + $"DanmakuEngine.Logging.Logger.Debug(\"[+] [SGDI] Injected base class for {classSymbol.ToDisplayString()}\");");
                codeBuilder.AppendLine($"#endif");

                codeBuilder.AppendLine();
            }

            int injectedMembers = 0;

            foreach (var m in members)
            {
                if (HandleMember(m, codeBuilder, indent))
                    injectedMembers++;
            }

            codeBuilder.AppendLine("#if DEBUG");

            codeBuilder.AppendLine(indent + $"DanmakuEngine.Logging.Logger.Debug(\"[+] [SGDI] Injected {injectedMembers} members into {classSymbol.ToDisplayString()}\");");

            codeBuilder.AppendLine("#endif");

            // end for injection method
            codeBuilder.AppendLine($"        }}");
            // end for class declaration
            codeBuilder.AppendLine($"    }}");
            // end for namespace declaration
            codeBuilder.AppendLine($"}}");

            return codeBuilder.ToString();
        }

        private bool HandleMember(ISymbol m, StringBuilder codeBuilder, string indent)
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
                return false;

            codeBuilder.Append(indent);

            var code = CodeForGetByType(memberName, typeName);

            codeBuilder.AppendLine(code);

            return true;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var topNamespace = context.Compilation.Assembly.GlobalNamespace;

            var classes = topNamespace.GetAllSubClasses();

            foreach (var classSymbol in classes)
            {
                var membersToInject = classSymbol.GetMembers()
                        .Where(m => (m is IFieldSymbol || m is IPropertySymbol)
                            && m.GetAttributes().Any(a => a.AttributeClass!.Name == inject_attribute))
                        .ToList();

                var hasIInjectableInterface = classSymbol.AllInterfaces.Any(i => i.Name == iinjectable_interface);

                // we don't want to inject interfaces
                var isInterface = classSymbol.TypeKind == TypeKind.Interface;

                if ((hasIInjectableInterface && !isInterface) || membersToInject.Any())
                {
                    var code = HandleClass(classSymbol, membersToInject);

                    context.AddSource($"{classSymbol.GetClassNameWithNamespace()}_{name_extension}.g.cs", SourceText.From(code, Encoding.UTF8));
                }
            }
        }
    }
}