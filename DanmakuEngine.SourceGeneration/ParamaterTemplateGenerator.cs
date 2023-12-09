using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DanmakuEngine.SourceGeneration;

[Generator(LanguageNames.CSharp)]
public class ParamaterTemplateGenerator : ISourceGenerator
{
    private const string paramaters_base_class = @"Paramaters";
    private const string argument_class = @"Argument";
    private const string description_attribute = @"DescriptionAttribute";
    private const string method_name = @"setupChildren";
    private const string name_extension = @"InitializeChildren";
    private const string usings = @"// <auto-generated/>
using DanmakuEngine.Arguments;
";
    private const string generated_method = $@"private void {method_name}()";

    public void Execute(GeneratorExecutionContext context)
    {
        var topNamespace = context.Compilation.Assembly.GlobalNamespace;
        var classes = topNamespace.GetAllSubClasses();

        foreach (var classSymbol in classes)
        {
            // ParamaterTemplates must be inherited from `Paramaters`
            var isParamaters = classSymbol.BaseType?.Name == paramaters_base_class;

            if (!isParamaters)
                continue;

            // Judge members in `HandleMembers`
            var membersToInit = classSymbol.GetMembers().ToList();

            var code = HandleClass(classSymbol, membersToInit);

            context.AddSource($"{classSymbol.GetClassNameWithNamespace()}_{name_extension}.g.cs", SourceText.From(code, Encoding.UTF8));

        }
    }

    private string HandleClass(INamedTypeSymbol classSymbol, List<ISymbol> members)
    {
        string nsName = classSymbol.ContainingNamespace.ToDisplayString();
        string className = classSymbol.Name;

        string nsDeclaration = $"namespace {nsName}";

        StringBuilder codeBuilder = new StringBuilder();
        codeBuilder.AppendLine(usings);
        codeBuilder.AppendLine($"{nsDeclaration}");
        codeBuilder.AppendLine($"{{");
        codeBuilder.AppendLine($"    partial class {className} : {paramaters_base_class}");
        codeBuilder.AppendLine($"    {{");
        codeBuilder.AppendLine($"        {generated_method}");
        codeBuilder.AppendLine($"        {{");

        codeBuilder.AppendLine($"            Children = new()");
        codeBuilder.AppendLine($"            {{");

        string indent = GeneratorHelper.GetIndent(4, 4);

        foreach (var m in members)
        {
            HandleMember(m, codeBuilder, indent);
        }

        codeBuilder.AppendLine($"            }};");

        codeBuilder.AppendLine($"");

        codeBuilder.AppendLine($"            System.Diagnostics.Debug.Assert(Children.Count != 0);");

        // end for generated method
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

        if (typeName.Name != argument_class)
            return;

        var description = (string)m.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass.Name == description_attribute)
            ?.ConstructorArguments[0].Value;

        if (string.IsNullOrEmpty(description))
        {
            description = $"Description not provided for {memberName}";
        }

        var code = $"{{ {memberName}, \"{description}\" }},";

        codeBuilder.Append(indent);
        codeBuilder.AppendLine(code);
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // ignore
    }
}
