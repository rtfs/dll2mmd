using DiagramGenerator.ClassGraph;
using System.CommandLine;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var outputOption = new Option<FileInfo?>(
        aliases: new[] {"--output", "-o"},
        description: "Output file.",
        getDefaultValue: () => new FileInfo("output.md"));
        outputOption.AddAlias("-o");

        var nsOption = new Option<IList<string>>(
                    aliases: new[] {"--namespace", "-ns"},
                    description: "Namespace from which to fetch classes.",
                    getDefaultValue: () => new List<string>())
        {
            AllowMultipleArgumentsPerToken = true
        };

        var filesOption = new Option<IList<string>>(
                    aliases: new[] {"--files", "-f"},
                    description: "Dll files from which to fetch classes.",
                    // isDefault: true,
                    parseArgument: result =>
                    {
                        var ret = new List<string>();
                        foreach (var token in result.Tokens)
                        {
                            string? filePath = token.Value;
                            if (!File.Exists(filePath))
                            {
                                result.ErrorMessage = $"File does not exist {filePath}";
                                return new List<string>();
                            }
                            else
                            {
                                ret.Add(filePath);
                            }
                        }
                        return ret;
                    }
                    )
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };

        var tnOption = new Option<IList<string>>(
                    aliases: new[] {"--type-names", "-t"},
                    description: "Name of specific classes to form the diagram.",
                    getDefaultValue: () => new List<string>())
        {
            AllowMultipleArgumentsPerToken = true
        };

        var ignoreDepencencyOption = new Option<bool>(
                    name: "--ignore-dependency",
                    description: "If true, only dependency of inheritance and implementation would be generated.");

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

        void Execute(FileInfo outputFile, IList<string> nsList, IList<string> files, IList<string> tnList, bool ignoreDependency)
        {
            string output = outputFile?.FullName ?? string.Empty;
            var builder = new CsGraphBuilder();
            var graph = builder.Build(files, nsList, tnList, ignoreDependency);
            var generator = new MermaidGenerator();
            var text = generator.Generate(graph);
            File.WriteAllText(output, text);
        }
    }
}