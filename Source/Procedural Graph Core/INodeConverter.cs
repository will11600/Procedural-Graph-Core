using System.Diagnostics.CodeAnalysis;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// INodeConverter interface.
/// </summary>
public interface INodeConverter
{
    bool TryConvert(Actor actor, [NotNullWhen(true)] out ActorNode? node);
}
