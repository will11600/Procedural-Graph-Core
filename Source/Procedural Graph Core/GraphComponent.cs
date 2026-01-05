using FlaxEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ProceduralGraph;

/// <summary>
/// Base class for all node parameter data models. Implements property change notification 
/// with high-performance bitwise updates for enum flags.
/// </summary>
public abstract class GraphComponent : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    private Guid _actorID;
    /// <summary>
    /// Gets or sets the unique identifier of the Actor associated with these parameters.
    /// </summary>
    [Serialize, HideInEditor]
    public Guid ActorID
    {
        get => _actorID;
        set => RaiseAndSetIfChanged(ref _actorID, in value);
    }

    /// <summary>
    /// Updates a field and triggers the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">A reference to the field to update.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property (automatically populated).</param>
    /// <returns>True if the value changed; otherwise false.</returns>
    protected bool RaiseAndSetIfChanged<T>(ref T field, ref readonly T value, [CallerMemberName] string? propertyName = default)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyHasChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Specialized helper to update enum flags efficiently using bitwise operations and trigger change notifications.
    /// </summary>
    /// <typeparam name="T">An unmanaged enum type.</typeparam>
    /// <param name="flags">The current flags field.</param>
    /// <param name="value">The flag to toggle.</param>
    /// <param name="isSet">Whether the flag should be set (true) or cleared (false).</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>True if the flags were modified; otherwise false.</returns>
    protected unsafe bool RaiseAndSetIfChanged<T>(ref T flags, T value, bool isSet, [CallerMemberName] string? propertyName = default) where T : unmanaged, Enum => sizeof(T) switch
    {
        < sizeof(short) => RaiseAndSetNumberIfChanged(ref Unsafe.As<T, byte>(ref flags), Unsafe.As<T, byte>(ref value), isSet, propertyName),
        < sizeof(int) => RaiseAndSetNumberIfChanged(ref Unsafe.As<T, short>(ref flags), Unsafe.As<T, short>(ref value), isSet, propertyName),
        < sizeof(long) => RaiseAndSetNumberIfChanged(ref Unsafe.As<T, int>(ref flags), Unsafe.As<T, int>(ref value), isSet, propertyName),
        _ => RaiseAndSetNumberIfChanged(ref Unsafe.As<T, long>(ref flags), Unsafe.As<T, long>(ref value), isSet, propertyName),
    };

    private bool RaiseAndSetNumberIfChanged<T>(ref T flags, T value, bool isSet, string? propertyName) where T : INumberBase<T>, IBitwiseOperators<T, T, T>
    {
        if ((flags & value) != default == isSet)
        {
            return false;
        }

        flags = isSet ? flags | value : flags & ~value;
        PropertyHasChanged(propertyName);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PropertyHasChanged(string? propertyName)
    {
        PropertyChangedEventArgs eventArgs = new(propertyName);
        PropertyChanged?.Invoke(this, eventArgs);
    }
}
