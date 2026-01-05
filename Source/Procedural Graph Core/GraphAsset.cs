using System.Collections.Generic;

namespace ProceduralGraph;

internal sealed class GraphAsset
{
    public EntityComponent[] Nodes { get; set; }

    public GraphAsset()
    {
        Nodes = [];
    }

    public GraphAsset(IEnumerable<EntityComponent> nodeModels)
    {
        Nodes = [.. nodeModels];
    }
}
