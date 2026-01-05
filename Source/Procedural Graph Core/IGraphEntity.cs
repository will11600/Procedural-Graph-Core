using FlaxEditor.CustomEditors;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph;

/// <summary>
/// Represents a processing unit within the procedural graph.
/// </summary>
public interface IGraphEntity : IDisposable
{
    /// <summary>
    /// Gets a cancellation token that is triggered when the entity is stopping or disposing.
    /// </summary>
    CancellationToken StoppingToken { get; }

    /// <summary>
    /// Gets the configuration parameters for this entity.
    /// </summary>
    ICollection<GraphComponent> Components { get; }

    /// <summary>
    /// Gets or the Actor associated with this entity.
    /// </summary>
    Actor Actor { get; }

    /// <summary>
    /// Gets a <see cref="CustomValueContainer"/> of configurable values to display in the editor.
    /// </summary>
    CustomValueContainer ValueContainer { get; }

    /// <summary>
    /// Requests that the entity stop any background processing immediately.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for the stopping process itself.</param>
    /// <returns>A task representing the shutdown operation.</returns>
    Task StopAsync(CancellationToken cancellationToken);
}