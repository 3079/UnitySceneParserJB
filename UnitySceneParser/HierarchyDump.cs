using System.IO;
using System.Text;

namespace JetBrainsInternship;

public class HierarchyDump
{
    private readonly Dictionary<string, SceneData> _scenes = new();
    private SceneData? _currentScene;
    
    public void BeginScene(string scenePath)
    {
        _currentScene = new SceneData(scenePath);
        _scenes[scenePath] = _currentScene;
    }

    public sealed class SceneData
    {
        public string ScenePath { get; }
        public Dictionary<long, GameObject> GameObjects { get; } = new();
        public Dictionary<long, Transform> Transforms { get; } = new();
        public Dictionary<long,long> GameObjectToTransform { get; } = new();
        public List<long> SceneRootRefs { get; set; } = new();
        public List<long> Roots { get; set; } = new();
        
        public SceneData(string path) =>  ScenePath = path;
    }

    public void Observe(UnityYamlDoc doc)
    {
        if (_currentScene == null) return;
        
        switch (doc.ClassId)
        {
            case 1: // GameObject
            {
                _currentScene.GameObjects[doc.FileId] = new GameObject(
                    name: YamlHelpers.GetString(doc.Body, "m_Name") ?? "(Unnamed)"
                );
                break;
            }
            case 4: // Transform
            case 224: // RectTransform
            {
                var goId = YamlHelpers.GetRef(doc.Body, "m_GameObject");
                var parentId = YamlHelpers.GetRef(doc.Body, "m_Father");
                var children = YamlHelpers.GetRefList(doc.Body, "m_Children");
                _currentScene.Transforms[doc.FileId] = new Transform(goId, parentId, children);
                _currentScene.GameObjectToTransform[goId] = doc.FileId;
                break;
            }
            case 1660057539: // SceneRoots
            {
                _currentScene.SceneRootRefs = YamlHelpers.GetRefList(doc.Body, "m_Roots");
                break;
            }
        }
    }
    
    public void EndAndDumpScene(string outputDir)
    {
        if (_currentScene == null) return;
        _currentScene.Roots = YamlHelpers.FindRoots(_currentScene.Transforms, _currentScene.GameObjectToTransform, _currentScene.SceneRootRefs);
        // DumpSceneToConsole();
        DumpSceneToFile(outputDir);
        _currentScene = null;
    }

    public sealed record GameObject(string name);
    public sealed record Transform(long goId, long parentId, List<long> children);
    
    public void DumpSceneToConsole()
    {
        DumpScene(TextWriter.Synchronized(Console.Out));
    }

    public void DumpSceneToFile(string outputPath)
    {
        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        // UTF-8 without BOM is fine; change if they require BOM
        using var writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        DumpScene(writer);
    }
    
    public void DumpScene(TextWriter writer)
    {
        foreach (var root in _currentScene.Roots)
            PrintTree(writer, root, "");
    }
    
    private void PrintTree(TextWriter writer, long transformId, string prefix)
    {
        writer.WriteLine($"{prefix}{NameOf(transformId)}");

        if (!_currentScene.Transforms.TryGetValue(transformId, out var tf) || tf.children.Count == 0)
            return;

        foreach (var childTf in tf.children)
            if (_currentScene.Transforms.ContainsKey(childTf))
                PrintTree(writer, childTf, prefix + "--");
    }
    
    private string NameOf(long transformId)
    {
        if (!_currentScene.Transforms.TryGetValue(transformId, out var transform)) return $"<Missing Transform {transformId}>";
        if (!_currentScene.GameObjects.TryGetValue(transform.goId, out var gameObject)) return $"<Missing GameObject {transform.goId}>";
        return gameObject.name;
    }
}