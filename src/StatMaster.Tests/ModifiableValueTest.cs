using StatMaster;

namespace StatMaster.Tests
{
    public class ModifiableValueTest
    {
        private ModifiableValue<float> health;
        private IModifiable<float> currentHealth;
        private IModifier<float> boost;
        private IModifier<float> boost20;
        private IModifier<IValue<float>, float> damage;

        private int healthNotifications = 0;
        private int currentHealthNotifications = 0;
        private int damageNotifications = 0;
        private int boostNotifications = 0;

        public ModifiableValueTest()
        {
            InitializeValues();
            InitializeModifiers();
            AttachEventHandlers();
        }

        private void InitializeValues()
        {
            health = new ModifiableValue<float>(100f);
            currentHealth = new ModifiableReadOnlyValue<float>(health);
        }

        private void InitializeModifiers()
        {
            boost = Modifier.Times(1.10f, "10% boost");
            boost20 = Modifier.Times(1.2f, "20% boost");
            damage = Modifier.Plus(new PropertyValue<float>(), "damage");
            health.Modifiers.Add(boost);
            currentHealth.Modifiers.Add(damage);
        }

        private void AttachEventHandlers()
        {
            health.PropertyChanged += (_, _) => healthNotifications++;
            currentHealth.PropertyChanged += (_, _) => currentHealthNotifications++;
            damage.PropertyChanged += (_, _) => damageNotifications++;
            boost.PropertyChanged += (_, _) => boostNotifications++;
        }

        [Fact]
        public void PropertyValue_Test()
        {
            // Arrange
            var v = new PropertyValue<int> { Value = 1 };
            var iv = (IReadOnlyValue<int>)v;

            // Assert initial and current values
            Assert.Equal(1, v.Value);
            Assert.Equal(1, iv.Value);

            // Act
            v.Value = 2;

            // Assert new values
            Assert.Equal(2, v.Value);
            Assert.Equal(2, iv.Value);
        }

        [Fact]
        public void Unmodified_Test()
        {
            // Clear all modifiers from health
            health.Modifiers.Clear();

            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(100f, health.Value);

            // Assert notification counts
            Assert.Equal(1, healthNotifications);
            Assert.Equal(1, currentHealthNotifications);
            Assert.Equal(0, damageNotifications);
            Assert.Equal(0, boostNotifications);
        }

        [Fact]
        public void Modified_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Assert notification counts
            Assert.Equal(0, healthNotifications);
            Assert.Equal(0, currentHealthNotifications);
            Assert.Equal(0, damageNotifications);
            Assert.Equal(0, boostNotifications);
        }

        [Fact]
        public void Disabled_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Act
            boost.Enabled = false;

            // Assert current health value
            Assert.Equal(100f, health.Value);

            // Assert notification counts
            Assert.Equal(1, healthNotifications);
            Assert.Equal(1, currentHealthNotifications);
            Assert.Equal(0, damageNotifications);
            Assert.Equal(1, boostNotifications);
        }

        [Fact]
        public void Notification_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Act
            damage.Context.Value = 10f;

            // Assert notification counts
            Assert.Equal(0, healthNotifications);
            Assert.Equal(1, currentHealthNotifications);
            Assert.Equal(1, damageNotifications);
            Assert.Equal(0, boostNotifications);
        }

        [Fact]
        public void NotificationOnAdd_Test()
        {
            // Assert initial and current health values
            Assert.Equal(100f, health.Initial.Value);
            Assert.Equal(110f, health.Value);

            // Assert notification counts
            Assert.Equal(0, healthNotifications);
            Assert.Equal(0, currentHealthNotifications);
            Assert.Equal(0, damageNotifications);
            Assert.Equal(0, boostNotifications);

            // Act
            health.Modifiers.Add(boost20);

            // Assert current health value
            Assert.Equal(132f, health.Value);

            // Assert notification counts
            Assert.Equal(1, healthNotifications);
            Assert.Equal(1, currentHealthNotifications);
            Assert.Equal(0, damageNotifications);
            Assert.Equal(0, boostNotifications);
        }

        [Fact]
        public void Stat_ToString_Test()
        {
            // Assert initial and current health values
            Assert.Equal("110", health.ToString());
            Assert.Equal("110", currentHealth.ToString());
        }

        [Fact]
        public void Modifier_ToString_Test()
        {
            var m = Modifier.Plus(1);
            Assert.Equal("+1", m.ToString());

            m = Modifier.Plus(1, "+1 sword");
            Assert.Equal("\"+1 sword\" +1", m.ToString());

            m = Modifier.Times(2, "blah");
            Assert.Equal("\"blah\" *2", m.ToString());
        }

        [Fact]
        public void Different_AccumulationStyle_Test()
        {
            // Arrange
            var strength = new ModifiableValue<float>(10f);
            var strengthPercentageGain = new ModifiableValue<float>(1f);

            // Act
            strengthPercentageGain.Modifiers.Add(Modifier.Plus(0.10f));
            strength.Modifiers.Add(Modifier.Times<float>(strengthPercentageGain));

            // Assert
            Assert.Equal(11f, strength.Value);
        }

        [Fact]
        public void Different_AccumulationStyle_MixedTypes_Test()
        {
            // Arrange
            var strength = new ModifiableValue<int>(10);
            var strengthPercentageGain = new ModifiableValue<float>(1f);

            // Act
            strengthPercentageGain.Modifiers.Add(Modifier.Plus(0.1f));
            strength.Modifiers.Add(
                Modifier.Times<float>(strengthPercentageGain).Cast<float, int>()
            );

            // Assert
            Assert.Equal(11, strength.Value);
        }

        [Fact]
        public void Character_AccumulationStyle_Test()
        {
            // Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.

            // Arrange
            var stat = new ModifiableValue<float>(10f);
            var baseFlatPlus = new ModifiableValue<float>();
            var baseTimes = new ModifiableValue<float>(1f);
            var basePlus = new ModifiableValue<float>();
            var totalTimes = new ModifiableValue<float>(1f);
            var totalPlus = new ModifiableValue<float>();

            Assert.True(stat is IReadOnlyValue<float>);
            Assert.True(baseFlatPlus is IReadOnlyValue<float>);
            Assert.True(baseTimes is IReadOnlyValue<float>);
            Assert.True(basePlus is IReadOnlyValue<float>);
            Assert.True(totalTimes is IReadOnlyValue<float>);
            Assert.True(totalPlus is IReadOnlyValue<float>);

            stat.Modifiers.Add(Modifier.Plus<float>(baseFlatPlus));
            stat.Modifiers.Add(Modifier.Times<float>(baseTimes));
            stat.Modifiers.Add(Modifier.Plus<float>(basePlus));
            stat.Modifiers.Add(Modifier.Times<float>(totalTimes));
            stat.Modifiers.Add(Modifier.Plus<float>(totalPlus));

            Assert.Equal(10f, stat.Value);

            // Act
            baseFlatPlus.Modifiers.Add(Modifier.Plus(1f));
            Assert.Equal(11f, stat.Value);

            baseTimes.Modifiers.Add(Modifier.Plus(1f));
            Assert.Equal(22f, stat.Value);

            basePlus.Modifiers.Add(Modifier.Plus(3f));
            Assert.Equal(25f, stat.Value);

            totalTimes.Modifiers.Add(Modifier.Plus(1f));
            Assert.Equal(50f, stat.Value);

            totalPlus.Modifiers.Add(Modifier.Plus(5f));
            Assert.Equal(55f, stat.Value);
        }

        [Fact]
        public void Ways_ToAdd_Test()
        {
            // Arrange
            var stat = new ModifiableValue<float>(10f);
            var baseFlatPlus = new ModifiableValue<float>(2f);

            Assert.True(stat is IReadOnlyValue<float>);
            Assert.True(baseFlatPlus is IReadOnlyValue<float>);

            var m = Modifier.Plus((IReadOnlyValue<float>)baseFlatPlus);

            // Act
            stat.Modifiers.Add(m);
            stat.Modifiers.Add(Modifier.Plus<float>(baseFlatPlus));
            stat.Modifiers.Add(Modifier.Plus<float>(baseFlatPlus));

            // Assert
            Assert.Equal(16f, stat.Value);
        }

        [Fact]
        public void Ways_ToAdd_Literals_Test()
        {
            // Arrange
            var stat = new ModifiableValue<float>(10f);
            var m = Modifier.Plus(2f);

            // Act
            stat.Modifiers.Add(m);
            stat.Modifiers.Add(Modifier.Plus(2f));
            stat.Modifiers.Add(Modifier.Plus<float>(2f));

            // Assert
            Assert.Equal(16f, stat.Value);
        }

        [Fact]
        public void CharacterStyle_Test()
        {
            // Arrange
            int notifications = 0;
            int notifications2 = 0;
            var stat = new CharacterStat<float>(10f);

            stat.PropertyChanged += (_, _) => notifications++;
            stat.BaseFlatPlus.PropertyChanged += (_, _) => notifications2++;

            Assert.Equal(0, notifications);
            Assert.Equal(0, notifications2);
            Assert.Equal(10f, stat.Value);

            // Act
            stat.BaseFlatPlus.Modifiers.Add(Modifier.Plus(1f));

            Assert.Equal(1, notifications);
            Assert.Equal(1, notifications2);
            Assert.Equal(11f, stat.Value);

            stat.BaseFlatPlus.Modifiers.Add(Modifier.Plus(1f));

            // Assert
            Assert.Equal(2, notifications);
            Assert.Equal(2, notifications2);
            Assert.Equal(12f, stat.Value);
        }

        [Fact]
        public void ModifierPriority_Test()
        {
            health.Modifiers.Clear();

            health.Modifiers.Add(boost);
            health.Modifiers.Add(-10, damage);

            Assert.Equal(damage, health.Modifiers.First());
            Assert.Equal(boost, health.Modifiers.Skip(1).First());

            health.Modifiers.Add(boost20);

            Assert.Equal(boost20, health.Modifiers.Skip(2).First());
        }
    }
}
