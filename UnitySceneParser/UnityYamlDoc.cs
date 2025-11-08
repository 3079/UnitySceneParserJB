using YamlDotNet.RepresentationModel;

namespace JetBrainsInternship;

public sealed record UnityYamlDoc
(
    int ClassId,
    long FileId,
    YamlMappingNode Body
);