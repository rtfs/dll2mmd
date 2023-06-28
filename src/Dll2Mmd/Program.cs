using DiagramGenerator.ClassGraph;
using System.CommandLine;


class Program
{
    static async Task<int> Main(string[] args)
    {
        var outputOption = new Option<FileInfo?>(
                    name: "--output",
                    description: "Output file.",
                    getDefaultValue: () => new FileInfo("output.md"));
        outputOption.AddAlias("-o");

        var nsOption = new Option<IList<string>>(
                    name: "--namespace",
                    description: "Namespace from which to fetch classes.",
                    getDefaultValue: () => new List<string>())
        {
            AllowMultipleArgumentsPerToken = true
        };
        nsOption.AddAlias("-ns");

        var filesOption = new Option<IList<string>>(
                    name: "--files",
                    description: "Dll files from which to fetch classes.")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };
        filesOption.AddAlias("-f");

        var tnOption = new Option<IList<string>>(
                    name: "--type-names",
                    description: "Name of specific classes to form the diagram.",
                    getDefaultValue: () => new List<string>())
        {
            AllowMultipleArgumentsPerToken = true
        };
        tnOption.AddAlias("-t");

        var ignoreDepencencyOption = new Option<bool>(
                    name: "--ignore-dependency",
                    description: "If true, only dependency of inheritance and implementation would be generating.");
        // getDefaultValue: () => true);
        // isDefault: true,
        // parseArgument: result =>
        //     {
        //         if (result.Tokens.Count == 0)
        //         {
        //             return false;

        //         }
        //         else
        //         {
        //             return true;
        //         }
        //     });

        var rootCommand = new RootCommand("Generate mermaid.js class-diagram from .net dll files.");
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(nsOption);
        rootCommand.AddOption(filesOption);
        rootCommand.AddOption(tnOption);
        rootCommand.AddOption(ignoreDepencencyOption);

        rootCommand.SetHandler((output, ns, files, tns, ignoreDep) =>
            {
                Execute(output!, ns, files, tns, ignoreDep);
            },
            outputOption, nsOption, filesOption, tnOption, ignoreDepencencyOption);

        return await rootCommand.InvokeAsync(args);
    }

    static void Execute(FileInfo outputFile, IList<string> nsList, IList<string> files, IList<string> tnList, bool ignoreDependency)
    {
        foreach (var fn in files)
        {
            Console.WriteLine(fn);
        }
        foreach (var ns in nsList)
        {
            Console.WriteLine(ns);
        }
        foreach (var tn in tnList)
        {
            Console.WriteLine(tn);
        }

        string output = outputFile?.FullName ?? string.Empty;
        var builder = new CsGraphBuilder();
        var graph = builder.Build(files, nsList, tnList, ignoreDependency);
        var generator = new MermaidGenerator();
        var text = generator.Generate(graph);
        File.WriteAllText(output, text);
    }
}