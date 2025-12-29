# Procedural Graph Core

**Procedural Graph Core** is [Flax Engine](https://flaxengine.com/) Editor Plugin designed to facilitate the creation and management of procedural generation graphs. It provides a framework for converting scene `Actors` into processing `Nodes`, which can then execute asynchronous generation tasks.

## Overview

This plugin establishes a core architecture for procedural generation within the Flax Editor. It decouples the representation of the graph (`Node`) from the actual generation logic (`IGenerator`) and the source data (`Actor`), allowing for flexible, asynchronous build processes.

### Key Features

- **Node Conversion System:** A robust interface (`INodeConverter`) to dynamically convert Flax `Actors` into graph `Nodes`.

- **Asynchronous Generation:** Nodes produce `IGenerator` instances capable of running `BuildAsync` tasks, ensuring the editor remains responsive during heavy procedural generation.

- **Actor Binding:** The `ActorNode` class handles binding to scene actors, tracking their IDs, and detecting updates automatically.

- **Hierarchy Management:** Built-in support for parent/child node relationships and propagation flags (`PropagateUpwards`, `PropagateDownwards`).

## Installation

1. Copy the `Procedural-Graph-Core` folder into the `Plugins` directory of your Flax project.
2. Regenerate your project script files (right-click `.flaxproj` -> **Generate scripts**).
3. Open the editor and ensure the plugin is enabled in **Tools -> Plugins**.

## Architecture & Usage

The core workflow revolves around the **Builder**, **Converters**, **Nodes**, and **Generators**.

### 1. The Procedural Graph Builder

The `ProceduralGraphBuilder` is an `EditorPlugin` that manages the registry of node converters. You must register your custom converters here to allow the system to recognize and process different Actor types.

```csharp
// Example: Accessing the plugin to register a converter
var graphBuilder = FlaxEngine.PluginManager.GetPlugin<ProceduralGraph.ProceduralGraphBuilder>();
graphBuilder.AddConverter<MyCustomNodeConverter>();

```

### 2. Defining Nodes

Create classes that inherit from `Node` or `ActorNode`.

- **`Node`**: The base class for all graph elements. It holds references to parents and children and defines the `CreateGenerator()` method.

- **`ActorNode`**: A specialized node that represents a Flax `Actor`. It automatically handles serialization of the Actor ID and monitors the Actor for changes.

```csharp
public class MyCustomNode : ActorNode
{
    public override IGenerator CreateGenerator()
    {
        return new MyCustomGenerator(this);
    }
}

```

### 3. Converters (`INodeConverter`)

To bridge the gap between a Flax Actor in your scene and the procedural graph, implement the `INodeConverter` interface.

```csharp
public class MyCustomNodeConverter : INodeConverter
{
    public bool TryConvert(Actor actor, out ActorNode node)
    {
        if (actor is MyScriptType)
        {
            node = new MyCustomNode();
            node.Actor = actor;
            return true;
        }
        node = null;
        return false;
    }
}

```

### 4. Generators (`IGenerator`)

The `IGenerator` interface handles the actual "work" of the node. This separation ensures that logic data (`Node`) is kept separate from execution logic (`Generator`).

```csharp
public class MyCustomGenerator : IGenerator
{
    public async Task BuildAsync(CancellationToken cancellationToken)
    {
        // Perform procedural generation logic here
    }
}

```

## License

This project is licensed under the **PolyForm Shield License 1.0.0**.

> **Summary:** You are free to use, modify, and distribute this software, provided you do not use it to build a product that competes with the software itself.

Please see the [LICENSE.md](LICENSE.md) file for the full legal text.
