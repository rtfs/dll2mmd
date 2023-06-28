using DiagramGenerator.ClassGraph;

var files = new List<string>{
    @"d:\projects\src\tools\Cs2Diagram\src\ClassGraph\bin\Debug\net6.0\ClassGraph.dll"
};
var nsList = new List<string>{
    "DiagramGenerator.ClassGraph"
};
var tnList = new List<string>();
bool ignoreDependency = true;
string output = "./output.md";
var builder = new CsGraphBuilder();
var graph = builder.Build(files, nsList, tnList, ignoreDependency);
var generator = new MermaidGenerator();
var text = generator.Generate(graph);
File.WriteAllText(output, text);