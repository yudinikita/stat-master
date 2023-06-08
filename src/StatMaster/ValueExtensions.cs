using System;
using System.ComponentModel;
using System.Collections;
using System.Threading;

namespace StatMaster
{
    /// <summary>
    /// Provides extension methods for value-related operations.
    /// </summary>
    public static class ValueExtensions
    {
        /// <summary>
        /// Projects the value of an <see cref="IReadOnlyValue{S}"/> using the specified function.
        /// </summary>
        /// <typeparam name="S">The source value type.</typeparam>
        /// <typeparam name="T">The projected value type.</typeparam>
        /// <param name="v">The source value.</param>
        /// <param name="func">The projection function.</param>
        /// <returns>The projected value.</returns>
        public static IReadOnlyValue<T> Select<S, T>(this IReadOnlyValue<S> v, Func<S, T> func)
        {
            var w = ReadOnlyValue.Create(() => func(v.Value), out var callOnChange);
            v.PropertyChanged += (_, _) => callOnChange();
            return w;
        }

        /// <summary>
        /// Combines the values of two <see cref="IReadOnlyValue{T}"/> instances using the specified function.
        /// </summary>
        /// <typeparam name="S">The first value type.</typeparam>
        /// <typeparam name="T">The second value type.</typeparam>
        /// <typeparam name="U">The resulting value type.</typeparam>
        /// <param name="v">The first value.</param>
        /// <param name="w">The second value.</param>
        /// <param name="func">The combining function.</param>
        /// <returns>The combined value.</returns>
        public static IReadOnlyValue<U> Zip<S, T, U>(
            this IReadOnlyValue<S> v,
            IReadOnlyValue<T> w,
            Func<S, T, U> func
        )
        {
            var u = ReadOnlyValue.Create(() => func(v.Value, w.Value), out var callOnChange);
            v.PropertyChanged += (_, _) => callOnChange();
            w.PropertyChanged += (_, _) => callOnChange();
            return u;
        }

        /// <summary>
        /// Projects the value of an <see cref="IValue{S}"/> using the specified getter and setter.
        /// </summary>
        /// <typeparam name="S">The source value type.</typeparam>
        /// <typeparam name="T">The projected value type.</typeparam>
        /// <param name="v">The source value.</param>
        /// <param name="get">The getter function.</param>
        /// <param name="set">The setter action.</param>
        /// <returns>The projected value.</returns>
        public static IValue<T> Select<S, T>(
            this IValue<S> v,
            Func<S, T> get,
            Action<IValue<S>, T> set
        )
        {
            var w = PropertyValue.Create(() => get(v.Value), x => set(v, x), out var callOnChange);
            v.PropertyChanged += (_, _) => callOnChange();
            return w;
        }

        /// <summary>
        /// Represents a disposable object that executes an action when disposed.
        /// </summary>
        private class ActionDisposable : IDisposable
        {
            private Action action;

            public ActionDisposable(Action action) => this.action = action;

            public void Dispose()
            {
                action();
            }
        }

        /// <summary>
        /// Subscribes to the property change events of an object and executes the specified action.
        /// </summary>
        /// <typeparam name="T">The type of the object implementing <see cref="INotifyPropertyChanged"/>.</typeparam>
        /// <param name="v">The object to subscribe to.</param>
        /// <param name="action">The action to execute on property change.</param>
        /// <returns>An <see cref="IDisposable"/> representing the subscription.</returns>
        public static IDisposable OnChange<T>(this T v, Action<T> action)
            where T : INotifyPropertyChanged
        {
            v.PropertyChanged += PropertyChange;
            return new ActionDisposable(() => v.PropertyChanged -= PropertyChange);

            void PropertyChange(object source, PropertyChangedEventArgs args) => action(v);
        }

        /// <summary>
        /// Casts an <see cref="IModifier{X}"/> to an <see cref="IModifier{Y}"/>.
        /// </summary>
        /// <typeparam name="X">The source value type.</typeparam>
        /// <typeparam name="Y">The target value type.</typeparam>
        /// <param name="m">The modifier to cast.</param>
        /// <returns>The casted modifier.</returns>
        public static IModifier<Y> Cast<X, Y>(this IModifier<X> m)
#if NET7_0_OR_GREATER
            where X : INumber<X>
            where Y : INumber<Y>
#endif
        {
            return new Modifier.CastingModifier<X, Y>(m);
        }

#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Animates the value of an <see cref="IValue{float}"/> towards a target value over a specified duration.
        /// </summary>
        /// <param name="v">The value to animate.</param>
        /// <param name="targetValue">The target value.</param>
        /// <param name="duration">The duration of the animation.</param>
        /// <param name="period">The time period between updates during the animation.</param>
        /// <param name="token">The cancellation token to stop the animation.</param>
        /// <returns>An enumerator representing the animation.</returns>
        public static IEnumerator LerpTo(
            this IValue<float> v,
            float targetValue,
            float duration,
            float? period = null,
            CancellationToken token = default
        )
        {
            float startValue = v.Value;
            float start = UnityEngine.Time.time;
            float t = 0f;
            var wait = period.HasValue ? new UnityEngine.WaitForSeconds(period.Value) : null;
            do
            {
                t = (UnityEngine.Time.time - start) / duration;
                v.Value = UnityEngine.Mathf.Lerp(startValue, targetValue, t);
                yield return wait;
            } while (t <= 1f && !token.IsCancellationRequested);
            if (!token.IsCancellationRequested)
                v.Value = targetValue;
        }

        /// <summary>
        /// Creates a new <see cref="IReadOnlyValue{float}"/> that lerps to the current value of an <see cref="IReadOnlyValue{float}"/> over a specified duration whenever the source value changes.
        /// </summary>
        /// <param name="v">The source value.</param>
        /// <param name="component">The <see cref="UnityEngine.MonoBehaviour"/> to start the coroutine on.</param>
        /// <param name="duration">The duration of the animation.</param>
        /// <param name="period">The time period between updates during the animation.</param>
        /// <returns>The lerping value.</returns>
        public static IReadOnlyValue<float> LerpOnChange(
            this IReadOnlyValue<float> v,
            UnityEngine.MonoBehaviour component,
            float duration,
            float? period = null
        )
        {
            var w = new PropertyValue<float>(v.Value);
            var source = new CancellationTokenSource();
            var token = source.Token;
            bool isRunning = false;

            v.PropertyChanged += OnChange;

            return w;

            void OnChange(object sender, PropertyChangedEventArgs args)
            {
                if (isRunning)
                    source.Cancel();
                component.StartCoroutine(w.LerpTo(v.Value, duration, period, token));
            }
        }
#endif
    }
}
