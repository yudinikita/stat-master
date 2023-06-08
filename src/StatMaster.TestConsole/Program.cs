using StatMaster;

var health = new ModifiableValue<float>(100f);

Console.WriteLine($"Health is {health.Value}.");

// Output: Health is 100.

health.Modifiers.Add(Modifier.Times(1.10f));

Console.WriteLine($"Health is {health.Value}.");

// Output: Health is 110.

health.Modifiers.Add(Modifier.Plus(5f, "+5 health"));

Console.WriteLine($"Health is {health.Value}.");

// Output: Health is 115.
