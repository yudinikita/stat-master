using StatMaster;

namespace StatMaster.Tests
{
    public class ModifierTest
    {
        [Fact]
        public void Modifier_ToString_Test()
        {
            // This trick works in dotnet core, not in Unity 2021.3 though.
            var m = Modifier.Create((int x) => x + 1);
            Assert.Equal("(int x) => x + 1", m.ToString());

            var n = Modifier.Create((int x) => x + 1, "+1 strength");
            Assert.Equal("+1 strength", n.ToString());
        }

        [Fact]
        public void Covariance_Test()
        {
            IModifier<IValue<int>, int> m = Modifier.Plus(new PropertyValue<int>(1));
            Assert.True(m is IModifier<IValue<int>, int>);
            Assert.True(m is IModifier<IReadOnlyValue<int>, int>);
            IModifier<IReadOnlyValue<int>, int> n = (IModifier<IReadOnlyValue<int>, int>)m;
            Assert.True(n is IModifier<IReadOnlyValue<int>, int>);
        }
    }
}
