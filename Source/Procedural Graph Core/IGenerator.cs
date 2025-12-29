using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph;

/// <summary>
/// IGenerator interface.
/// </summary>
public interface IGenerator
{
    Task BuildAsync(CancellationToken cancellationToken);
}
