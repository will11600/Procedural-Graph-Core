using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEditor.CustomEditors;
using FlaxEditor.Scripting;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// A default implementation of a graph entity that handles asynchronous generation, property change debouncing, and thread safety.
/// </summary>
/// <typeparam name="T">The type of generator to use.</typeparam>
public class GraphEntity<T> : IGraphEntity where T : IGenerator<T>
{
    private static readonly ScriptType _collectionType = new(typeof(ObservableCollection<GraphComponent>));

    /// <summary>
    /// The time to wait after a property change before triggering a rebuild, used to prevent excessive re-computation.
    /// </summary>
    protected readonly TimeSpan debouncePeriod;

    /// <summary>
    /// A flag indicating that parameters have changed and a rebuild is required.
    /// </summary>
    protected bool isDirty;

    /// <summary>
    /// Ensures that only one generation task runs at a time.
    /// </summary>
    protected readonly SemaphoreSlim semaphore;

    /// <summary>
    /// The source for cancelling background generation tasks when the entity is disposed or stopped.
    /// </summary>
    protected readonly CancellationTokenSource stoppingCts;
    /// <inheritdoc/>
    public CancellationToken StoppingToken => stoppingCts.Token;

    /// <summary>
    /// Gets a value indicating whether this entity has been disposed.
    /// </summary>
    protected bool IsDisposed { get; private set; }

    private readonly ObservableCollection<GraphComponent> _components;
    /// <summary>
    /// Gets the parameters associated with this entity.
    /// </summary>
    public ICollection<GraphComponent> Components => _components;

    /// <inheritdoc/>
    public Actor Actor { get; }

    /// <inheritdoc/>
    public CustomValueContainer ValueContainer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphEntity{T}"/> class.
    /// </summary>
    /// <param name="actor">The Actor associated with this entity.</param>
    /// <param name="cancellationToken">A token to observe for external cancellation.</param>
    /// <param name="models">The data models for this entity.</param>
    /// <param name="debounceSeconds">The delay in seconds to wait for changes to settle before rebuilding.</param>
    public GraphEntity(Actor actor, IEnumerable<GraphComponent> models, CancellationToken cancellationToken, double debounceSeconds = 0.2)
    {
        _components = [.. models];
        _components.CollectionChanged += OnCollectionChanged;
        _components.ItemPropertyChanged += OnPropertyChanged;

        Actor = actor;
        semaphore = new(1, 1);
        debouncePeriod = TimeSpan.FromSeconds(debounceSeconds);
        stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        stoppingCts.Token.Register(OnStopping);

        CollectionAttribute collectionAttribute = new()
        {
            Display = CollectionAttribute.DisplayType.Header
        };

        ValueContainer = new(_collectionType, (instance, index) => _components, attributes: [collectionAttribute])
        {
            _components
        };
    }

    /// <summary>
    /// Handles property change events from the <see cref="Components"/> collection.
    /// </summary>
    protected virtual void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MarkAsDirty();
    }

    /// <summary>
    /// Handles property change events from the items inside the <see cref="Components"/> collection.
    /// </summary>
    protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        MarkAsDirty();
    }

    /// <summary>
    /// Marks the entity as dirty and attempts to start the generation loop.
    /// </summary>
    public void MarkAsDirty()
    {
        Editor.Instance.Scene.MarkSceneEdited(Actor.Scene);
        Interlocked.Exchange(ref isDirty, true);
        if (semaphore.Wait(0))
        {
            StartGenerating();
        }
    }

    /// <summary>
    /// The core execution loop. Uses a <see cref="PeriodicTimer"/> to wait for the debounce period 
    /// and then executes the generator if the entity is still dirty.
    /// </summary>
    protected virtual async void StartGenerating()
    {
        T? generator = default;
        try
        {
            using PeriodicTimer periodicTimer = new(debouncePeriod);
            while (Interlocked.Exchange(ref isDirty, false) && await periodicTimer.WaitForNextTickAsync(stoppingCts.Token))
            {
                if (!isDirty)
                {
                    generator = T.Create(Actor, _components);
                    await generator.BuildAsync(stoppingCts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            semaphore.Release();
            if (generator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        Task cancellation = stoppingCts.CancelAsync();
        try
        {
            await cancellation.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }
    }

    /// <summary>
    /// Called when <see cref="StoppingToken"/> is cancelled.
    /// </summary>
    protected virtual void OnStopping()
    {
        _components.CollectionChanged -= OnCollectionChanged;
        _components.ItemPropertyChanged -= OnPropertyChanged;
    }

    /// <summary>
    /// Disposes resources used by the entity.
    /// </summary>
    protected virtual void OnDisposing()
    {
        stoppingCts.Dispose();
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            OnDisposing();
        }

        IsDisposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
