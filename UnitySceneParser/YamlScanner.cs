namespace JetBrainsInternship;

public static class YamlScanner
{
    public static IEnumerable<string> Enumerate(string projectPath)
    {
        return Directory.EnumerateFiles(projectPath, "*.unity", SearchOption.AllDirectories);
    }
}