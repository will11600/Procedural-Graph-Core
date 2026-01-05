using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProceduralGraph;

internal sealed class GraphInstance : IReadOnlyDictionary<Actor, IGraphEntity>, IDisposable
{
    public string AssetPath { get; }

    public Scene Scene { get; }

    public int Count => _nodes.Count;

    public IEnumerable<Actor> Keys => _nodes.Keys;

    public IEnumerable<IGraphEntity> Values => _nodes.Values;

    public IGraphEntity this[Actor key] => _nodes[key];

    public GraphLifecycleManager LifecycleManager { get; }

    private readonly Dictionary<Actor, IGraphEntity> _nodes;
    private readonly Dictionary<Guid, List<GraphComponent>> _models;
    private bool _disposed;

    public GraphInstance(Scene scene, GraphLifecycleManager lifecycleManager)
    {
        AssetPath = Path.Combine(Globals.ProjectContentFolder, "SceneData", scene.Name, "Procedural Graph.json");
        Scene = scene ?? throw new ArgumentNullException(nameof(scene));
        LifecycleManager = lifecycleManager ?? throw new ArgumentNullException(nameof(lifecycleManager));

        _nodes = [];
        _models = Load(AssetPath);
        AddNodesRecurively(scene);
    }

    public void Save()
    {
        GraphAsset graph = new(_nodes.Values.SelectMany(GraphModels));
        Editor.SaveJsonAsset(AssetPath, graph);
    }

    public bool ContainsKey(Actor key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _nodes.ContainsKey(key);
    }

    public bool TryGetValue(Actor key, [MaybeNullWhen(false)] out IGraphEntity value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _nodes.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<Actor, IGraphEntity>> GetEnumerator()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _nodes.GetEnumerator();
    }

    public bool Add(Actor key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ref IGraphEntity? node = ref CollectionsMarshal.GetValueRefOrAddDefault(_nodes, key, out bool exists);
        if (!exists && TryGetConverter(key, out IGraphConverter? converter))
        {
            node = Convert(key, converter);
            return true;
        }
        return false;
    }

    public bool Remove(Actor key, [NotNullWhen(true)] out IGraphEntity? value)
    {
        return _nodes.Remove(key, out value); 
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        List<Task> stoppings = new(_nodes.Count);

        foreach (IGraphEntity node in _nodes.Values)
        {
            Task stopping = node.StopAsync(cancellationToken);
            stoppings.Add(stopping);
        }

        await Task.WhenAll(stoppings);
    }

    private void AddNodesRecurively(Actor parent)
    {
        if (TryGetConverter(parent, out IGraphConverter? converter))
        {
            IGraphEntity node = Convert(parent, converter);
            _nodes.Add(parent, node);
        }

        foreach (Actor actor in parent.Children)
        {
            AddNodesRecurively(actor);
        }
    }

    private IGraphEntity Convert(Actor actor, IGraphConverter converter)
    {
        if (_models.Remove(actor.ID, out List<GraphComponent>? models))
        {
            return converter.Convert(actor, models, LifecycleManager.StoppingToken);
        }

        return converter.Convert(actor, [], LifecycleManager.StoppingToken);
    }

    private bool TryGetConverter(Actor actor, [NotNullWhen(true)] out IGraphConverter? result)
    {
        foreach (IGraphConverter converter in LifecycleManager.Converters)
        {
            if (converter.CanConvert(actor))
            {
                result = converter;
                return true;
            }
        }

        result = default;
        return false;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (IGraphEntity node in _nodes.Values)
            {
                node.Dispose();
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static Dictionary<Guid, List<GraphComponent>> Load(string path)
    {
        if (Content.LoadAsync<JsonAsset>(path)?.GetInstance<GraphAsset>() is not GraphAsset proceduralGraph)
        {
            return [];
        }

        Dictionary<Guid, List<GraphComponent>> nodes = [];
        foreach (EntityComponent GraphModel in proceduralGraph.Nodes)
        {
            ref List<GraphComponent>? models = ref CollectionsMarshal.GetValueRefOrAddDefault(nodes, GraphModel.ActorID, out bool exists);

            if (exists)
            {
                models!.Add(GraphModel.Component);
                continue;
            }

            models = [GraphModel.Component];
        }

        return nodes;
    }

    private static IEnumerable<EntityComponent> GraphModels(IGraphEntity node)
    {
        Guid actorId = node.Actor.ID;
        foreach (GraphComponent model in node.Components)
        {
            yield return new EntityComponent(actorId, model);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
