using System;
using System.Collections.Generic;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// Represents an abstract base class for nodes that are associated with a specific actor in the scene.
/// </summary>
public abstract class ActorNode : Node, IEquatable<ActorNode>
{
    private Guid _actorID;
    [Serialize, HideInEditor]
    internal Guid ActorID
    {
        get => _actorID;
        set
        {
            if (_actorID == value)
            {
                return;
            }

            _actorID = value;
            _actor = Level.FindActor(value);
        }
    }
    
    private Actor? _actor;
    /// <summary>
    /// Gets or sets the actor associated with this node.
    /// </summary>
    /// <value>
    /// The <see cref="FlaxEngine.Actor"/> instance linked to this node, or <see langword="null"/> if no actor is assigned.
    /// </value>
    [NoSerialize, HideInEditor]
    public Actor? Actor
    {
        get => _actor; 
        set
        {
            _actor = value;
            _actorID = value?.ID ?? Guid.Empty;
        }
    }

    private int _hashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActorNode"/> class.
    /// </summary>
    public ActorNode() : base()
    {
        Scripting.LateUpdate += OnUpdate;
    }

    /// <inheritdoc/>
    public override void OnStopping()
    {
        base.OnStopping();
        Scripting.LateUpdate -= OnUpdate;
    }

    protected virtual void OnUpdate()
    {
        if (Actor == null)
        {
            return;
        }

        int currentHashCode = GetHashCode();
        (_hashCode, currentHashCode) = (currentHashCode, _hashCode);

        if (_hashCode != currentHashCode)
        {
            ParametersHaveChanged();
        }
    }

    /// <inheritdoc/>
    public bool Equals(ActorNode? other)
    {
        return EqualityComparer<Actor>.Default.Equals(Actor, other?.Actor);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ActorNode);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Actor == null ? base.GetHashCode() : HashCode.Combine(Actor, Actor.Transform);
    }
}
