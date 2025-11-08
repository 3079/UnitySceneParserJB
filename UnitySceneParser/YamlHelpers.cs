using YamlDotNet.RepresentationModel;

namespace JetBrainsInternship;

public static class YamlHelpers
{
    public static string? GetString(YamlMappingNode map, string key)
        => map.Children.TryGetValue(new YamlScalarNode(key), out var v) ? ((YamlScalarNode)v).Value : null;

    public static int GetInt(YamlMappingNode map, string key)
        => int.TryParse(GetString(map, key), out var i) ? i : 0;
    
    public static long GetRef(YamlMappingNode map, string key)
    {
        if (!map.Children.TryGetValue(new YamlScalarNode(key), out var v)) return 0;
        var m = v as YamlMappingNode;
        if (m == null) return 0;
        if (!m.Children.TryGetValue(new YamlScalarNode("fileID"), out var fidNode)) return 0;
        return long.TryParse(((YamlScalarNode)fidNode).Value, out var fid) ? fid : 0;
    }

    public static List<long> GetRefList(YamlMappingNode map, string key)
    {
        var result = new List<long>();
        if (!map.Children.TryGetValue(new YamlScalarNode(key), out var v)) return result;
        if (v is YamlSequenceNode seq)
        {
            foreach (var item in seq)
                if (item is YamlMappingNode m && m.Children.TryGetValue(new YamlScalarNode("fileID"), out var fidNode)
                                              && long.TryParse(((YamlScalarNode)fidNode).Value, out var fid))
                    result.Add(fid);
        }
        return result;
    }

    public static string? GetGuid(YamlMappingNode map, string key)
    {
        if (!map.Children.TryGetValue(new YamlScalarNode(key), out var n)) return null;
        if (n is not YamlMappingNode m) return null;
        if (!m.Children.TryGetValue(new YamlScalarNode("guid"), out var guidNode)) return null;
        return (guidNode as YamlScalarNode)?.Value;
    }

    public static List<long> FindRoots(
        Dictionary<long, HierarchyDump.Transform> transforms,
        Dictionary<long, long> goToTransform,
        List<long> sceneRootRefs
    )
    {
        List<long> roots = new List<long>();
        
        // Prefer SceneRoots if present
        if (sceneRootRefs.Count > 0)
        {
            foreach (var id in sceneRootRefs)
            {
                if (transforms.ContainsKey(id))
                    roots.Add(id);
                else if (goToTransform.TryGetValue(id, out var tfId))
                    roots.Add(tfId);
            }
            
            if (roots.Count > 0)
                return roots.Distinct().ToList();
        }
        
        // Fallback: robust detection from parent/children links
        var allTransforms   = new HashSet<long>(transforms.Keys);
        var allChildrenRefs = new HashSet<long>(transforms.Values.SelectMany(t => t.children));
        roots = allTransforms
            .Where(id =>
            {
                var t = transforms[id];
                return !allChildrenRefs.Contains(id) || t.parentId == 0 || !transforms.ContainsKey(t.parentId);
            })
            .Distinct()
            .ToList();
        return roots;
    }
}