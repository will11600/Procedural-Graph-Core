using FlaxEngine;
using Newtonsoft.Json;
using System;

namespace ProceduralGraph;

/// <summary>
/// Node class.
/// </summary>
public abstract class Node
{
    public static event Action<Node>? ParametersChanged;

    private Node? _parent;
    [NoSerialize]
    public Node? Parent
    {
        get => _parent;
        set => SetParent(value);
    }

    [Serialize, ShowInEditor, JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public Node[] Children { get; set; } = [];

    public virtual NodeFlags Flags => NodeFlags.None;

    public abstract IGenerator CreateGenerator();

    protected void ParametersHaveChanged()
    {
        ParametersChanged?.Invoke(this);
    }

    public virtual void Draw()
    {

    }

    protected virtual void SetParent(Node? parent)
    {
        _parent = parent;
    }

    public virtual void OnParentStateChanged()
    {

    }

    public virtual void OnChildStateChanged(Node node)
    {

    }

    public virtual void OnStopping()
    {

    }
}
