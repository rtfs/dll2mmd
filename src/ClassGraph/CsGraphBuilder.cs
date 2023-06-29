using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace DiagramGenerator.ClassGraph;

public class CsGraphBuilder : IGraphBuilder
{
    public Graph Build(IEnumerable<string> files, IEnumerable<string> nsList, IEnumerable<string> typenameList, bool inheritanceOnly)
    {

        var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
        Console.WriteLine($"Loading core assemblies from: {Environment.NewLine}{runtimeDirectory}");
        string[] runtimeAssemblies = Directory.GetFiles(runtimeDirectory, "*.dll");
        IList<string> allPath = new List<string>(runtimeAssemblies);

        foreach (var file in files)
        {
            var pathFile = file;
            if (!Path.IsPathRooted(pathFile))
                pathFile = Path.GetFullPath(pathFile);
            string[] modelAssemblies = Directory.GetFiles(Path.GetDirectoryName(pathFile)!, "*.dll");
            Console.WriteLine($"Following libraries will be scanned: {Environment.NewLine}{string.Join(Environment.NewLine, modelAssemblies)}{Environment.NewLine}");
            allPath = allPath.Union(modelAssemblies).ToList();
        }

        var resolver = new PathAssemblyResolver(allPath);
        using var mlc = new MetadataLoadContext(resolver);

        var types = GetTypes(files, nsList, typenameList, mlc);

        var ret = new Graph();
        foreach (var type in types)
        {
            Console.WriteLine(type.Name);
            var c = BuildClass(type);
            ret.AddClass(c);
        }
        ret.RebuildRelation(inheritanceOnly);
        return ret;
    }

    private Class BuildClass(Type type)
    {
        var c = new Class(type.Name.TrimEnd('`', '1', '2', '3'))
        {
            IsInterface = type.IsInterface,
            BaseType = type.BaseType?.Name.TrimEnd('`', '1', '2', '3'), //trim end '`1' of generic type name. e.g. IList`1
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
                    typeArgs.Add(argType.Name.TrimEnd('`', '1', '2', '3'));
                }
                m.GenericType = method.ReturnType.Name.TrimEnd('`', '1', '2', '3'); //trim end '`1' of generic type name. e.g. IList`1
                m.TypeParams = typeArgs;
            }
            else
            {
                m.Type = method.ReturnType.Name.TrimEnd('`', '1', '2', '3'); //trim end '`1' of generic type name. e.g. IList`1
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
                    typeArgs.Add(argType.Name.TrimEnd('`', '1', '2', '3'));
                }
                p.GenericType = prop.PropertyType.Name.TrimEnd('`', '1', '2', '3'); //trim end '`1' of generic type name. e.g. IList`1
                p.TypeParams = typeArgs;
            }
            else
            {
                p.Type = prop.PropertyType.Name.TrimEnd('`', '1', '2', '3'); //trim end '`1' of generic type name. e.g. IList`1
            }
            ret.Add(p);
        }
        return ret;
    }

    private IList<Type> GetTypes(IEnumerable<string> files, IEnumerable<string> nsList, IEnumerable<string> typenameList, MetadataLoadContext mlc)
    {
        var types = new List<Type>();
        // modules = new List<string>();
        foreach (var file in files)
        {
            // var ass = Assembly.LoadFrom(pathFile);
            // var ass = AssemblyLoadContext.Default.LoadFromAssemblyPath(pathFile);
            var pathFile = file;
            if (!Path.IsPathRooted(pathFile))
                pathFile = Path.GetFullPath(pathFile);
            Assembly ass = mlc.LoadFromAssemblyPath(pathFile);

            if (!nsList.Any() && !typenameList.Any())
            {
                foreach (var type in ass.GetExportedTypes())
                {
                    if (type.IsClass || type.IsInterface)
                    {
                        types.Add(type);
                    }
                }
            }
            else
            {
                foreach (var type in ass.ExportedTypes)
                {
                    if ((nsList.Contains(type.Namespace) || typenameList.Contains(type.Name)) && (type.IsClass || type.IsInterface))
                    {
                        types.Add(type);
                    }
                }
            }
        }
        return types;
    }
}