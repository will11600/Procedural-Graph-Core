using FlaxEngine;
using Newtonsoft.Json;
using System;

namespace ProceduralGraph;

internal sealed class EntityComponent
{
    [Serialize, ShowInEditor]
    public Guid ActorID { get; set; }

    [Serialize, ShowInEditor, JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public GraphComponent Component { get; set; }

    public EntityComponent(Guid actorID, GraphComponent model)
    {
        ActorID = actorID;
        Component = model;
    }

    public EntityComponent()
    {
        ActorID = Guid.Empty;
        Component = default!;
    }
}