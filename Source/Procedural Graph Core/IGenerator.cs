using FlaxEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph;

/// <summary>
/// Defines the logic for performing procedural generation based on a specific set of parameters.
/// </summary>
public interface IGenerator<TSelf> where TSelf : IGenerator<TSelf>
{
    /// <summary>
    /// Asynchronously builds or updates the procedural content.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for request cancellation.</param>
    /// <returns>A task representing the asynchronous build operation.</returns>
    Task BuildAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new instance of <typeparamref name="TSelf"/>.
    /// </summary>
    /// <param name="actor">The actor.</param>
    /// <param name="models">The current configuration data.</param>
    /// <returns>The constructed <typeparamref name="TSelf"/> instance.</returns>
    static abstract TSelf Create(Actor actor, IEnumerable<GraphComponent> models);
}
