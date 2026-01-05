using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FlaxEditor.CustomEditors.Editors;
using FlaxEngine;

namespace ProceduralGraph.Interface;

/// <summary>
/// Custom editor base class for actors that are represented or managed by a procedural graph entity.
/// Provides integration between the Flax Editor UI and the procedural graph lifecycle.
/// </summary>
public abstract class ProceduralActorEditor : GenericEditor
{
    /// <summary>
    /// Gets the manager responsible for handling the synchronization and lifecycle of graph-linked objects.
    /// </summary>
    protected GraphLifecycleManager? LifecycleManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralActorEditor"/> class and retrieves the <see cref="GraphLifecycleManager"/> plugin.
    /// </summary>
    public ProceduralActorEditor() : base()
    {
        LifecycleManager = PluginManager.GetPlugin<GraphLifecycleManager>();
    }

    /// <summary>
    /// Attempts to find the procedural graph entity associated with the actor currently being edited.
    /// </summary>
    /// <param name="entity">When this method returns, contains the associated <see cref="IGraphEntity"/> if found; otherwise, null.</param>
    /// <returns><c>true</c> if a corresponding entity was found in the lifecycle manager; otherwise, <c>false</c>.</returns>
    protected bool TryFindEntity([NotNullWhen(true)] out IGraphEntity? entity)
    {
        if (LifecycleManager != null && LifecycleManager.TryFindEntity(Values.FirstOrDefault() as Actor, out entity))
        {
            return true;
        }

        entity = default;
        return false;
    }
}