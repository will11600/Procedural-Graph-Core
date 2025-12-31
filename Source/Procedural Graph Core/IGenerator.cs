using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph;

/// <summary>
/// Defines the logic for performing procedural generation based on a specific set of parameters.
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Asynchronously builds or updates the procedural content.
    /// </summary>
    /// <param name="parameters">The current configuration data.</param>
    /// <param name="cancellationToken">A token to monitor for request cancellation.</param>
    /// <returns>A task representing the asynchronous build operation.</returns>
    Task BuildAsync(ICollection<Model> parameters, CancellationToken cancellationToken);
}
