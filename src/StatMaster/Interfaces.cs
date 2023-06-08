using System.ComponentModel;
using System.Collections.Generic;

namespace StatMaster
{
    /// <summary>
    /// IReadOnlyValue<T> notifies listeners when it changes. That's it.
    /// You can only read this value but that doesn't mean it's immutable or const.
    /// It may be there are other things that change it.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IReadOnlyValue<out T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current value.
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// IValue<T> is an interface that represents a mutable value of type T.
    /// It inherits from IReadOnlyValue<T> and allows direct modification of the value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IValue<T> : IReadOnlyValue<T>
    {
        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        new T Value { get; set; }
    }

    /// <summary>
    /// Represents an interface for a bounded value.
    /// Implementations of this interface define the minimum and maximum values.
    /// </summary>
    /// <typeparam name="T">The type of the bounded value.</typeparam>
    public interface IBounded<T>
    {
        /// <summary>
        /// Gets the minimum allowed value.
        /// </summary>
        T MinValue { get; }

        /// <summary>
        /// Gets the maximum allowed value.
        /// </summary>
        T MaxValue { get; }
    }

    /// <summary>
    /// Represents a collection of modifiers that sequentially alter an initial value.
    /// </summary>
    /// <typeparam name="T">The type of the modifiable value.</typeparam>
    public interface IModifiable<T> : IReadOnlyValue<T>, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the collection of modifiers that alter the modifiable value.
        /// The implementation of the collection should handle property change events.
        /// </summary>
        IPriorityCollection<IModifier<T>> Modifiers { get; }
    }

    /// <summary>
    /// Represents a modifiable value that is altered by a collection of modifiers.
    /// The initial value type S is converted to T and then modified by the enabled modifiers in the collection.
    /// </summary>
    /// <typeparam name="S">The type of the initial value.</typeparam>
    /// <typeparam name="T">The type of the modifiable value.</typeparam>
    public interface IModifiable<out S, T> : IModifiable<T>
    {
        /// <summary>
        /// Gets the initial value of type S that is converted and modified.
        /// </summary>
        S Initial { get; }
    }

    /// <summary>
    /// Represents a modifiable value that can be altered by various effects in games,
    /// such as health, strength, etc.
    /// </summary>
    /// <typeparam name="T">The type of the modifiable value.</typeparam>
    public interface IModifiableValue<T> : IModifiable<IValue<T>, T> { }

    /// <summary>
    /// Represents a collection that allows specifying a priority for the items.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    public interface IPriorityCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Adds an item to the collection with the specified priority.
        /// </summary>
        /// <param name="priority">The priority of the item.</param>
        /// <param name="item">The item to add.</param>
        void Add(int priority, T item);
    }

    /// <summary>
    /// Represents a modifier that alters the value of a modifiable value.
    /// </summary>
    /// <typeparam name="T">The type of the value being modified.</typeparam>
    public interface IModifier<T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets a value indicating whether the modifier is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Modifies the given value based on the symbol and the context.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="given">The value to modify.</param>
        /// <returns>The modified value.</returns>
        T Modify(T given);
    }

    /// <summary>
    /// Represents a modifier that provides a context and modifies a value.
    /// It is useful for exposing I(ReadOnly)Values, among other things.
    /// </summary>
    /// <typeparam name="S">The type of the context.</typeparam>
    /// <typeparam name="T">The type of the value being modified.</typeparam>
    public interface IModifier<out S, T> : IModifier<T>
    {
        /// <summary>
        /// Gets the context associated with the modifier.
        /// </summary>
        S Context { get; }
    }

    /// <summary>
    /// Represents a target that applies a modifier to a specific object.
    /// </summary>
    /// <typeparam name="S">The type of the object to which the modifier applies.</typeparam>
    /// <typeparam name="T">The type of the value being modified.</typeparam>
    public interface ITarget<in S, T>
    {
        /// <summary>
        /// Gets the modifier associated with the target.
        /// </summary>
        IModifier<T> Modifier { get; }

        /// <summary>
        /// Applies the modifier to the specified object.
        /// </summary>
        /// <param name="thing">The object to which the modifier applies.</param>
        /// <returns>The modifiable object.</returns>
        IModifiable<T> AppliesTo(S thing);
    }

    /// <summary>
    /// Represents a decorator that allows peeking inside the decorated object.
    /// </summary>
    /// <typeparam name="T">The type of the decorated object.</typeparam>
    public interface IDecorator<out T>
    {
        /// <summary>
        /// Gets the decorated object.
        /// </summary>
        T Decorated { get; }
    }
}
