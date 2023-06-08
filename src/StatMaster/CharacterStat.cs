#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace StatMaster
{
    /// <summary>
    /// Represents a character stat that can be modified.
    /// Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
    /// </summary>
    public class CharacterStat<T> : ModifiableValue<T>
#if NET7_0_OR_GREATER
        where T : INumber<T>
#endif
    {
        /// <summary>
        /// An additional value added to the base value.
        /// Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
        /// </summary>
        public IModifiableValue<T> BaseFlatPlus { get; }

        /// <summary>
        /// An additional value added to the result after applying BaseTimes.
        /// Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
        /// </summary>
        public IModifiableValue<T> BasePlus { get; }

        /// <summary>
        /// A multiplier applied to the base value.
        /// Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
        /// </summary>
        public IModifiableValue<T> BaseTimes { get; }

        /// <summary>
        /// An additional value added to the final result.
        /// Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
        /// </summary>
        public IModifiableValue<T> TotalPlus { get; }

        /// <summary>
        /// Another multiplier applied to the result of BaseTimes and BasePlus.
        /// Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
        /// </summary>
        public IModifiableValue<T> TotalTimes { get; }

        /// <summary>
        /// Represents a character stat that can be modified.
        /// </summary>
        /// <param name="initialValue">The initial value of the character stat.</param>
        public CharacterStat(T initialValue)
            : base(initialValue)
        {
            BaseFlatPlus = new ModifiableValue<T>();
            BasePlus = new ModifiableValue<T>();
            BaseTimes = new ModifiableValue<T>(One());
            TotalPlus = new ModifiableValue<T>();
            TotalTimes = new ModifiableValue<T>(One());

            InitializeModifiers();
        }

        /// <summary>
        /// Initializes the modifiers for the character stat.
        /// </summary>
        private void InitializeModifiers()
        {
            Modifiers.Add(100, Modifier.Plus(BaseFlatPlus));
            Modifiers.Add(200, Modifier.Times(BaseTimes));
            Modifiers.Add(300, Modifier.Plus(BasePlus));
            Modifiers.Add(400, Modifier.Times(TotalTimes));
            Modifiers.Add(500, Modifier.Plus(TotalPlus));
        }

        #region Utility Methods

        /// <summary>
        /// Returns the value "one" of type T.
        /// </summary>
        /// <returns>The value "one" of type T.</returns>
        private static T One()
        {
#if NET7_0_OR_GREATER
            return T.One;
#else
            return Modifier.GetOperator<T>().One;
#endif
        }

        #endregion Utility Methods
    }
}
