namespace DiagramGenerator.ClassGraph;

public class MermaidGenerator : IDiagramGenerator
{
    private static string MDFrame = 
@"```mermaid
classDiagram
{0}
{1}
```
";

    private static string ClassFrame = 
@"class {0}
{1}
{2}
";

    public string Generate(Graph graph)
    {
        string allClass = string.Empty;
        foreach (var @class in graph.Classes)
        {
            var classString = GenerateClass(@class);
            allClass += classString + "\r\n";
        }

        string allRelation = string.Empty;
        foreach (var relation in graph.Relations)
        {
            var relationString = GenerateRelation(relation);
            allRelation += relationString + "\r\n";
        }
        return string.Format(MDFrame, allClass, allRelation);
    }

    private string GenerateClass(Class @class)
    {
        var allProperties = string.Empty;
        foreach (var property in @class.Properties)
        {
            allProperties += GenerateClassProperty(@class.Name, property) + "\r\n";
        }

        var allMethods = string.Empty;
        foreach (var method in @class.Methods)
        {
            allMethods += GenerateClassMethod(@class.Name, method) + "\r\n";
        }
        return string.Format(ClassFrame, @class.Name, allProperties, allMethods);
    }

    private string GenerateClassProperty(string className, Property property)
    {
        var typeString = GetTypeString(property.Type, property.GenericType, property.TypeParams);
        var visibilityNotion = GetVisibilityNotion(property.MemberVisibility);
        return $"{className} : {visibilityNotion}{typeString} {property.Name}";
    }

    private string GetTypeString(string? type, string? genericType, IList<string> typeParams)
    {
        if (!string.IsNullOrEmpty(type))
            return type;
        return $"{genericType}~{typeParams.FirstOrDefault()}~";
    }

    private string GenerateClassMethod(string className, Method method)
    {
        var typeString = GetTypeString(method.Type, method.GenericType, method.TypeParams);
        var visibilityNotion = GetVisibilityNotion(method.MemberVisibility);
        return $"{className} : {visibilityNotion}{method.Name}() {typeString}";
    }

    private string GenerateRelation(ClassRelation relation)
    {
        var relationNotion = GetRelationNotion(relation.Type);
        return $"{relation.To.Name} {relationNotion} {relation.From.Name}";
    }

    private string GetRelationNotion(RelationType type)
    {
        switch (type)
        {
            case RelationType.Inheritance:
                return "<|--";
            case RelationType.Implementation:
                return "<|..";
            case RelationType.Dependency:
                return "<--";
            default:
                break;
        }
        return string.Empty;
    }

    private string GetVisibilityNotion(Visibility visibility)
    {
        switch (visibility)
        {
            case Visibility.Private:
                return "-";
            case Visibility.Protected:
                return "#";
            case Visibility.Public:
                return "+";
            case Visibility.Internal:
                return "~";
            default:
                break;
        }
        return string.Empty;
    }
}