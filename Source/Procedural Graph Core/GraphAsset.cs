using System.Collections.Generic;

namespace ProceduralGraph;

internal sealed class GraphAsset
{
    public EntityComponent[] Components { get; set; }

    public GraphAsset()
    {
        Components = [];
    }

    public GraphAsset(IEnumerable<EntityComponent> entityComponents)
    {
        Components = [.. entityComponents];
    }
}
