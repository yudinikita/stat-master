using System;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace StatMaster
{
    public static class ModifiableValue
    {
        /// <summary>
        /// Collects how a particular modifier changes the value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modifiable">The modifiable value.</param>
        /// <param name="modifier">The modifier to probe.</param>
        /// <returns>An enumerable of before and after values.</returns>
        public static IEnumerable<(T before, T after)> ProbeAffects<T>(
            this IModifiable<IReadOnlyValue<T>, T> modifiable,
            IModifier<T> modifier
        )
        {
            T before = modifiable.Initial.Value;
            foreach (var _modifier in modifiable.Modifiers)
            {
                T after = before;
                if (_modifier.Enabled)
                    after = _modifier.Modify(before);
                if (modifier == _modifier)
                    yield return (before, after);
                before = after;
            }
        }

#if NET7_0_OR_GREATER
        /// <summary>
        /// Returns the delta a modifier (may be multiple) does.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modifiable">The modifiable value.</param>
        /// <param name="modifier">The modifier to probe.</param>
        /// <returns>The accumulated delta.</returns>
        public static T ProbeDelta<T>(
            this IModifiable<IReadOnlyValue<T>, T> modifiable,
            IModifier<T> modifier
        )
            where T : INumber<T>
        {
            T accum = T.Zero;
            foreach (T delta in modifiable.ProbeAffects(modifier).Select(x => x.after - x.before))
                accum += delta;
            return accum;
        }
#else
        public static T ProbeDelta<T>(
            this IModifiable<IReadOnlyValue<T>, T> modifiable,
            IModifier<T> modifier
        )
        {
            var op = Modifier.GetOperator<T>();
            T accum = op.Zero;
            foreach (
                var delta in modifiable
                    .ProbeAffects(modifier)
                    .Select(x => op.Sum(x.after, op.Negate(x.before)))
            )
                accum = op.Sum(accum, delta);
            return accum;
        }
#endif

        /// <summary>
        /// Removes all occurrences of an item from a collection. Returns the number of items removed.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns>The number of items removed.</returns>
        public static int RemoveAll<T>(this ICollection<T> collection, T item)
        {
            int count = 0;
            while (collection.Remove(item))
                count++;
            return count;
        }
    }

#if UNITY_5_3_OR_NEWER
    /**
     * In order to make Unity's serialization work properly, we need to have a concrete type
     * rather than an interface as the initial value.
     */
    [Serializable]
    public class ModifiableValue<T> : Modifiable<PropertyValue<T>, T>, IModifiableValue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableValue{T}"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        public ModifiableValue(PropertyValue<T> initial)
            : base(initial) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableValue{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public ModifiableValue(T initialValue)
            : base(new PropertyValue<T>(initialValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableValue{T}"/> class.
        /// </summary>
        public ModifiableValue()
            : this(default(T)) { }

        IValue<T> IModifiable<IValue<T>, T>.Initial => _initial;
    }

    /// <summary>
    /// Represents a modifiable read-only value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class ModifiableReadOnlyValue<T> : Modifiable<ReadOnlyValue<T>, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableReadOnlyValue{T}"/> class.
        /// </summary>
        /// <param name="initial">The initial read-only value.</param>
        public ModifiableReadOnlyValue(ReadOnlyValue<T> initial)
            : base(initial) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableReadOnlyValue{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public ModifiableReadOnlyValue(T initialValue)
            : base(new ReadOnlyValue<T>(initialValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableReadOnlyValue{T}"/> class.
        /// </summary>
        public ModifiableReadOnlyValue()
            : this(default(T)) { }
    }

    /// <summary>
    /// Represents a modifiable value implementing the <see cref="IValue{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class ModifiableIValue<T> : Modifiable<IValue<T>, T>, IModifiableValue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableIValue{T}"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        public ModifiableIValue(IValue<T> initial)
            : base(initial) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableIValue{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public ModifiableIValue(T initialValue)
            : base(new PropertyValue<T>(initialValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableIValue{T}"/> class.
        /// </summary>
        public ModifiableIValue()
            : this(default(T)) { }
    }

    /// <summary>
    /// Represents a modifiable read-only value implementing the <see cref="IReadOnlyValue{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class ModifiableIReadOnlyValue<T> : Modifiable<IReadOnlyValue<T>, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableIReadOnlyValue{T}"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        public ModifiableIReadOnlyValue(IReadOnlyValue<T> initial)
            : base(initial) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableIReadOnlyValue{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public ModifiableIReadOnlyValue(T initialValue)
            : base(new ReadOnlyValue<T>(initialValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableIReadOnlyValue{T}"/> class.
        /// </summary>
        public ModifiableIReadOnlyValue()
            : this(default(T)) { }
    }
#else

    /// <summary>
    /// Represents a modifiable value implementing the <see cref="IValue{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class ModifiableValue<T> : Modifiable<IValue<T>, T>, IModifiableValue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableValue{T}"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        public ModifiableValue(IValue<T> initial)
            : base(initial) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableValue{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public ModifiableValue(T initialValue)
            : base(new PropertyValue<T>(initialValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableValue{T}"/> class.
        /// </summary>
        public ModifiableValue()
            : this(default(T)) { }
    }

    /// <summary>
    /// Represents a modifiable read-only value implementing the <see cref="IReadOnlyValue{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class ModifiableReadOnlyValue<T> : Modifiable<IReadOnlyValue<T>, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableReadOnlyValue{T}"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        public ModifiableReadOnlyValue(IReadOnlyValue<T> initial)
            : base(initial) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableReadOnlyValue{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public ModifiableReadOnlyValue(T initialValue)
            : base(new ReadOnlyValue<T>(initialValue)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifiableReadOnlyValue{T}"/> class.
        /// </summary>
        public ModifiableReadOnlyValue()
            : this(default(T)) { }
    }
#endif

    /// <summary>
    /// Represents a modifiable value with bounds implementing the <see cref="IBounded{T}"/> interface.
    /// </summary>
    /// <typeparam name="S">The type of the initial value.</typeparam>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class BoundedModifiable<S, T> : Modifiable<S, T>, IBounded<T>
        where S : IReadOnlyValue<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        private readonly IReadOnlyValue<T> _minValue;
        private readonly IReadOnlyValue<T> _maxValue;

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        public T MinValue => _minValue.Value;

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        public T MaxValue => _maxValue.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedModifiable{S, T}"/> class with the specified initial value, minimum value, and maximum value.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        public BoundedModifiable(S initial, IReadOnlyValue<T> minValue, IReadOnlyValue<T> maxValue)
            : base(initial)
        {
            _minValue = minValue ?? throw new ArgumentNullException(nameof(minValue));
            _maxValue = maxValue ?? throw new ArgumentNullException(nameof(maxValue));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedModifiable{S, T}"/> class with the specified initial value, lower bound, and upper bound.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public BoundedModifiable(S value, T lowerBound, IReadOnlyValue<T> upperBound)
            : this(value, new ReadOnlyValue<T>(lowerBound), upperBound) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedModifiable{S, T}"/> class with the specified initial value, lower bound, and upper bound.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public BoundedModifiable(S value, IReadOnlyValue<T> lowerBound, T upperBound)
            : this(value, lowerBound, new ReadOnlyValue<T>(upperBound)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedModifiable{S, T}"/> class with the specified initial value, lower bound, and upper bound.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public BoundedModifiable(S value, T lowerBound, T upperBound)
            : this(value, new ReadOnlyValue<T>(lowerBound), new ReadOnlyValue<T>(upperBound)) { }

        /// <summary>
        /// Gets the bounded value by clamping the base value within the specified bounds.
        /// </summary>
        public override T Value => BoundedValue<T>.Clamp(base.Value, MinValue, MaxValue);
    }

    /// <summary>
    /// Represents a modifiable value with modifiers implementing the <see cref="IModifiable{S, T}"/> interface.
    /// </summary>
    /// <typeparam name="S">The type of the initial value.</typeparam>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class Modifiable<S, T> : IModifiable<S, T>
        where S : IReadOnlyValue<T>
    {
        protected ModifiersSortedList _modifiers;

        /// <summary>
        /// Gets the collection of modifiers.
        /// </summary>
        public IPriorityCollection<IModifier<T>> Modifiers => _modifiers;

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        protected S _initial;

        /// <summary>
        /// Gets the initial value.
        /// </summary>
        public virtual S Initial => _initial;

        /// <summary>
        /// This value is only updated if the value needs to be recomputed
        /// </summary>
        private T _cachedValue;

        /// <summary>
        /// ///  Flag to track if the value needs to be recomputed
        /// </summary>
        private bool _valueNeedsUpdate = true;

        /// <summary>
        /// Gets the current value after applying modifiers.
        /// </summary>
        public virtual T Value
        {
            get
            {
                if (_valueNeedsUpdate)
                {
                    _cachedValue = ComputeValue();
                    _valueNeedsUpdate = false;
                }

                return _cachedValue;
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly PropertyChangedEventArgs modifiersEventArgs =
            new PropertyChangedEventArgs(nameof(Modifiers));

        public Modifiable(S initial)
        {
            _initial = initial ?? throw new ArgumentNullException(nameof(initial));
            _initial.PropertyChanged += Chain;
            _modifiers = new ModifiersSortedList(this);
        }

        protected void Chain(object sender, PropertyChangedEventArgs args)
        {
            OnChange(nameof(Initial));
        }

        internal void OnChangeModifiers()
        {
            _valueNeedsUpdate = true;
            PropertyChanged?.Invoke(this, modifiersEventArgs);
        }

        internal void OnChange(string name)
        {
            _valueNeedsUpdate = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal void ModifiersChanged(object sender, PropertyChangedEventArgs e)
        {
            _valueNeedsUpdate = true;
            PropertyChanged?.Invoke(this, modifiersEventArgs);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(bool showModifiers)
        {
            if (!showModifiers)
            {
                return ToString();
            }

            var builder = new StringBuilder();
            builder.Append(" \"base\" ");
            builder.Append(Initial);
            builder.Append(' ');

            foreach (var modifier in Modifiers)
            {
                builder.Append(modifier);
                builder.Append(' ');
            }

            builder.Append("-> ");
            builder.Append(Value);
            return builder.ToString();
        }

        private T ComputeValue()
        {
            T v = Initial.Value;
            foreach (var modifier in Modifiers)
            {
                if (modifier.Enabled)
                {
                    v = modifier.Modify(v);
                }
            }
            return v;
        }

        /// <summary>
        /// Represents a sorted list for modifiers.
        /// </summary>
        /// <remarks>
        /// A sorted list for modifiers. It uses a tuple (int priority, int age)
        /// because SortedList<K,V> can only store one value per key. We may have many
        /// modifiers with the same priority (default priority is 0). So modifiers are
        /// ordered by priority first and age second. Each modifier will have a unique
        /// age ensuring that the keys will be unique.
        /// </remarks>
        protected class ModifiersSortedList
            : IPriorityCollection<IModifier<T>>,
                IComparer<(int priority, int age)>
        {
            private readonly Modifiable<S, T> _parent;
            private readonly SortedList<(int priority, int age), IModifier<T>> _modifiers =
                new SortedList<(int priority, int age), IModifier<T>>();
            private int _addCount = 0;

            public ModifiersSortedList(Modifiable<S, T> parent)
            {
                _parent = parent;
            }

            public IEnumerator<IModifier<T>> GetEnumerator()
            {
                return _modifiers.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IModifier<T> modifier)
            {
                Add(0, modifier);
            }

            public void Add(int priority, IModifier<T> modifier)
            {
                modifier.PropertyChanged -= _parent.ModifiersChanged;
                modifier.PropertyChanged += _parent.ModifiersChanged;
                _modifiers.Add((priority, ++_addCount), modifier);
                _parent.OnChangeModifiers();
            }

            public void Clear()
            {
                foreach (var modifier in _modifiers.Values)
                {
                    modifier.PropertyChanged -= _parent.ModifiersChanged;
                }

                _modifiers.Clear();
                _parent.OnChangeModifiers();
            }

            public bool Contains(IModifier<T> modifier)
            {
                return _modifiers.ContainsValue(modifier);
            }

            public void CopyTo(IModifier<T>[] array, int arrayIndex)
            {
                _modifiers.Values.CopyTo(array, arrayIndex);
            }

            public bool Remove(IModifier<T> modifier)
            {
                int index = _modifiers.IndexOfValue(modifier);
                if (index < 0)
                {
                    return false;
                }

                modifier.PropertyChanged -= _parent.ModifiersChanged;
                _modifiers.RemoveAt(index);
                _parent.OnChangeModifiers();
                return true;
            }

            public int Count => _modifiers.Count;

            public bool IsReadOnly => false;

            public int Compare((int priority, int age) x, (int priority, int age) y)
            {
                int result = x.priority.CompareTo(y.priority);
                if (result != 0)
                {
                    return result;
                }
                return x.age.CompareTo(y.age);
            }
        }
    }
}
