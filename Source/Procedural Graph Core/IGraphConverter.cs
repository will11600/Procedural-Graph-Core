using System.Collections.Generic;
using System.Threading;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// Defines a contract for classes capable of converting a Flax Engine <see cref="Actor"/> into a graph entity.
/// </summary>
public interface IGraphConverter
{
    /// <summary>
    /// Determines whether the specified actor can be converted into an <see cref="IGraphEntity"/>.
    /// </summary>
    /// <param name="actor">The actor to evaluate.</param>
    /// <returns>True if the actor was compatible; otherwise false.</returns>
    bool CanConvert(Actor actor);

    /// <summary>
    /// Converts the specified actor into an <see cref="IGraphEntity"/>.
    /// </summary>
    /// <param name="actor">The actor to convert.</param>
    /// <param name="models">The deserialized <see cref="GraphComponent"/> instances for this actor.</param>
    /// <param name="stoppingToken">A cancellation token that is triggered when the <see cref="GraphLifecycleManager"/> is unloaded.</param>
    /// <returns>The resulting entity.</returns>
    /// <exception cref="System.ArgumentException">Thrown if the specified actor cannot be converted.</exception>
    IGraphEntity Convert(Actor actor, IEnumerable<GraphComponent> models, CancellationToken stoppingToken);
}
