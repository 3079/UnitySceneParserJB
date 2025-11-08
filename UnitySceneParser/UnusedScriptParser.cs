using System.Text;

namespace JetBrainsInternship;

public sealed record Script(string path, string guid);

public class UnusedScriptParser
{
    private HashSet<string> _usedGuids = new(StringComparer.OrdinalIgnoreCase); 
    private List<Script> _unusedScripts = new();
    public IReadOnlySet<string> UsedGuids => _usedGuids;
    
    public void Observe(UnityYamlDoc d)
    {
        if (d.ClassId != 114) return;
        var guid = YamlHelpers.GetGuid(d.Body, "m_Script");
        if (!string.IsNullOrEmpty(guid)) _usedGuids.Add(guid!);
    }

    
    public void CalculateUnused(List<Script> allScripts)
    {
        _unusedScripts = allScripts.Where(x => !_usedGuids.Contains(x.guid)).ToList();
    }
    
    public void Dump(string inputPath, TextWriter writer)
    {
        writer.WriteLine("Relative Path,GUID");
        foreach (var script in _unusedScripts)
        {
            var relativePath = Path.GetRelativePath(inputPath, script.path);
            writer.WriteLine($"{relativePath},{script.guid}");
        }
    }
    
    public void DumpToFile(string inputPath, string outputPath)
    {
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        // UTF-8 without BOM is fine; change if they require BOM
        using var writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        Dump(inputPath, writer);
    }
}