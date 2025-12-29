using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProceduralGraph;

/// <summary>
/// NodeConverterFactory EditorPlugin.
/// </summary>
public sealed class ProceduralGraphBuilder : EditorPlugin
{
    private readonly Dictionary<Type, INodeConverter> _converters;

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

    public void AddConverter<T>() where T : INodeConverter, new()
    {
        AddConverter(new T());
    }

    public void AddConverter<T>(T instance) where T : INodeConverter
    {
        if (_converters.TryAdd(typeof(T), instance))
        {
            return; 
        }

        throw new ArgumentException("A node converter of the same type already exists.", nameof(instance));
    }

    public T? RemoveConverter<T>() where T : INodeConverter
    {
        if (_converters.Remove(typeof(T), out INodeConverter? converter))
        {
            return (T)converter;
        }

        return default;
    }

    public bool TryConvertFirst(Actor actor, [NotNullWhen(true)] out ActorNode? node)
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
