using System.Reflection;

namespace DiagramGenerator.ClassGraph;

public class CsGraphBuilder : IGraphBuilder
{
    public Graph Build(IEnumerable<string> files, IEnumerable<string> nsList, IEnumerable<string> typenameList, bool inheritanceOnly)
    {
        var types = GetTypes(files, nsList, typenameList);

        var ret = new Graph();
        foreach (var type in types)
        {
            var c = BuildClass(type);
            ret.AddClass(c);
        }
        ret.RebuildRelation(inheritanceOnly);
        return ret;
    }

    private Class BuildClass(Type type)
    {
        var c = new Class(type.Name)
        {
            IsInterface = type.IsInterface,
            BaseType = type.BaseType?.Name,
            ImplementedInterface = type.GetTypeInfo().ImplementedInterfaces.Select(o => o.Name).ToList()
        };

        // properties
        var pList = BuildProperties(type);
        foreach (var p in pList)
        {
            c.AddProperty(p);
        }

        // methods
        var mList = BuildMethods(type);
        foreach (var m in mList)
        {
            c.AddMethod(m);
        }
        return c;
    }
    private IEnumerable<Method> BuildMethods(Type type)
    {
        var ret = new List<Method>();
        foreach (var method in type.GetMethods())
        {
            if (method.IsSpecialName || !method.IsPublic || method.Module != type.Module)
                continue;
            var m = new Method(method.Name, Visibility.Public);
            if (method.ReturnType.IsGenericType)
            {
                var typeArgs = new List<string>();
                foreach (var argType in method.ReturnType.GenericTypeArguments)
                {
                    typeArgs.Add(argType.Name);
                }
                m.GenericType = method.ReturnType.Name;
                m.TypeParams = typeArgs;
            }
            else
            {
                m.Type = method.ReturnType.Name;
            }
            ret.Add(m);
        }
        return ret;
    }

    private IEnumerable<Property> BuildProperties(Type type)
    {
        var ret = new List<Property>();
        foreach (var prop in type.GetProperties())
        {
            var p = new Property(prop.Name, Visibility.Public);
            if (prop.PropertyType.IsGenericType)
            {
                var typeArgs = new List<string>();
                foreach (var argType in prop.PropertyType.GenericTypeArguments)
                {
                    typeArgs.Add(argType.Name);
                }
                p.GenericType = prop.PropertyType.Name;
                p.TypeParams = typeArgs;
            }
            else
            {
                p.Type = prop.PropertyType.Name;
            }
            ret.Add(p);
        }
        return ret;
    }

    private IList<Type> GetTypes(IEnumerable<string> files, IEnumerable<string> nsList, IEnumerable<string> typenameList)
    {
        var types = new List<Type>();
        // modules = new List<string>();
        foreach (var file in files)
        {
            var ass = Assembly.LoadFile(file);
            foreach (var type in ass.ExportedTypes)
            {
                if ((nsList.Contains(type.Namespace) || typenameList.Contains(type.Name)) && (type.IsClass || type.IsInterface))
                {
                    types.Add(type);
                }
            }
        }
        return types;
    }
}