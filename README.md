<p align="center">
   <img src="https://github.com/nblackninja/stat-master/assets/36636599/5183a20f-dcad-48cd-996b-883abe22022b" alt="StatMaster logo" width="150">
</p>

<h3 align="center">StatMaster</h3>

<p align="center">
  StatMaster is a flexible attribute system for game characters, providing modifiable and customizable character stats.
  <br>
</p>

# 📊 StatMaster

The purpose of StatMaster is to offer a robust and extensible attribute system for game characters. It allows game developers to easily define and manage various character stats with modifiable values, modifiers, and customizable behavior. The system aims to enhance gameplay by providing flexible attribute calculations and empowering designers to create diverse character builds.

This library provides a dynamic attribute system that allows for the creation and management of various attributes. Attributes can be defined with different data types and can be easily modified and accessed through the provided interfaces. Also, provides seamless interaction between different attributes. Attributes can be combined, compared, and used in calculations, providing a comprehensive framework for attribute-based systems.

## 🖥 System Requirements

- Programming Language: C#
- .NET Framework: .NET Standard 2 or higher (including .NET 7)
- Unity3D Compatibility: Compatible with Unity3D engine version 5.3 or newer
- Resource Requirements: The library has minimal resource requirements and can run on systems with standard hardware configurations.
- Operating System: Compatible with Windows, macOS, and Linux operating systems.
- System Dependencies: The library has minimal external dependencies and does not require any additional libraries or frameworks to function properly.
- Memory and Storage: The library's memory footprint is efficient and optimized for performance. The storage requirements depend on the size and complexity of the attribute data being managed but are generally minimal. Caching is also used.

## ✨ Features

- **Interface-Driven Architecture**

The foundation of this library revolves around a set of interfaces that empower users to seamlessly substitute their own implementations as per their requirements. This level of flexibility proves especially valuable when defining modifiers within Unity or other frameworks.

- **Versatile Type Support**

Unlike restrictive libraries that confine attributes to float or int types, this library liberates users to choose the most suitable data type for each attribute. These diverse attribute types can effortlessly interact with one another, fostering a seamless integration of different data types.

- **Advanced Generic Math Operations**

The absence of generic math support in previous versions of .NET may have deterred some libraries from embracing generics. However, with the forthcoming release of .NET 7, this library takes full advantage of the new generic math capabilities. Furthermore, a clever workaround has been implemented to maintain API compatibility with .NET Standard 2.0, enabling seamless integration with Unity3D and other platforms.

- **Flexible Modifier System**

While basic operations like addition, subtraction, multiplication, and division are commonplace when dealing with stats, this library takes it a step further. It empowers users to effortlessly implement custom modifiers or create ad hoc ones, enabling more intricate behaviors such as clamping attribute values and introducing complex logic.

- **Event System**

A built-in event system enhances the library by enabling event-driven interactions with attributes. Users can define events and listeners to respond dynamically to attribute changes, facilitating the creation of robust gameplay mechanics and system updates.

- **Serialization Support**

This library seamlessly supports the serialization of attribute values, making it compatible with save systems and enabling easy persistence of character or game state. Attribute values can be effortlessly saved and loaded, ensuring continuity and seamless gameplay experiences.

- **Extensibility and Modularity**

The design philosophy of the library prioritizes extensibility and modularity. Users can effortlessly extend or modify existing functionality, introduce new attribute modifiers, or even create custom attribute types tailored to their specific needs. This promotes adaptability and empowers developers to shape the attribute system according to their unique requirements.

- **Performance Optimization**

The library incorporates meticulous performance optimizations to guarantee efficient attribute calculations and minimize overhead. Critical paths have been carefully optimized, and unnecessary computations have been eliminated, resulting in enhanced performance and responsiveness.

- **Cross-Platform Compatibility**

The library has been meticulously engineered to ensure compatibility across multiple platforms and frameworks, including Unity3D, .NET, and other prevalent development environments. It adheres to industry standards and follows best practices, fostering seamless integration and compatibility across various platforms. This allows developers to leverage the library's capabilities irrespective of their preferred development environment.

## 📝 Getting started

### Installation

To start using the StatMaster library in your project, follow these steps:

1. Clone or download the StatMaster repository from [GitHub](https://github.com/nblackninja/stat-master.git):

``` bash
git clone https://github.com/nblackninja/stat-master.git
```

2. Open the project in your preferred Integrated Development Environment (IDE) such as Visual Studio or JetBrains Rider.

### Usage

Once you have the StatMaster library set up in your project, you can begin utilizing its features to define and manage character attributes. Here's how:

1. Build the solution to ensure all dependencies are resolved.
2. Add a reference to the StatMaster library in your game project.
3. Import the necessary namespaces to access the classes and interfaces.

```c#
using StatMaster;
```

4. Start using the provided classes and interfaces to define and manage character attributes.

## 🗂 Example of Use

``` c#
// Creating a ModifiableValue instance for health
var health = new ModifiableValue<float>(100f);

// Displaying the initial health value
Console.WriteLine($"Health is {health.Value}."); // Output: Health is 100.

// Adding a modifier to increase health by 10%
health.Modifiers.Add(Modifier.Times(1.10f, "+10% health"));

// Displaying the updated health value
Console.WriteLine($"Health is {health.Value}."); // Output: Health is 110.

// Adding a modifier to increase health by a flat value of 5
health.Modifiers.Add(Modifier.Plus(5f, "+5 health"));

// Displaying the final health value
Console.WriteLine($"Health is {health.Value}."); // Output: Health is 115.
```

For [more examples](#-other-examples), see below.

## Attribute

Fundamentally, an attribute consists of an `initial.value`. When no modifiers are applied, the attribute's `value` remains the same as its `initial.value`. However, the attribute's `value` can be altered by applying various modifiers, starting from its `initial.value`.

``` c#
public interface IModifiableValue<T>
{
    // Gets or sets the initial value of type T that serves as the base for modifications.
    T Initial.Value { get; set; }

    // Gets the current value of the attribute after applying all modifiers.
    T Value { get; }

    // Gets the collection of modifiers associated with the attribute.
    ICollection<IModifier<T>> Modifiers { get; }

    // Event triggered by a change of the value properties
    event PropertyChangedEventHandler PropertyChanged;
}
```

To provide further clarity, let's consider an attribute with an initial value of i and three modifiers: m_1, m_2, and m_3. The attribute's final value, denoted as v, is computed using the following expression:

$$ v = m_3(m_2(m_1(i))) $$

## Modifier

A modifier is a component that accepts a value and has the ability to change it in various ways.

``` c#
public interface IModifier<T> 
{
    // Gets or sets whether the modifier is enabled or disabled.
    bool Enabled { get; set; }

    // Modifies the given value and returns the modified result.
    T Modify(T given);

    // Event triggered when the modifier's properties change.
    event PropertyChangedEventHandler PropertyChanged;
}
```

However, in many cases, the desired changes are simple operations such as addition, multiplication, or substitution of values. To simplify these common modifications, convenient methods are provided for int, float, and double types.

``` c#
public static class Modifier 
{
    // Creates a modifier that adds the specified value to the given value.
    public static IModifier<T> Plus<T>(T value, string name = null);

      // Creates a modifier that subtracts the specified value from the given value.
    public static IModifier<T> Minus<T>(T value, string name = null);

    // Creates a modifier that multiplies the given value by the specified value.
    public static IModifier<T> Times<T>(T value, string name = null);

    // Creates a modifier that divides the given value by the specified value.
    public static IModifier<T> Divide<T>(T value, string name = null);

    // Creates a modifier that substitutes the given value with the specified value.
    public static IModifier<T> Substitute<T>(T value, string name = null);
}
```

## Change Propagation

To facilitate change notifications, these classes utilize the `INotifyPropertyChanged` interface. When a modifier is modified or added, it triggers a change event that propagates to its associated attribute. Consequently, the attribute notifies all its listeners about the change. This approach eliminates the need for constant polling to detect attribute modifications and ensures that any updates are immediately communicated to interested parties.

## Simplified API

The API presented above is simplified to highlight its key features in a clear manner. However, the actual code includes additional abstractions such as `IValue<T>` and `IReadOnlyValue<T>`. These abstractions are designed to enhance reusability by allowing attributes to be used as modifiers, among other functionalities. By incorporating these abstractions, the codebase gains flexibility and extensibility, enabling developers to leverage attributes in various contexts and scenarios.

### Handling Mathematical Operations with Generics

The introduction of generic math operators in .NET 7 is a highly anticipated feature that brings significant benefits. It enables us to write methods like the following:

``` c#
T Plus<T>(T a, T b) where T : INumber<T> => a + b;
```

This syntax was not valid in previous versions of .NET.

In the context of this attribute library, we leverage the power of generic math. However, we also need to ensure compatibility with the .NET Standard 2.0 framework, which is supported by platforms like Unity. The trick involves using an interface called IOperator<T> along with specific operator implementations for different types. Here's an example:

``` c#
interface IOperator<T>
{
    T Plus(T a, T b);
}

struct OpFloat : IOperator<float>
{
    public float Plus(float a, float b) => a + b;
}

void SomeProcessing<T, TOperator>(...) where TOperator : struct, IOperator<T>
{
    T var1 = ...;
    T var2 = ...;
    T sum = default(TOperator).Plus(var1, var2);  // This operation incurs zero additional cost!
}

void Caller()
{
    SomeProcessing<float, OpFloat>(...);
}
```

## 👀 Other Examples

### Using Notifications

To stay informed about attribute changes, you can make use of notifications. Here's an example:

``` c#
var damage = new ModifiableValue<float>(10f);
damage.PropertyChanged += (_, _) => Console.WriteLine($"Damage is {damage.Value}.");
damage.Modifiers.Add(Modifier.Times(1.50f, "+50% damage")); 
// Output: Damage is 15.
damage.Modifiers.Add(Modifier.Plus(3f, "+3 damage")); 
// Output: Damage is 18.
```

In this code snippet, we create a `damage` attribute using the `ModifiableValue<float>` class. By subscribing to the `PropertyChanged` event, we can receive notifications whenever the attribute's value changes. When a modifier is added, the event handler triggers, and we print out the updated value of the `damage` attribute.

### Modeling a Consumable Attribute

Let's create a current health value that is tied to a maximum health attribute.

``` c#
var maxHealth = new ModifiableValue<float>(100f);
var health = new BoundedValue<float>(maxHealth.Value, 0f, maxHealth);
health.PropertyChanged += (_, _) => Console.WriteLine($"Health is {health.Value}/{maxHealth.Value}.");
// Output: Health is 100/100.
health.Value -= 10f;
// Output: Health is 90/100.
maxHealth.modifiers.Add(Modifier.Plus(20f, "+20 level gain"));
// Output: Health is 90/120.
```

### Using an Attribute as a Modifier

In addition to creating static value modifiers like `Modifier.Plus(20f)`, it is also possible to create dynamic modifiers based on other values or attributes.

Let's consider a scenario where the "max health" attribute is influenced by the "constitution" attribute.

``` c#
var constitution = new ModifiableValue<int>(10);
int level = 10;
// We can project values using limited LINQ-like extension methods.
var hpAdjustment = constitution.Select(con => (float)Math.Round((con - 10f) / 3f) * level);
var maxHealth = new ModifiableValue<float>(100f);
maxHealth.PropertyChanged += (_, _) => Console.WriteLine($"Max health is {maxHealth.Value}.");
maxHealth.Modifiers.Add(Modifier.Plus(hpAdjustment));
// Output: Max health is 100.
constitution.Initial.Value = 15;
// Output: Max health is 120.
```

In this example, we create a constitution attribute using the ModifiableValue<int> class with an initial value of 10. We also define a level variable with a value of 10.

It's worth noting that hpAdjustment depends on the value of level. However, since level is an integer, changes to it will not automatically notify hpAdjustment or maxHealth. For an elegant solution to handle changes in level, please refer to the Advanced Examples.

### Creating Custom Modifiers

You can create custom modifiers by implementing the `IModifier<T>` interface or by using the convenience methods available in the `Modifier` class, such as `FromFunc()`, as demonstrated in the example below. Let's consider the scenario where armor bestows different effects depending on the phase of the moon.

``` c#
var moonArmor = new ModifiableValue<float>(20f);
moonArmor.Modifiers.Add(Modifier.Create((float x) => DateTime.Now.IsFullMoon() ? 2 * x : x));
```

In this example, we create a `moonArmor` attribute using the `ModifiableValue<float>` class with an initial value of 20. We want the armor to have different effects depending on the phase of the moon.

Using the Modifier.Create method and a lambda function, we define a custom modifier that multiplies the value by 2 if the current date and time indicate a full moon using the DateTime.Now.IsFullMoon() extension method. Otherwise, the value remains unchanged.

By adding this custom modifier to the `moonArmor` attribute, the effect of the armor will vary dynamically based on the current phase of the moon.

> Please note that the `IsFullMoon()` method used in this example is fictional and serves as a placeholder for a custom logic that determines the moon phase. ↩

### Ordering Modifiers

The order in which modifiers are applied can be controlled by assigning them priorities. Each modifier is assigned a priority value, with the default being 0. Modifiers with lower priority numbers are applied first, while those with higher priority numbers are applied later. If multiple modifiers have the same priority, they are applied in the order they were added.

``` c#
var maxMana = new ModifiableValue<float>(50f);
var mana = new Modifiable<IReadOnlyValue<float>, float>(maxMana); // maxMana is an IReadOnlyValue.
var manaCost = Modifier.Minus(0f);
mana.Modifiers.Add(manaCost);
mana.PropertyChanged += (_, _) => Console.WriteLine($"Mana is {mana.Value}/{maxMana.Value}.");
mana.Modifiers.Add(priority: 100, Modifier.Create((float x) => Math.Clamp(x, 0, maxMana.Value)));
// Output: Mana is 50/50.
manaCost.Value = 1000f;
// Output: Mana is 0/50.
```

In this example, we define a maxMana attribute with an initial value of 50. The mana attribute is created using the `Modifiable<IReadOnlyValue<float>`, float> class, with maxMana passed as the initial value.

By using priorities and custom modifiers, we can control the order of application and enforce value constraints in attribute modifications.

### Applying a Time-Based Modifier

To temporarily enable or disable a modifier for a specific duration, you can use the `EnableAfter()` and `DisableAfter()` extension methods available for `IModifier<T>`.

In the following example, we demonstrate how to enable and disable a power-up modifier on an armor attribute after a certain time period:

``` c#
var armor = new ModifiableValue<int>(10);
var powerUp = Modifier.Plus(5);
armor.Modifiers.Add(powerUp);
armor.PropertyChanged += (_, _) => Console.WriteLine($"Armor is {armor.Value}.");
// Output: Armor is 15.
powerUp.DisableAfter(TimeSpan.FromSeconds(5f));
// ...
// [Wait 5 seconds.]
// Output: Armor is 10.
```

By utilizing the `EnableAfter()` and `DisableAfter()` extension methods, you can easily control the activation and deactivation of modifiers based on time intervals, providing dynamic behavior to your attribute system.

## 💪 Advanced Examples

### Combining Multiple Values

To address the issue where `hpAdjustment` does not update when `level` changes in the previous example, we can leverage the `Zip()` extension method inspired by LINQ. By using `Zip()`, we can synthesize multiple values and ensure that changes to any of them will trigger notifications for dependent attributes.

Consider the following example where we synthesize the values of constitution and level:

``` c#
var constitution = new ModifiableValue<int>(10);
var level = new PropertyValue<int>(10);

// We can combine values using limited LINQ-like extension methods.
var hpAdjustment = constitution.Zip(level, (con, lev) => (float) Math.Round((con - 10f) / 3f) * lev);

var maxHealth = new ModifiableValue<float>(100f);
maxHealth.PropertyChanged += (_, _) => Console.WriteLine($"Max health is {maxHealth.Value}.");
maxHealth.Modifiers.Add(Modifier.Plus(hpAdjustment));

// Output: Max health is 100. (unchanged)
constitution.Value = 15;
// Output: Max health is 120.
level.Value = 15;
// Output: Max health is 130.
```

## ✍️ Creating Your Own Attribute Class

While the `IModifiableValue<T>` interface provides a solid foundation for managing modifiers, there may come a point where you want to organize modifiers in a more structured manner. Thankfully, there are several approaches you can take to achieve this.

### Step 1: Define a CharacterStat Class

To illustrate one such approach, let's create a `CharacterStat<T>` class that extends the `ModifiableValue<T>` class:

``` c#
public class CharacterStat<T> : ModifiableValue<T>
{
    public IModifiableValue<T> BaseFlatPlus { get; }
    public IModifiableValue<T> BasePlus { get; }
    public IModifiableValue<T> BaseTimes { get; }
    public IModifiableValue<T> TotalPlus { get; }
    public IModifiableValue<T> TotalTimes { get; }

    public CharacterStat(T initialValue) : base(initialValue)
    {
        BaseFlatPlus = new ModifiableValue<T>();
        BasePlus = new ModifiableValue<T>();
        BaseTimes = new ModifiableValue<T>(One());
        TotalPlus = new ModifiableValue<T>();
        TotalTimes = new ModifiableValue<T>(One());

        // Value = ((baseValue + BaseFlatPlus) * BaseTimes + BasePlus) * TotalTimes + TotalPlus.
        InitializeModifiers();
    }

    private void InitializeModifiers()
    {
        Modifiers.Add(100, Modifier.Plus(BaseFlatPlus));
        Modifiers.Add(200, Modifier.Times(BaseTimes));
        Modifiers.Add(300, Modifier.Plus(BasePlus));
        Modifiers.Add(400, Modifier.Times(TotalTimes));
        Modifiers.Add(500, Modifier.Plus(TotalPlus));
    }

    private static T One() => Modifier.GetOperator<T>().One;
}
```

The `CharacterStat<T>` class introduces additional `IModifiableValue<T>` properties that represent different categories of modifiers. It also initializes the modifiers in a specific order to achieve the desired calculation formula.

### Step 2: Using the CharacterStat Class

Once you have defined the `CharacterStat<T>` class, you can create instances and apply modifiers accordingly:

``` c#
    var MoveSpeed = new CharacterStat<float>(5f);
    MoveSpeed.BaseFlatPlus.modifiers.Add(Modifier.Plus(3f));
    MoveSpeed.BasePlus.modifiers.Add(Modifier.Times(2f));
    MoveSpeed.TotalTimes.modifiers.Add(Modifier.Times(1.5f, "+50%"));
```

In this example, we create a `MoveSpeed` attribute using the `CharacterStat<float>` class. We then add modifiers to the appropriate categories (`BaseFlatPlus`, `BaseValuePlus`, and `TotalValueTimes`) to customize the attribute's behavior.

By organizing modifiers into distinct properties, you gain greater control and clarity over how they affect the attribute's value.

## 💬 Note

If you have any suggestions or encounter any problems when using the app, please feel free to contact us by email. I am ready to help you and solve any of your problems.

## ❤️ Acknowledgments

This project drew inspiration and gained valuable insights from the following sources:

- [Character Stats (aka Attributes) System by Kryzarel](https://forum.unity.com/threads/tutorial-character-stats-aka-attributes-system.504095/). Kryzarel has also developed the Unity3D [Character Stats asset](https://assetstore.unity.com/packages/tools/integration/character-stats-106351) associated with it.
- [SeawispHunter.RolePlay.Attributes](https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes/)

I express my gratitude for their contribution and influence on the development of this project.

## 🔐 License

The source code of this project is licensed under the MIT license, which can be found [here](LICENSE).

---

> nikitayudin782@gmail.com &nbsp;&middot;&nbsp;
> GitHub [@nblackninja](https://github.com/с) &nbsp;&middot;&nbsp;
> Telegram [@yudinikita](https://t.me/yudinikita)
