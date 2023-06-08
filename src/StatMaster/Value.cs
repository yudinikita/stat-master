using System;
using System.ComponentModel;

namespace StatMaster
{
    /// <summary>
    /// Represents a property value that can be observed for changes.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    [Serializable]
    public class PropertyValue<T> : IValue<T>
    {
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        protected T _value;
        public virtual T Value
        {
            get => _value;
            set
            {
                _value = value;
                PropertyChanged?.Invoke(this, valueEventArgs);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValue{T}"/> class.
        /// </summary>
        public PropertyValue() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValue{T}"/> class with the specified initial value.
        /// </summary>
        /// <param name="value">The initial value of the property.</param>
        public PropertyValue(T value) => _value = value;

        /// <summary>
        /// Event that is raised when the value of the property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// This is more costly: OnChange(nameof(value)). So let's just do this.
        /// </summary>
        private static PropertyChangedEventArgs valueEventArgs = new PropertyChangedEventArgs(
            nameof(Value)
        );

        /// <summary>
        /// Raises the PropertyChanged event with the provided property name.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected virtual void OnChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// A utility class for creating read-only values that can notify listeners when their value changes.
    /// </summary>
    public static class ReadOnlyValue
    {
        /// <summary>
        /// Creates a new instance of <see cref="IReadOnlyValue{T}"/> using the provided value getter function.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="valueGetter">The function that retrieves the value.</param>
        /// <param name="callOnChange">The action to call when the value changes.</param>
        /// <returns>The created <see cref="IReadOnlyValue{T}"/> instance.</returns>
        public static IReadOnlyValue<T> Create<T>(Func<T> valueGetter, out Action callOnChange) =>
            new DerivedReadOnlyValue<T>(valueGetter, out callOnChange);

        /// <summary>
        /// Creates a new instance of <see cref="IReadOnlyValue{T}"/> using the provided value getter function.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="valueGetter">The function that retrieves the value.</param>
        /// <returns>The created <see cref="IReadOnlyValue{T}"/> instance.</returns>
        public static IReadOnlyValue<T> Create<T>(Func<T> f) =>
            new DerivedReadOnlyValue<T>(f, out var callOnChange);

        /// <summary>
        /// Represents a derived implementation of <see cref="IReadOnlyValue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class DerivedReadOnlyValue<T> : IReadOnlyValue<T>
        {
            private readonly Func<T> _valueGetter;

            /// <summary>
            /// Initializes a new instance of the <see cref="DerivedReadOnlyValue{T}"/> class with the specified value getter function.
            /// </summary>
            /// <param name="valueGetter">The function that retrieves the value.</param>
            /// <param name="callOnChange">The action to call when the value changes.</param>
            public DerivedReadOnlyValue(Func<T> valueGetter, out Action callOnChange)
            {
                _valueGetter = valueGetter;
                callOnChange = OnChange;
            }

            public T Value => _valueGetter();

            public event PropertyChangedEventHandler PropertyChanged;
            private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(
                nameof(Value)
            );

            /// <summary>
            /// Raises the PropertyChanged event to notify listeners of a value change.
            /// </summary>
            protected virtual void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
        }
    }

    /// <summary>
    /// A utility class for creating instances of <see cref="IValue{T}"/>.
    /// </summary>
    public static class PropertyValue
    {
        /// <summary>
        /// Creates an instance of <see cref="IValue{T}"/> with the provided getter and setter functions.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="getter">The getter function for accessing the value.</param>
        /// <param name="setter">The setter action for modifying the value.</param>
        /// <param name="callOnChange">The action to call when the value changes.</param>
        /// <returns>An instance of <see cref="IValue{T}"/>.</returns>
        public static IValue<T> Create<T>(Func<T> getter, Action<T> setter, out Action callOnChange)
        {
            return new DerivedValue<T>(getter, setter, out callOnChange);
        }

        /// <summary>
        /// An implementation of <see cref="IValue{T}"/> that encapsulates a value with customizable getter and setter functions.
        /// </summary>
        internal class DerivedValue<T> : IValue<T>
        {
            private readonly Func<T> _getter;
            private readonly Action<T> _setter;

            /// <summary>
            /// Initializes a new instance of the <see cref="DerivedValue{T}"/> class with the provided getter, setter, and callback for value changes.
            /// </summary>
            /// <param name="getter">The getter function for accessing the value.</param>
            /// <param name="setter">The setter action for modifying the value.</param>
            /// <param name="callOnChange">The action to call when the value changes.</param>
            public DerivedValue(Func<T> getter, Action<T> setter, out Action callOnChange)
            {
                _getter = getter;
                _setter = setter;
                callOnChange = OnChange;
            }

            public T Value
            {
                get => _getter();
                set => _setter(value);
            }

            public event PropertyChangedEventHandler PropertyChanged;
            private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(
                nameof(Value)
            );

            protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
        }
    }

    /// <summary>
    /// Represents an <see cref="IValue{T}"/> that respects bounds.
    /// </summary>
    /// <remarks>
    /// If bounds change impinge on this object's current value, that value will change.
    /// Why no BoundedReadOnlyValue<T>? It's not truly needed. With a read only
    /// value, one only needs to clamp it for instance, a simple projection will do:
    /// ```var boundedValue = readOnlyValue.Select(x => Math.Clamp(x, 0f, myMax.value));```
    /// A special implementation is required for IValue<T> because it can be set,
    /// and although one can always clamp it's output as above, it won't function
    /// correctly. For instance, if you have `health` that is 100, you subtract 120.
    /// It will report 0. But when you add 10, it'll still report 0 because the
    /// underlying value would actually be at -10.
    /// </remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class BoundedValue<T> : IValue<T>, IBounded<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                _value = Clamp(value, MinValue, MaxValue);
                OnChange();
            }
        }

        public T MinValue => LowerBound.Value;
        public T MaxValue => UpperBound.Value;

        public readonly IReadOnlyValue<T> LowerBound;
        public readonly IReadOnlyValue<T> UpperBound;

        public static T Clamp(T value, T minValue, T maxValue)
        {
#if NET7_0_OR_GREATER
            if (value < minValue)
                value = minValue;
            if (value > maxValue)
                value = maxValue;
            return value;
#else
            var op = Modifier.GetOperator<T>();
            return op.Max(minValue, op.Min(maxValue, value));
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedValue{T}"/> class with the specified value and bounds.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public BoundedValue(T value, IReadOnlyValue<T> lowerBound, IReadOnlyValue<T> upperBound)
        {
            _value = value;
            LowerBound = lowerBound;
            LowerBound.PropertyChanged += BoundChanged;
            UpperBound = upperBound;
            UpperBound.PropertyChanged += BoundChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedValue{T}"/> class with the specified value and bounds.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public BoundedValue(T value, T lowerBound, IReadOnlyValue<T> upperBound)
            : this(value, new ReadOnlyValue<T>(lowerBound), upperBound) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedValue{T}"/> class with the specified value and bounds.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public BoundedValue(T value, IReadOnlyValue<T> lowerBound, T upperBound)
            : this(value, lowerBound, new ReadOnlyValue<T>(upperBound)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedValue{T}"/> class with the specified value and bounds.
        /// </summary>
        /// <param name="value">The initial value.</param>
        /// <param name="lowerBound">The lower bound.</param>
        /// <param name="upperBound">The upper bound.</param>
        public BoundedValue(T value, T lowerBound, T upperBound)
            : this(value, new ReadOnlyValue<T>(lowerBound), new ReadOnlyValue<T>(upperBound)) { }

        private void BoundChanged(object sender, PropertyChangedEventArgs e)
        {
            Value = _value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(
            nameof(Value)
        );

        protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Represents a simple read-only value of type T.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class ReadOnlyValue<T> : IReadOnlyValue<T>
    {
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        private readonly T _value;

        public T Value => _value;

        /// <summary>
        /// Initializes a new instance of the ReadOnlyValue class with the specified value.
        /// </summary>
        /// <param name="value">The value to be encapsulated.</param>
        public ReadOnlyValue(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the ReadOnlyValue class with the specified value and action.
        /// </summary>
        /// <param name="value">The value to be encapsulated.</param>
        /// <param name="callOnChange">The action to call when the value changes.</param>
        public ReadOnlyValue(T value, Action callOnChange)
            : this(value) => callOnChange = OnChange;

        public event PropertyChangedEventHandler PropertyChanged;
        private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(
            nameof(Value)
        );

        protected void OnChange()
        {
            PropertyChanged?.Invoke(this, eventArgs);
        }

        public override string ToString() => Value.ToString();
    }
}
