using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Runtime.CompilerServices;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace StatMaster
{
    /// <summary>
    /// Provides helper methods for creating modifiers.
    /// </summary>
    public static class Modifier
    {
        /// <summary>
        /// Creates a modifier based on a provided function and retrieves an action to call on change.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="func">The function to modify the value.</param>
        /// <param name="callOnChange">The action to call on change.</param>
        /// <param name="funcExpression">The expression representing the function.</param>
        /// <returns>The created modifier.</returns>
        public static IModifier<T> Create<T>(
            Func<T, T> func,
            out Action callOnChange,
            [CallerArgumentExpression("func")] string funcExpression = null
        ) => new FuncModifier<T>(func, out callOnChange) { Name = funcExpression };

        /// <summary>
        /// Creates a modifier based on a provided function.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="func">The function to modify the value.</param>
        /// <param name="funcExpression">The expression representing the function.</param>
        /// <returns>The created modifier.</returns>
        public static IModifier<T> Create<T>(
            Func<T, T> func,
            [CallerArgumentExpression("func")] string funcExpression = null
        ) => new FuncModifier<T>(func) { Name = funcExpression };

        /// <summary>
        /// Represents a modifier based on a function.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        internal class FuncModifier<T> : ContextModifier<Func<T, T>, T>
        {
            public FuncModifier(Func<T, T> func, out Action callOnChange)
                : this(func)
            {
                callOnChange = () => OnChange(nameof(Context));
            }

            public FuncModifier(Func<T, T> func)
                : base(func) { }

            /// <inheritdoc/>
            public override T Modify(T given) => Context(given);

            /// <inheritdoc/>
            public override string ToString() => Name ?? "?f()";
        }

        /// <summary>
        /// Enables a modifier after a specified time span.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modifier">The modifier to enable.</param>
        /// <param name="timeSpan">The time span after which to enable the modifier.</param>
        public static void EnableAfter<T>(this IModifier<T> modifier, TimeSpan timeSpan)
        {
            var timer = new Timer(Enable, modifier, timeSpan, Timeout.InfiniteTimeSpan);
            void Enable(object modifier) => ((IModifier<T>)modifier).Enabled = true;
        }

        /// <summary>
        /// Disables a modifier after a specified time span.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modifier">The modifier to disable.</param>
        /// <param name="timeSpan">The time span after which to disable the modifier.</param>
        public static void DisableAfter<T>(this IModifier<T> modifier, TimeSpan timeSpan)
        {
            var timer = new Timer(Disable, modifier, timeSpan, Timeout.InfiniteTimeSpan);
            void Disable(object modifier) => ((IModifier<T>)modifier).Enabled = false;
        }

        /// <summary>
        /// Wraps a modifier with a specific context.
        /// </summary>
        /// <typeparam name="S">The context type.</typeparam>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="modifier">The modifier to wrap.</param>
        /// <param name="context">The context to associate with the modifier.</param>
        /// <returns>The wrapped modifier.</returns>
        public static IModifier<S, T> WithContext<S, T>(this IModifier<T> modifier, S context) =>
            new WrappedModifier<S, T>(context, modifier);

        /// <summary>
        /// Represents a wrapped modifier that decorates an inner modifier.
        /// </summary>
        /// <typeparam name="S">The context type.</typeparam>
        /// <typeparam name="T">The value type.</typeparam>
        ///
        internal class WrappedModifier<S, T> : ContextModifier<S, T>, IDecorator<IModifier<T>>
        {
            protected IModifier<T> inner;

            /// <summary>
            /// Gets the inner modifier being decorated.
            /// </summary>
            public IModifier<T> Decorated => inner;

            /// <inheritdoc/>
            public override bool Enabled
            {
                get => inner.Enabled;
                set => inner.Enabled = value;
            }

            /// <summary>
            /// Creates a new instance of the WrappedModifier class.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="inner">The inner modifier to be decorated.</param>
            public WrappedModifier(S context, IModifier<T> inner)
                : base(context)
            {
                this.inner = inner;
                this.inner.PropertyChanged += Chain;
            }

            /// <inheritdoc/>
            public override T Modify(T given) => inner.Modify(given);

            /// <inheritdoc/>
            public override string ToString() => inner.ToString();
        }

#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Enables the modifier after a specified delay by setting its Enabled property to true.
        /// </summary>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="modifier">The modifier to enable.</param>
        /// <param name="seconds">The delay in seconds.</param>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        public static IEnumerator EnableAfterCoroutine<T>(this IModifier<T> modifier, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            modifier.Enabled = true;
        }

        /// <summary>
        /// Disables the modifier after a specified delay by setting its Enabled property to false.
        /// </summary>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="modifier">The modifier to disable.</param>
        /// <param name="seconds">The delay in seconds.</param>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        public static IEnumerator DisableAfterCoroutine<T>(
            this IModifier<T> modifier,
            float seconds
        )
        {
            yield return new WaitForSeconds(seconds);
            modifier.Enabled = false;
        }
#endif

        /// <summary>
        /// Creates a target for modifying a value in a list.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="modifier">The modifier to apply to the value.</param>
        /// <param name="index">The index of the value in the list.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the list value.</returns>
        public static ITarget<IList<IModifiableValue<T>>, T> TargetList<T>(
            this IModifier<T> modifier,
            int index,
            [CallerArgumentExpression("index")] string name = null
        )
        {
            return new ListTarget<T>
            {
                Modifier = modifier,
                Context = index,
                Name = name
            };
        }

        /// <summary>
        /// Creates a target for modifying a value in a dictionary.
        /// </summary>
        /// <typeparam name="K">The type of the dictionary key.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="modifier">The modifier to apply to the value.</param>
        /// <param name="key">The key of the value in the dictionary.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the dictionary value.</returns>
        public static ITarget<IDictionary<K, IModifiableValue<T>>, T> TargetDictionary<K, T>(
            this IModifier<T> modifier,
            K key,
            [CallerArgumentExpression("key")] string name = null
        ) =>
            new DictionaryTarget<K, T>
            {
                Modifier = modifier,
                Context = key,
                Name = name
            };

        /// <summary>
        /// Creates a target for modifying a value based on a custom target function.
        /// </summary>
        /// <typeparam name="S">The type of the target context.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="modifier">The modifier to apply to the value.</param>
        /// <param name="target">The function that provides the target value.</param>
        /// <param name="name">The name of the target.</param>
        /// <returns>The target for modifying the value based on the custom target function.</returns>
        public static ITarget<S, T> Target<S, T>(
            this IModifier<T> modifier,
            Func<S, IModifiableValue<T>> target,
            string name = null
        ) =>
            new FuncTarget<S, T>
            {
                Modifier = modifier,
                Context = target,
                Name = name
            };

        /// <summary>
        /// Represents a base class for targets that apply modifications to values.
        /// </summary>
        /// <typeparam name="R">The type of the target context.</typeparam>
        /// <typeparam name="S">The type of the target object.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal abstract class BaseTarget<R, S, T> : ITarget<S, T>
        {
            /// <summary>
            /// Gets or sets the name of the target.
            /// </summary>
            public string Name { get; init; }

            /// <summary>
            /// Gets or sets the context of the target.
            /// </summary>
            public R Context { get; init; }

            /// <summary>
            /// Gets or sets the modifier to be applied to the value.
            /// </summary>
            public IModifier<T> Modifier { get; init; }

            /// <summary>
            /// Gets the default name of the target.
            /// </summary>
            public virtual string DefaultName => Context.ToString();

            /// <summary>
            /// Applies the target to the specified object.
            /// </summary>
            /// <param name="bag">The object to apply the target to.</param>
            /// <returns>The modifiable value that the target applies to.</returns>
            public abstract IModifiable<T> AppliesTo(S bag);

            /// <summary>
            /// Returns the string representation of the target.
            /// </summary>
            /// <returns>The string representation of the target.</returns>
            public override string ToString() => Name ?? DefaultName;
        }

        /// <summary>
        /// Represents a target that applies modifications to a value based on a custom target function.
        /// </summary>
        /// <typeparam name="S">The type of the target context.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class FuncTarget<S, T> : BaseTarget<Func<S, IModifiableValue<T>>, S, T>
        {
            public override IModifiable<T> AppliesTo(S bag) => Context(bag);
        }

        /// <summary>
        /// Represents a target that applies modifications to a value in a list.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class ListTarget<T> : BaseTarget<int, IList<IModifiableValue<T>>, T>
        {
            public override IModifiable<T> AppliesTo(IList<IModifiableValue<T>> bag) =>
                bag[Context];
        }

        /// <summary>
        /// Represents a target that applies modifications to a value in a dictionary.
        /// </summary>
        /// <typeparam name="K">The type of the dictionary key.</typeparam>
        /// <typeparam name="T">The type of the value.</typeparam>
        internal class DictionaryTarget<K, T>
            : BaseTarget<K, IDictionary<K, IModifiableValue<T>>, T>
        {
            public override IModifiable<T> AppliesTo(IDictionary<K, IModifiableValue<T>> bag) =>
                bag[Context];
        }

#if NET7_0_OR_GREATER

        // Plus

        /// <summary>
        /// Creates a plus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Plus<S>(S v, string name = null)
            where S : INumber<S> => Plus(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a plus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Plus<S>(
            IReadOnlyValue<S> v,
            string name = null
        )
            where S : INumber<S> =>
            new NumericalModifier<IReadOnlyValue<S>, S>(v) { name = name, symbol = '+' };

        /// <summary>
        /// Creates a plus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IValue<S>, S> Plus<S>(IValue<S> v, string name = null)
            where S : INumber<S> =>
            new NumericalModifier<IValue<S>, S>(v) { name = name, symbol = '+' };

        // Times

        /// <summary>
        /// Creates a times modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Times<S>(S v, string name = null)
            where S : INumber<S> => Times(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a times modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Times<S>(
            IReadOnlyValue<S> v,
            string name = null
        )
            where S : INumber<S> =>
            new NumericalModifier<IReadOnlyValue<S>, S>(v) { name = name, symbol = '*' };

        /// <summary>
        /// Creates a times modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IValue<S>, S> Times<S>(IValue<S> v, string name = null)
            where S : INumber<S> =>
            new NumericalModifier<IValue<S>, S>(v) { name = name, symbol = '*' };

        // Minus

        /// <summary>
        /// Creates a minus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Minus<S>(S v, string name = null)
            where S : INumber<S> => Minus(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a minus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Minus<S>(
            IReadOnlyValue<S> v,
            string name = null
        )
            where S : INumber<S> =>
            new NumericalModifier<IReadOnlyValue<S>, S>(v) { name = name, symbol = '-' };

        /// <summary>
        /// Creates a minus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IValue<S>, S> Minus<S>(IValue<S> v, string name = null)
            where S : INumber<S> =>
            new NumericalModifier<IValue<S>, S>(v) { name = name, symbol = '-' };

        // Divide

        /// <summary>
        /// Creates a divide modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Divide<S>(S v, string name = null)
            where S : INumber<S> => Divide(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a divide modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Divide<S>(
            IReadOnlyValue<S> v,
            string name = null
        )
            where S : INumber<S> =>
            new NumericalModifier<IReadOnlyValue<S>, S>(v) { name = name, symbol = '/' };

        /// <summary>
        /// Creates a divide modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IValue<S>, S> Divide<S>(IValue<S> v, string name = null)
            where S : INumber<S> =>
            new NumericalModifier<IValue<S>, S>(v) { name = name, symbol = '/' };

        // Substitute

        /// <summary>
        /// Creates a substitute modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Substitute<S>(S v, string name = null)
            where S : INumber<S> => Substitute(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a substitute modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Substitute<S>(
            IReadOnlyValue<S> v,
            string name = null
        )
            where S : INumber<S> =>
            new NumericalModifier<IReadOnlyValue<S>, S>(v) { name = name, symbol = '=' };

        /// <summary>
        /// Creates a substitute modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IValue<S>, S> Substitute<S>(IValue<S> v, string name = null)
            where S : INumber<S> =>
            new NumericalModifier<IValue<S>, S>(v) { name = name, symbol = '=' };
#else
        /// <summary>
        /// Represents an operator for performing mathematical operations on a generic type.
        /// Here is the alternative to having a nice INumber<T> type like .NET7 will have.
        /// </summary>
        /// <typeparam name="X">The type on which the operator performs operations.</typeparam>
        public interface IOperator<X>
        {
            /// <summary>
            /// Creates a value of type X from a value of type T.
            /// </summary>
            /// <typeparam name="T">The type of the input value.</typeparam>
            /// <param name="other">The input value.</param>
            /// <returns>The converted value of type X.</returns>
            X Create<T>(T other);

            /// <summary>
            /// Adds two values of type X and returns the result.
            /// </summary>
            /// <param name="a">The first value.</param>
            /// <param name="b">The second value.</param>
            /// <returns>The sum of the two values.</returns>
            X Sum(X a, X b);

            /// <summary>
            /// Multiplies two values of type X and returns the result.
            /// </summary>
            /// <param name="a">The first value.</param>
            /// <param name="b">The second value.</param>
            /// <returns>The product of the two values.</returns>
            X Times(X a, X b);

            /// <summary>
            /// Divides two values of type X and returns the result.
            /// </summary>
            /// <param name="a">The dividend.</param>
            /// <param name="b">The divisor.</param>
            /// <returns>The division result.</returns>
            X Divide(X a, X b);

            /// <summary>
            /// Negates a value of type X and returns the result.
            /// </summary>
            /// <param name="a">The value to be negated.</param>
            /// <returns>The negated value.</returns>
            X Negate(X a);

            /// <summary>
            /// Returns the maximum of two values of type X.
            /// </summary>
            /// <param name="a">The first value.</param>
            /// <param name="b">The second value.</param>
            /// <returns>The maximum value.</returns>
            X Max(X a, X b);

            /// <summary>
            /// Returns the minimum of two values of type X.
            /// </summary>
            /// <param name="a">The first value.</param>
            /// <param name="b">The second value.</param>
            /// <returns>The minimum value.</returns>
            X Min(X a, X b);

            /// <summary>
            /// Gets the zero value of type X.
            /// </summary>
            X Zero { get; }

            /// <summary>
            /// Gets the one value of type X.
            /// </summary>
            X One { get; }
        }

        /// <summary>
        /// Operator implementation for float type.
        /// </summary>
        internal struct OpFloat : IOperator<float>
        {
            public float Create<T>(T other) => Convert.ToSingle(other);

            public float Sum(float a, float b) => a + b;

            public float Times(float a, float b) => a * b;

            public float Divide(float a, float b) => a / b;

            public float Negate(float a) => -a;

            public float Max(float a, float b) => Math.Max(a, b);

            public float Min(float a, float b) => Math.Min(a, b);

            public float Zero => 0f;
            public float One => 1f;
        }

        /// <summary>
        /// Operator implementation for double type.
        /// </summary>
        internal struct OpDouble : IOperator<double>
        {
            public double Create<T>(T other) => Convert.ToDouble(other);

            public double Sum(double a, double b) => a + b;

            public double Times(double a, double b) => a * b;

            public double Divide(double a, double b) => a / b;

            public double Negate(double a) => -a;

            public double Max(double a, double b) => Math.Max(a, b);

            public double Min(double a, double b) => Math.Min(a, b);

            public double Zero => 0.0;
            public double One => 1.0;
        }

        /// <summary>
        /// Operator implementation for int type.
        /// </summary>
        internal struct OpInt : IOperator<int>
        {
            public int Create<T>(T other) => Convert.ToInt32(other);

            public int Sum(int a, int b) => a + b;

            public int Times(int a, int b) => a * b;

            public int Divide(int a, int b) => a / b;

            public int Negate(int a) => -a;

            public int Max(int a, int b) => Math.Max(a, b);

            public int Min(int a, int b) => Math.Min(a, b);

            public int Zero => 0;
            public int One => 1;
        }

#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Operator implementation for Vector3 type.
        /// </summary>
        internal struct OpVector3 : IOperator<Vector3>
        {
            /// <summary>
            /// Operator implementation for Vector3 type.
            /// </summary>
            public Vector3 Create<T>(T other)
            {
                var op = GetOperator<float>();
                var o = op.Create(other);
                return new Vector3(o, o, o);
            }

            public Vector3 Sum(Vector3 a, Vector3 b) => a + b;

            public Vector3 Times(Vector3 a, Vector3 b) => Vector3.Scale(a, b);

            public Vector3 Divide(Vector3 a, Vector3 b) =>
                Vector3.Scale(a, new Vector3(1f / b.x, 1f / b.y, 1f / b.z));

            public Vector3 Negate(Vector3 a) => -a;

            public Vector3 Max(Vector3 a, Vector3 b) => Vector3.Max(a, b);

            public Vector3 Min(Vector3 a, Vector3 b) => Vector3.Min(a, b);

            public Vector3 Zero => Vector3.zero;
            public Vector3 One => Vector3.one;
        }
#endif

        /// <summary>
        /// Gets the operator instance for the specified type. Not quite zero cost since this boxes the struct.
        /// </summary>
        /// <typeparam name="S">The type of the operator.</typeparam>
        /// <returns>The operator instance.</returns>
        public static IOperator<S> GetOperator<S>()
        {
            switch (Type.GetTypeCode(typeof(S)))
            {
                case TypeCode.Double:
                    return (IOperator<S>)(object)default(OpDouble);
                case TypeCode.Single:
                    return (IOperator<S>)(object)default(OpFloat);
                case TypeCode.Int32:
                    return (IOperator<S>)(object)default(OpInt);
                case TypeCode.Object:
#if UNITY_5_3_OR_NEWER
                    if (typeof(S) == typeof(Vector3))
                        return (IOperator<S>)(object)default(OpVector3);
                    else
                        goto default;
#endif
                default:
                    throw new NotImplementedException(
                        $"No IOperator<T> implementation for type {typeof(S)}."
                    );
            }
        }

        // Plus

        /// <summary>
        /// Creates a plus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Plus<S>(S v, string name = null)
            where S : struct => Plus(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a plus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Plus<S>(
            IReadOnlyValue<S> v,
            string name = null
        ) => new NumericalModifier<IReadOnlyValue<S>, S>(v) { Name = name, Symbol = '+' };

        /// <summary>
        /// Creates a plus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The plus modifier.</returns>
        public static IModifier<IValue<S>, S> Plus<S>(IValue<S> v, string name = null) =>
            new NumericalModifier<IValue<S>, S>(v) { Name = name, Symbol = '+' };

        // Times

        /// <summary>
        /// Creates a times modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The times modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Times<S>(S v, string name = null)
            where S : struct => Times(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a times modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The times modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Times<S>(
            IReadOnlyValue<S> v,
            string name = null
        ) => new NumericalModifier<IReadOnlyValue<S>, S>(v) { Name = name, Symbol = '*' };

        /// <summary>
        /// Creates a times modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The times modifier.</returns>
        public static IModifier<IValue<S>, S> Times<S>(IValue<S> v, string name = null) =>
            new NumericalModifier<IValue<S>, S>(v) { Name = name, Symbol = '*' };

        // Minus

        /// <summary>
        /// Creates a minus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The minus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Minus<S>(S v, string name = null)
            where S : struct => Minus(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a minus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The minus modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Minus<S>(
            IReadOnlyValue<S> v,
            string name = null
        ) => new NumericalModifier<IReadOnlyValue<S>, S>(v) { Name = name, Symbol = '-' };

        /// <summary>
        /// Creates a minus modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The minus modifier.</returns>
        public static IModifier<IValue<S>, S> Minus<S>(IValue<S> v, string name = null) =>
            new NumericalModifier<IValue<S>, S>(v) { Name = name, Symbol = '-' };

        // Divide

        /// <summary>
        /// Creates a divide modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The divide modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Divide<S>(S v, string name = null)
            where S : struct => Divide(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a divide modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The divide modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Divide<S>(
            IReadOnlyValue<S> v,
            string name = null
        ) => new NumericalModifier<IReadOnlyValue<S>, S>(v) { Name = name, Symbol = '/' };

        /// <summary>
        /// Creates a divide modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The divide modifier.</returns>
        public static IModifier<IValue<S>, S> Divide<S>(IValue<S> v, string name = null) =>
            new NumericalModifier<IValue<S>, S>(v) { Name = name, Symbol = '/' };

        // Substitute

        /// <summary>
        /// Creates a substitute modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The substitute modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Substitute<S>(S v, string name = null)
            where S : struct => Substitute(new ReadOnlyValue<S>(v), name);

        /// <summary>
        /// Creates a substitute modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The substitute modifier.</returns>
        public static IModifier<IReadOnlyValue<S>, S> Substitute<S>(
            IReadOnlyValue<S> v,
            string name = null
        ) => new NumericalModifier<IReadOnlyValue<S>, S>(v) { Name = name, Symbol = '=' };

        /// <summary>
        /// Creates a substitute modifier with the specified value and name.
        /// </summary>
        /// <typeparam name="S">The type of the value.</typeparam>
        /// <param name="v">The value.</param>
        /// <param name="name">The name of the modifier.</param>
        /// <returns>The substitute modifier.</returns>
        public static IModifier<IValue<S>, S> Substitute<S>(IValue<S> v, string name = null) =>
            new NumericalModifier<IValue<S>, S>(v) { Name = name, Symbol = '=' };
#endif

        /// <summary>
        /// Casts a numerical type into something else.
        /// </summary>
        /// <typeparam name="S">The type of the source numerical type.</typeparam>
        /// /// <typeparam name="T">The type of the target numerical type.</typeparam>
        internal class CastingModifier<S, T> : ContextModifier<IModifier<S>, T>
#if NET7_0_OR_GREATER
            where S : INumber<S>
            where T : INumber<T>
#endif
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CastingModifier{S, T}"/> class with the specified context.
            /// </summary>
            /// <param name="context">The context.</param>
            public CastingModifier(IModifier<S> context)
                : base(context) { }

#if NET7_0_OR_GREATER
            public override T Modify(T given) => T.Create(context.Modify(S.Create(given)));
#else
            public override T Modify(T given)
            {
                var s = GetOperator<S>();
                var t = GetOperator<T>();
                return t.Create(Context.Modify(s.Create(given)));
            }
#endif
        }
    }

    /// <summary>
    /// An abstract modifier that keeps a particular context about it.
    /// If that context implements <see cref="INotifyPropertyChanged"/>, its events will provoke
    /// this modifier's PropertyChanged events.
    /// </summary>
    /// <typeparam name="S">The type of the context.</typeparam>
    /// <typeparam name="T">The type of the value to modify.</typeparam>
    public abstract class ContextModifier<S, T> : IModifier<S, T>, IDisposable
    {
        /// <summary>
        /// Gets or sets the name of the modifier.
        /// </summary>
        public string Name { get; init; }
        private bool _enabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether the modifier is enabled.
        /// </summary>
        public virtual bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;
                _enabled = value;
                OnChange(nameof(Enabled));
            }
        }

        public S Context { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextModifier{S, T}"/> class with the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public ContextModifier(S context)
        {
            if (context is INotifyPropertyChanged notify)
                notify.PropertyChanged += Chain;
            Context = context;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property name.
        /// </summary>
        /// <param name="name">The name of the property that changed.</param>
        protected void OnChange(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Handles the PropertyChanged event of the chained context.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        internal void Chain(object sender, PropertyChangedEventArgs args) =>
            OnChange(nameof(Context));

        /// <summary>
        /// Modifies the given value.
        /// </summary>
        /// <param name="given">The value to modify.</param>
        /// <returns>The modified value.</returns>
        public abstract T Modify(T given);

        public void Dispose()
        {
            if (Context is INotifyPropertyChanged notify)
                notify.PropertyChanged -= Chain;
        }

        /// <summary>
        /// Returns a string representation of the modifier.
        /// </summary>
        /// <returns>A string representation of the modifier.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Name != null)
            {
                builder.Append('"');
                builder.Append(Name);
                builder.Append('"');
                builder.Append(' ');
            }

            builder.Append(Context);
            return builder.ToString();
        }
    }

    /// <summary>
    /// Represents a numerical modifier that operates on a context value.
    /// </summary>
    /// <typeparam name="S">The type of the context value.</typeparam>
    /// <typeparam name="T">The numeric type.</typeparam>
    public class NumericalModifier<S, T> : ContextModifier<S, T>
        where S : IReadOnlyValue<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        /// <summary>
        /// Gets or sets the symbol representing the operation of the modifier.
        /// </summary>
        public char Symbol { get; init; } = '?';

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericalModifier{S, T}"/> class with the specified context.
        /// </summary>
        /// <param name="context">The context value.</param>
        public NumericalModifier(S context)
            : base(context) { }

#if NET7_0_OR_GREATER
        public override T Modify(T given)
        {
            T v = context.value;
            switch (symbol)
            {
                case '+':
                    return given + v;
                case '-':
                    return given - v;
                case '*':
                    return given * v;
                case '/':
                    return given / v;
                case '=':
                    return v;
                default:
                    throw new NotImplementedException("Unsupported symbol: " + symbol);
            }
        }
#else
        public override T Modify(T given)
        {
            var t = Modifier.GetOperator<T>();
            T v = Context.Value;
            switch (Symbol)
            {
                case '+':
                    return t.Sum(given, v);
                case '-':
                    return t.Sum(given, t.Negate(v));
                case '*':
                    return t.Times(given, v);
                case '/':
                    return t.Divide(given, v);
                case '=':
                    return v;
                default:
                    throw new NotImplementedException("Unsupported symbol: " + Symbol);
            }
        }
#endif

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            // builder.Append("ref ");
            if (Name != null)
            {
                // Append the name enclosed in double quotes
                builder.Append('"');
                builder.Append(Name);
                builder.Append('"');
                builder.Append(' ');
            }
            builder.Append(Symbol);

            builder.Append(Context);

            return builder.ToString();
        }
    }

    /// <summary>
    /// Provides extension methods for modifying a bag with targeted modifiers.
    /// </summary>
    public static class TargetedModifierExtensions
    {
        /// <summary>
        /// Adds the modifier associated with the applicator to the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag to which the modifier will be added.</param>
        public static void AddToBag<S, T>(this ITarget<S, T> applicator, S bag) =>
            applicator.AppliesTo(bag).Modifiers.Add(applicator.Modifier);

        /// <summary>
        /// Removes the modifier associated with the applicator from the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag from which the modifier will be removed.</param>
        /// <returns>True if the modifier was successfully removed, otherwise false.</returns>
        public static bool RemoveFromBag<S, T>(this ITarget<S, T> applicator, S bag) =>
            applicator.AppliesTo(bag).Modifiers.Remove(applicator.Modifier);

        /// <summary>
        /// Checks if the modifier associated with the applicator is contained in the bag.
        /// </summary>
        /// <typeparam name="S">The type of the bag.</typeparam>
        /// <typeparam name="T">The type of the modifier.</typeparam>
        /// <param name="applicator">The applicator implementing ITarget<S, T>.</param>
        /// <param name="bag">The bag to check for the presence of the modifier.</param>
        /// <returns>True if the modifier is contained in the bag, otherwise false.</returns>
        public static bool ContainedInBag<S, T>(this ITarget<S, T> applicator, S bag) =>
            applicator.AppliesTo(bag).Modifiers.Contains(applicator.Modifier);
    }
}
