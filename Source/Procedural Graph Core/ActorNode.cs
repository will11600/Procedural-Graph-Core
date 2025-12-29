using System;
using System.Collections.Generic;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// ActorNode class.
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

    public ActorNode() : base()
    {
        Scripting.LateUpdate += OnUpdate;
    }

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

    public bool Equals(ActorNode? other)
    {
        return EqualityComparer<Actor>.Default.Equals(Actor, other?.Actor);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ActorNode);
    }

    public override int GetHashCode()
    {
        return Actor == null ? base.GetHashCode() : HashCode.Combine(Actor, Actor.Transform);
    }
}
