using StatMaster;

namespace StatMaster.Tests
{
    public class ValueTests
    {
        private PropertyValue<int> a;
        private IValue<int> b;

        public ValueTests()
        {
            a = new PropertyValue<int>();
            b = a.Select(x => x + 1, (v, x) => v.Value = x - 1);
        }

        [Fact]
        public void Value_Select_Test()
        {
            Assert.Equal(0, a.Value);
            Assert.Equal(1, b.Value);

            a.Value = 2;
            Assert.Equal(2, a.Value);
            Assert.Equal(3, b.Value);

            b.Value = 3;
            Assert.Equal(2, a.Value);
            Assert.Equal(3, b.Value);

            b.Value = 4;
            Assert.Equal(3, a.Value);
            Assert.Equal(4, b.Value);
        }

        [Fact]
        public void Bounded_InputValue_Test()
        {
            var boundedValue = new BoundedValue<float>(100f, 0f, 100f);
            var v = new ModifiableValue<float>(boundedValue);

            v.Modifiers.Add(Modifier.Plus(10f));
            Assert.Equal(110f, v.Value);

            boundedValue.Value = 200f;
            Assert.Equal(110f, v.Value);
        }

        [Fact]
        public void Bounded_OutputValue_Test()
        {
            var v = new BoundedModifiable<IValue<float>, float>(
                new PropertyValue<float>(100f),
                0f,
                100f
            );

            v.Modifiers.Add(Modifier.Plus(10f));
            Assert.Equal(100f, v.Value);

            v.Initial.Value = 200f;
            Assert.Equal(100f, v.Value);
        }

        [Fact]
        public void Bounded_InputOutputValue_Test()
        {
            var v = new BoundedModifiable<IValue<float>, float>(
                new BoundedValue<float>(100f, 0f, 100f),
                0f,
                100f
            );

            v.Modifiers.Add(Modifier.Plus(10f));
            Assert.Equal(100f, v.Value);

            v.Initial.Value = 200f;
            Assert.Equal(100f, v.Value);
        }
    }
}
