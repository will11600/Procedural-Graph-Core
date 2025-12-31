using System.Diagnostics.CodeAnalysis;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// Defines a contract for classes capable of converting a Flax Engine <see cref="Actor"/> into a graph node.
/// </summary>
public interface INodeConverter
{
    /// <summary>
    /// Attempts to convert the specified actor into an <see cref="INode"/>.
    /// </summary>
    /// <param name="actor">The actor to evaluate.</param>
    /// <param name="node">The resulting node if conversion succeeds.</param>
    /// <returns>True if the actor was compatible and converted; otherwise false.</returns>
    bool TryConvert(Actor actor, [NotNullWhen(true)] out INode? node);
}
