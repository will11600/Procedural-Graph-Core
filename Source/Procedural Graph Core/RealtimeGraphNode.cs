using System.Collections.Generic;
using System.Threading;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
///  An extension of <seealso cref="GraphEntity{T}"/> that can handle dynamic updates.
/// </summary>
/// <typeparam name="T">The type of generator to use.</typeparam>
public class RealtimeGraphentity<T> : GraphEntity<T> where T : IGenerator<T>
{
    private int _transformHashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealtimeGraphentity{T}"/> class.
    /// </summary>
    /// <param name="actor">The Actor associated with this entity.</param>
    /// <param name="cancellationToken">A token to observe for external cancellation.</param>
    /// <param name="components">The data models for this entity.</param>
    /// <param name="debounceSeconds">The delay in seconds to wait for changes to settle before rebuilding.</param>
    public RealtimeGraphentity(Actor actor, IEnumerable<GraphComponent> components, CancellationToken cancellationToken, double debounceSeconds = 0.2) : 
        base(actor, components, cancellationToken, debounceSeconds)
    {
        _transformHashCode = actor.Transform.GetHashCode();
        Scripting.Update += OnUpdate;
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void OnUpdate()
    {
        int currentTransformHashCode = Actor.Transform.GetHashCode();
        (_transformHashCode, currentTransformHashCode) = (currentTransformHashCode, _transformHashCode);
        if (_transformHashCode != currentTransformHashCode)
        {
            MarkAsDirty();
        }
    }

    /// <inheritdoc/>
    protected override void OnStopping()
    {
        base.OnStopping();
        Scripting.Update -= OnUpdate;
    }
}
