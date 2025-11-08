using YamlDotNet.RepresentationModel;

namespace JetBrainsInternship;

public static class UnityYamlParser
{
    public static IEnumerable<UnityYamlDoc> ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
            yield break;
        
        var input = new StreamReader(filePath);
        var yaml = new YamlStream();
        try
        {
            yaml.Load(input);
        }
        catch
        {
            yield break;
        }

        foreach (var doc in yaml.Documents)
        {
            if (doc.RootNode is not YamlMappingNode root) 
                continue;
            
            var tag = root.Tag.ToString();
            var anchor = root.Anchor.ToString();
            
            if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(anchor)) 
                continue;
            
            if (!int.TryParse(tag.Split(':').Last(), out var classId))
                continue;
            
            if (!long.TryParse(anchor, out var fileId))
                continue;

            var top = root.Children.FirstOrDefault();
            if (top.Value is not YamlMappingNode body)
                continue;
            
            var result = new UnityYamlDoc(classId, fileId, body);
            yield return result;
        }
    }
}
