using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FlaxEditor;
using FlaxEngine;

namespace ProceduralGraph;

/// <summary>
/// A base implementation of a graph node that handles asynchronous generation, property change debouncing, and thread safety.
/// </summary>
public class Node : INode
{
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
    /// The source for cancelling background generation tasks when the node is disposed or stopped.
    /// </summary>
    protected readonly CancellationTokenSource stoppingCts;
    /// <inheritdoc/>
    public CancellationToken StoppingToken => stoppingCts.Token;

    /// <summary>
    /// Gets a value indicating whether this node has been disposed.
    /// </summary>
    protected bool IsDisposed { get; private set; }

    private readonly ObservableCollection<Model> _models;
    /// <summary>
    /// Gets the parameters associated with this node.
    /// </summary>
    public ICollection<Model> Models => _models;

    /// <summary>
    /// Gets the generator responsible for the build logic.
    /// </summary>
    public IGenerator Generator { get; }

    /// <summary>
    /// Gets or the Actor associated with this node.
    /// </summary>
    public Actor Actor { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="actor">The Actor associated with this node.</param>
    /// <param name="generator">The generation logic provider.</param>
    /// <param name="cancellationToken">A token to observe for external cancellation.</param>
    /// <param name="parameters">The data model for this node.</param>
    /// <param name="debounceSeconds">The delay in seconds to wait for changes to settle before rebuilding.</param>
    /// <exception cref="ArgumentNullException">Thrown if generator or parameters are null.</exception>
    public Node(Actor actor, IGenerator generator, ReadOnlySpan<Model> parameters, CancellationToken cancellationToken, double debounceSeconds = 0.2)
    {
        _models = [.. parameters];
        _models.CollectionChanged += OnCollectionChanged;
        _models.ItemPropertyChanged += OnPropertyChanged;

        Actor = actor;
        Generator = generator ?? throw new ArgumentNullException(nameof(generator));
        semaphore = new(1, 1);
        debouncePeriod = TimeSpan.FromSeconds(debounceSeconds);
        stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    }

    /// <summary>
    /// Handles property change events from the <see cref="Models"/> collection.
    /// </summary>
    protected virtual void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MarkAsDirty();
    }

    /// <summary>
    /// Handles property change events from the items inside the <see cref="Models"/> collection.
    /// </summary>
    protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        MarkAsDirty();
    }

    /// <summary>
    /// Marks the node as dirty and attempts to start the generation loop.
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
    /// and then executes the generator if the node is still dirty.
    /// </summary>
    protected virtual async void StartGenerating()
    {
        try
        {
            using PeriodicTimer periodicTimer = new(debouncePeriod);
            while (Interlocked.Exchange(ref isDirty, false) && await periodicTimer.WaitForNextTickAsync(stoppingCts.Token))
            {
                if (!isDirty)
                {
                    await Generator.BuildAsync(_models, stoppingCts.Token);
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
    /// Disposes resources used by the node.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            _models.CollectionChanged -= OnCollectionChanged;
            _models.ItemPropertyChanged -= OnPropertyChanged;
            stoppingCts.Dispose();
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
