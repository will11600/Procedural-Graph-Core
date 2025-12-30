using FlaxEngine;
using Newtonsoft.Json;
using System;

namespace ProceduralGraph;

/// <summary>
/// Provides a base implementation for all procedural graph nodes.
/// </summary>
public abstract class Node
{
    /// <summary>
    /// Occurs when the parameters of any node have been modified.
    /// </summary>
    public static event Action<Node>? ParametersChanged;

    private Node? _parent;
    /// <summary>
    /// Gets or sets the parent node of this instance.
    /// </summary>
    /// <value>
    /// The parent <see cref="Node"/>, or <see langword="null"/> if this is a root node.
    /// </value>
    [NoSerialize]
    public Node? Parent
    {
        get => _parent;
        set => SetParent(value);
    }

    /// <summary>
    /// Gets or sets the collection of child nodes attached to this node.
    /// </summary>
    [Serialize, ShowInEditor, JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public Node[] Children { get; set; } = [];

    /// <summary>
    /// Gets the configuration flags that define the behavior of this node.
    /// </summary>
    public virtual NodeFlags Flags => NodeFlags.None;

    /// <summary>
    /// Creates a generator instance responsible for processing this node's logic.
    /// </summary>
    /// <returns>
    /// An <see cref="IGenerator"/> implementation for this node type.
    /// </returns>
    public abstract IGenerator CreateGenerator();

    protected virtual void ParametersHaveChanged()
    {
        ParametersChanged?.Invoke(this);
    }

    /// <summary>
    /// Renders the visual representation of the node within the editor or simulation.
    /// </summary>
    public virtual void Draw()
    {

    }

    protected virtual void SetParent(Node? parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Called when the state of the parent node has changed.
    /// </summary>
    public virtual void OnParentStateChanged()
    {

    }

    /// <summary>
    /// Called when the state of a specific child node has changed.
    /// </summary>
    /// <param name="node">The child node that triggered the state change.</param>
    public virtual void OnChildStateChanged(Node node)
    {

    }

    /// <summary>
    /// Performs cleanup operations when the graph or simulation is stopping.
    /// </summary>
    public virtual void OnStopping()
    {

    }
}
