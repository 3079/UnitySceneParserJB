namespace JetBrainsInternship;

public class ScriptScanner
{
    public static IEnumerable<Script> Enumerate(string projectPath)
    {
        foreach (var meta in Directory.EnumerateFiles(projectPath, "*.cs.meta", SearchOption.AllDirectories))
        {
            string? guid = null;

            foreach (var line in File.ReadLines(meta))
            {
                if (line.StartsWith("guid:", StringComparison.Ordinal))
                {
                    guid = line.Substring("guid:".Length).Trim();
                    break;
                }
            }
            if (string.IsNullOrEmpty(guid)) continue;
            var csPath = meta.Replace(".cs.meta", ".cs", StringComparison.OrdinalIgnoreCase);
            if (File.Exists(csPath))
                yield return new Script(csPath, guid);       
        }
    }

    public static List<Script> CollectScripts(string projectPath)
    {
        return Enumerate(projectPath).ToList();
    }
}