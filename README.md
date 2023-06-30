dll2mmd
==================================================

## What is dll2mmd

Dll2mmd is a dotnet tool for generating mermaid.js class-diagram from assemblies.

## Installing dll2mmd

1. Install .Net SDK 6.0 or later.
2. Install dll2mmd as a global dotnet tool.

    ```shell
    $ dotnet tool install --global dll2mmd
    You can invoke the tool using the following command: dll2mmd
    Tool 'dll2mmd' (version '1.0.5') was successfully installed.
    ```

## Usage

```shell
Description:
  Generate mermaid.js class-diagram from .net dll files.

Usage:
  dll2mmd [options]

Options:
  -o, --output <output>           Output file. [default: output.md]
  -ns, --namespace <namespace>    Namespace from which to fetch classes. []
  -f, --files <files> (REQUIRED)  Dll files from which to fetch classes.
  -t, --type-names <type-names>   Name of specific classes to form the diagram. []
  --ignore-dependency             If true, only dependency of inheritance and implementation would be generated.
  --version                       Show version information
  -?, -h, --help                  Show help and usage information
```

## Example

1. Generate Zoo.dll by compiling following file.

    ```csharp
    namespace Zoo;
    public interface IAnimal
    {
        string Name { get; set; }
        IFood FavorateFood { get; set; }
        void Speak();
    }

    public abstract class Mammal : IAnimal
    {
        public string Name { get; set; }
        public IFood FavorateFood { get; set; }

        public abstract void Speak();

        protected Mammal(string name, IFood favorateFood)
        {
            Name = name;
            FavorateFood = favorateFood;
        }
    }

    public class Cat : Mammal
    {
        public Cat(string name, IFood favorateFood) : base(name, favorateFood) {}
        public override void Speak()
        {
            Console.WriteLine("meow");
        }
    }

    public class Dog : Mammal
    {
        public Dog(string name, IFood favorateFood) : base(name, favorateFood) { }
        public override void Speak()
        {
            Console.WriteLine("woof");
        }
    }

    public interface IFood
    {
        string Taste { get; set; }
    }

    public class Fish : IFood, IAnimal
    {
        public string Taste { get; set; }
        public string Name { get; set; }
        public IFood FavorateFood { get; set; }

        public Fish(string taste, string name, IFood favorateFood)
        {
            Taste = taste;
            Name = name;
            FavorateFood = favorateFood;
        }
        public void Speak()
        {
            Console.WriteLine("...");
        }
    }

    public class Bone : IFood
    {
        public string Taste { get; set; }
        public Bone(string taste)
        {
            Taste = taste;
        }
    }

    ```

2. Run dll2mmd to generate output.md from Zoo.dll.

    ```shell
    $ dll2mmd -f Zoo.dll
    ```

3. output.md

    ```mermaid
    classDiagram

    class IAnimal
    IAnimal : +String Name
    IAnimal : +IFood FavorateFood
    IAnimal : +Speak() Void

    class Mammal
    Mammal : +String Name
    Mammal : +IFood FavorateFood
    Mammal : +Speak() Void

    class Cat
    Cat : +String Name
    Cat : +IFood FavorateFood
    Cat : +Speak() Void

    class Dog
    Dog : +String Name
    Dog : +IFood FavorateFood
    Dog : +Speak() Void

    class IFood
    IFood : +String Taste

    class Fish
    Fish : +String Taste
    Fish : +String Name
    Fish : +IFood FavorateFood
    Fish : +Speak() Void

    class Bone
    Bone : +String Taste


    IAnimal <|.. Mammal
    Mammal <|-- Cat
    Mammal <|-- Dog
    IFood <|.. Fish
    IAnimal <|.. Fish
    IFood <|.. Bone

    ```
