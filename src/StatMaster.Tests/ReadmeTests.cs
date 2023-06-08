using StatMaster;

namespace StatMaster.Tests
{
    public class ReadmeTests
    {
        [Fact]
        public void Health_Modification_Test()
        {
            // Arrange
            var health = new ModifiableValue<float>(100f);

            // Assert initial health value
            Assert.Equal(100f, health.Value);

            // Act - Apply modifiers
            health.Modifiers.Add(Modifier.Times(1.10f));
            health.Modifiers.Add(Modifier.Plus(5f, "+5 health"));

            // Assert modified health value
            Assert.Equal(115f, health.Value);
        }

        [Fact]
        public void Damage_Modification_Test()
        {
            // Arrange
            var damage = new ModifiableValue<float>(10f);
            int notificationCount = 0;
            damage.PropertyChanged += (_, _) => notificationCount++;

            // Assert initial damage value and notification count
            Assert.Equal(10f, damage.Value);
            Assert.Equal(0, notificationCount);

            // Act - Apply modifiers
            damage.Modifiers.Add(Modifier.Times(1.50f));
            damage.Modifiers.Add(Modifier.Plus(3f, "+3 damage"));

            // Assert modified damage value and notification count
            Assert.Equal(18f, damage.Value);
            Assert.Equal(2, notificationCount);
        }

        [Fact]
        public void Health_Bounds_Test()
        {
            // Arrange
            var maxHealth = new ModifiableValue<float>(100f);
            var health = new BoundedValue<float>(maxHealth.Value, 0f, maxHealth);
            int notificationCount = 0;
            health.PropertyChanged += (_, _) => notificationCount++;

            // Assert initial health and notification count
            Assert.Equal(0, notificationCount);
            Assert.Equal(100f, health.Value);
            Assert.Equal(100f, maxHealth.Value);

            // Act - Modify health
            health.Value -= 10f;
            maxHealth.Modifiers.Add(Modifier.Plus(20f, "+20 level gain"));

            // Assert modified health and maxHealth
            Assert.Equal(90f, health.Value);
            Assert.Equal(120f, maxHealth.Value);
        }

        [Fact]
        public void Health_Calculation_Test()
        {
            // Arrange
            var constitution = new ModifiableValue<int>(10);
            int level = 10;
            var hpAdjustment = constitution.Select(
                con => (float)Math.Round((con - 10f) / 3f) * level
            );
            var maxHealth = new ModifiableValue<float>(100f);
            int notificationCount = 0;
            maxHealth.PropertyChanged += (_, _) => notificationCount++;

            // Act - Apply modifiers
            maxHealth.Modifiers.Add(Modifier.Plus(hpAdjustment));

            // Assert initial max health and notification count
            Assert.Equal(100f, maxHealth.Value);
            Assert.Equal(1, notificationCount);

            // Act - Modify constitution
            constitution.Initial.Value = 15;

            // Assert modified max health and notification count
            Assert.Equal(120f, maxHealth.Value);
            Assert.Equal(2, notificationCount);
        }

        [Fact]
        public void Constitution_WithZip_Test()
        {
            // Arrange
            var constitution = new ModifiableValue<int>(10);
            var level = new PropertyValue<int>(10);
            var hpAdjustment = constitution.Zip(
                level,
                (con, lev) => (float)Math.Round((con - 10f) / 3f) * lev
            );
            var maxHealth = new ModifiableValue<float>(100f);
            int notificationCount = 0;
            maxHealth.PropertyChanged += (_, _) => notificationCount++;

            // Act - Apply modifiers
            maxHealth.Modifiers.Add(Modifier.Plus(hpAdjustment));

            // Assert initial max health and notification count
            Assert.Equal(100f, maxHealth.Value);
            Assert.Equal(1, notificationCount);

            // Act - Modify constitution
            constitution.Initial.Value = 15;

            // Assert modified max health and notification count
            Assert.Equal(120f, maxHealth.Value);
            Assert.Equal(2, notificationCount);

            // Act - Modify level
            level.Value = 15;

            // Assert modified max health and notification count
            Assert.Equal(130f, maxHealth.Value);
            Assert.Equal(3, notificationCount);
        }
    }
}
