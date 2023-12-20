using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DanmakuEngine.SourceGeneration;

public static class GeneratorHelper
{
    private static readonly ImmutableDictionary<Accessibility, string> accessbilityMap = new Dictionary<Accessibility, string>()
    {
        { Accessibility.NotApplicable, "" },
        { Accessibility.Public, "public" },
        { Accessibility.Private, "private" },
        { Accessibility.Internal, "internal" },
        { Accessibility.Protected, "protected" },
    }.ToImmutableDictionary();

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

    public static IEnumerable<INamedTypeSymbol> GetAllSubClasses(this INamespaceSymbol ns)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol nestedNs)
            {
                foreach (var c in nestedNs.GetAllSubClasses())
                {
                    yield return c;
                }
            }
            else if (member is INamedTypeSymbol classSymbol)
            {
                yield return classSymbol;
            }
        }
    }
}
