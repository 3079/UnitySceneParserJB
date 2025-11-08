using JetBrainsInternship;

if (args.Length != 2)
{
    Console.Error.WriteLine("Please specify both the Unity project path as well as the output directory");
    return;
}

var inputPath = args[0];
var outputPath = args[1];

var hierarchy = new HierarchyDump();
var unusedScripts = new UnusedScriptParser();

var scenePaths = YamlScanner.Enumerate(inputPath);
var allScripts = ScriptScanner.CollectScripts(inputPath);

foreach (var scenePath in scenePaths)
{
    hierarchy.BeginScene(scenePath);
    
    var sceneName = Path.GetFileNameWithoutExtension(scenePath);
    var hierarchyDumpPath = Path.Combine(outputPath, $"{sceneName}.dump");
    
    foreach (var doc in UnityYamlParser.ParseFile(scenePath))
    {
        hierarchy.Observe(doc);
        unusedScripts.Observe(doc);
    }
    
    hierarchy.EndAndDumpScene(hierarchyDumpPath);
}

unusedScripts.CalculateUnused(allScripts);
var unusedScriptsDumpPath = Path.Combine(outputPath, "UnusedScripts.txt");
unusedScripts.DumpToFile(inputPath, unusedScriptsDumpPath);
