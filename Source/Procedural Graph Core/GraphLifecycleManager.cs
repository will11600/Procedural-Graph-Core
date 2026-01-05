using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;

namespace ProceduralGraph;

/// <summary>
/// GraphLifecycleManager EditorPlugin
/// </summary>
public sealed class GraphLifecycleManager : EditorPlugin
{
    private readonly Dictionary<Scene, GraphInstance> _graphs = [];

    private CancellationTokenSource? _stoppingCts;
    /// <summary>
    /// Gets a cancellation token that is triggered when the <see cref="Plugin"/> is unloaded.
    /// </summary>
    public CancellationToken StoppingToken => _stoppingCts!.Token;

    private readonly List<IGraphConverter> _converters = [];
    /// <summary>
    /// Gets a collection of graph converters which facilitate the transformation of Flax Actors into entities.
    /// </summary>
    public ICollection<IGraphConverter> Converters => _converters;

    /// <summary>
    /// Initializes a new instance of <see cref="GraphLifecycleManager"/>.
    /// </summary>
    public GraphLifecycleManager() : base()
    {
        _graphs = [];
        _converters = [];
        _description = new PluginDescription()
        {
            Name = "Procedural Graph: Core",
            Author = "William Brocklesby",
            AuthorUrl = "https://william-brocklesby.com",
            Category = "Procedural Graph",
            Description = "Procedural Graph is a Flax Engine Editor Plugin designed to manage and execute procedural graph generation in real-time. It serves as the runtime execution layer for the Procedural Graph system, handling the lifecycle of graph nodes, listening for scene changes, and managing asynchronous generation tasks to ensure editor responsiveness.",
            RepositoryUrl = "https://github.com/will11600/Procedural-Graph-Client.git",
            Version = new(1, 0, 0)
        };
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        try
        {
            _stoppingCts = new CancellationTokenSource();

            Level.SceneSaving += OnSceneSaving;

            Level.SceneLoaded += OnSceneLoaded;
            Level.SceneUnloaded += OnSceneUnloaded;

            Level.ActorSpawned += OnActorSpawned;
            Level.ActorDeleted += OnActorDeleted;
        }
        finally
        {
            base.Initialize();
        }
    }

    /// <inheritdoc/>
    public override void Deinitialize()
    {
        try
        {
            _stoppingCts!.Cancel();
        }
        finally
        {
            Level.SceneSaving -= OnSceneSaving;

            Level.SceneLoaded -= OnSceneLoaded;
            Level.SceneUnloaded -= OnSceneUnloaded;

            Level.ActorSpawned -= OnActorSpawned;
            Level.ActorDeleted -= OnActorDeleted;

            _stoppingCts?.Dispose();

            foreach (GraphInstance graph in _graphs.Values)
            {
                graph.Dispose();
            }

            base.Deinitialize();
        }
    }

    internal bool TryFindEntity(Actor? actor, [NotNullWhen(true)] out IGraphEntity? entity)
    {
        if (actor != null && _graphs.TryGetValue(actor.Scene, out GraphInstance? graph))
        {
            return graph.TryGetValue(actor, out entity);
        }

        entity = default;
        return false;
    }

    private void OnActorSpawned(Actor actor)
    {
        Scene scene = actor.Scene;
        ref GraphInstance? sceneInfo = ref CollectionsMarshal.GetValueRefOrAddDefault(_graphs, scene, out bool exists);
        if (!exists)
        {
            sceneInfo = new GraphInstance(scene, this);
        }

        sceneInfo!.Add(actor);
    }

    private async void OnActorDeleted(Actor actor)
    {
        if (!_graphs.TryGetValue(actor.Scene, out GraphInstance? graph) || !graph.Remove(actor, out IGraphEntity? entity))
        {
            return;
        }

        try
        {
            await entity.StopAsync(_stoppingCts!.Token);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
        }
        finally
        {
            entity.Dispose();
        }
    }

    private void OnSceneLoaded(Scene scene, Guid guid)
    {
        GraphInstance sceneInfo = new(scene, this);
        _graphs.Add(scene, sceneInfo);
    }

    private async void OnSceneUnloaded(Scene scene, Guid guid)
    {
        if (!_graphs.Remove(scene, out GraphInstance? graph))
        {
            return;
        }

        try
        {
            await graph.StopAsync(_stoppingCts!.Token);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
        }
        finally
        {
            graph.Dispose();
        }
    }

    private void OnSceneSaving(Scene scene, Guid guid)
    {
        if (_graphs.TryGetValue(scene, out GraphInstance? graph))
        {
            graph.Save();        
        }
    }
}
