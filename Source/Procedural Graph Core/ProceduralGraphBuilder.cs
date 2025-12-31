using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProceduralGraph;

/// <summary>
/// <para>The central Editor Plugin and factory for the Procedural Graph system.</para>
/// <para>Manages the registration of node converters and facilitates the transformation of Flax Actors into processing Nodes.</para>
/// </summary>
public sealed class ProceduralGraphBuilder : EditorPlugin
{
    private readonly Dictionary<Type, INodeConverter> _converters;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralGraphBuilder"/> class and sets up plugin metadata.
    /// </summary>
    public ProceduralGraphBuilder()
    {
        _converters = [];
        _description = new PluginDescription()
        {
            Name = "Procedural Graph Core",
            Author = "William Brocklesby",
            AuthorUrl = "https://william-brocklesby.com",
            Category = "Procedural Graph",
            Version = new(1, 0, 0)
        };
    }

    /// <summary>
    /// Registers a new node converter type. The type must have a parameterless constructor.
    /// </summary>
    /// <typeparam name="T">The type of the converter to add.</typeparam>
    public void AddConverter<T>() where T : INodeConverter, new()
    {
        AddConverter(new T());
    }

    /// <summary>
    /// Registers an instance of a node converter.
    /// </summary>
    /// <typeparam name="T">The type of the converter.</typeparam>
    /// <param name="instance">The converter instance to register.</param>
    /// <exception cref="ArgumentException">Thrown if a converter of the same type is already registered.</exception>
    public void AddConverter<T>(T instance) where T : INodeConverter
    {
        if (_converters.TryAdd(typeof(T), instance))
        {
            return; 
        }

        throw new ArgumentException("A node converter of the same type already exists.", nameof(instance));
    }

    /// <summary>
    /// Removes a registered converter of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the converter to remove.</typeparam>
    /// <returns>The removed converter instance, or null if it wasn't found.</returns>
    public T? RemoveConverter<T>() where T : INodeConverter
    {
        if (_converters.Remove(typeof(T), out INodeConverter? converter))
        {
            return (T)converter;
        }

        return default;
    }

    /// <summary>
    /// Attempts to convert a Flax <see cref="Actor"/> into a <see cref="INode"/> using the first available registered converter that supports it.
    /// </summary>
    /// <param name="actor">The source Flax <see cref="Actor"/> to convert.</param>
    /// <param name="node">The resulting <see cref="INode"/> if successful; otherwise, null.</param>
    /// <returns>True if a conversion was successful, otherwise false.</returns>
    public bool TryConvertFirst(Actor actor, [NotNullWhen(true)] out INode? node)
    {
        foreach (INodeConverter converter in _converters.Values)
        {
            if (converter.TryConvert(actor, out node))
            {
                return true;
            }
        }

        node = null;
        return false;
    }
}
