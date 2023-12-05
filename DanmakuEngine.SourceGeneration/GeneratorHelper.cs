using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DanmakuEngine.SourceGeneration;

public static class GeneratorHelper
{
    private static readonly Dictionary<Accessibility, string> accessbilityMap = new Dictionary<Accessibility, string>()
    {
        { Accessibility.NotApplicable, "" },
        { Accessibility.Public, "public" },
        { Accessibility.Private, "private" },
        { Accessibility.Internal, "internal" },
        { Accessibility.Protected, "protected" },
    };

    public static string AccessibilityToString(Accessibility accessibility)
    {
        if (accessbilityMap.TryGetValue(accessibility, out string str))
        {
            return str;
        }

        return "private";
    }

    public static string GetIndent(int spaces, int count)
        => new(' ', spaces * count);

    public static string GetClassNameWithNamespace(this INamedTypeSymbol classSymbol)
    {
        string nsName = classSymbol.ContainingNamespace.ToDisplayString();
        string className = classSymbol.Name;

        return $"{nsName}.{className}";
    }
}