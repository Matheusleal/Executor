# MtsCli Executor

MtsCli Executor is a command-line interface (CLI) tool designed for extensibility. It allows developers to add new commands by simply creating new classes. The tool automatically discovers and integrates new commands at runtime.

## How it Works

The core of the MtsCli Executor is the `Commander` class, which handles the discovery, parsing, and execution of commands.

- **Command Discovery**: On startup, the `Commander` scans the application's assemblies for any classes that inherit from `CommandBase`. This allows for a modular architecture where commands can be added or removed without changing the core application logic.
- **Command Parsing**: The `Commander` parses the command-line arguments to identify the requested command and its options.
- **Command Execution**: Once the command is identified, the `Commander` executes it, passing along any provided options. The tool supports both synchronous and asynchronous commands.

## How to Add a New Command

Adding a new command is straightforward. Follow these steps:

1.  **Create a new class** in the `MtsCli.Executor/Commands` directory. The name of the file should be descriptive of the command's function (e.g., `MyNewCommand.cs`).

2.  **Inherit from `CommandSync` or `CommandAsync`**.
    - Use `CommandSync` for synchronous operations.
    - Use `CommandAsync` for asynchronous operations.

3.  **Implement the abstract properties**:
    - `Name`: A user-friendly name for the command.
    - `Flag`: The long-form flag for the command (e.g., `my-new-command`).
    - `ShortFlag`: A short-form flag for the command (e.g., `mnc`).
    - `Description`: A brief description of what the command does.
    - `Options`: A list of `Option` objects that the command accepts.

4.  **Implement the `Execute` or `ExecuteAsync` method**:
    - This method contains the logic for your command.
    - It receives a `CommandInput` object, which contains the arguments passed to the command.

### Example: A Simple "Hello World" Command

Here is an example of a simple command that prints a greeting.

**File**: `MtsCli.Executor/Commands/HelloWorldCommand.cs`

```csharp
using MtsCli.Executor.CliLib;
using MtsCli.Executor.Helpers;

namespace MtsCli.Executor.Commands;

public class HelloWorldCommand : CommandSync
{
    public override string Name => "Hello World";
    public override string Flag => "hello";
    public override string ShortFlag => "h";
    public override string Description => "Prints a greeting.";
    public override List<Option> Options =>
    [
        new Option(
            Name: "name",
            ShortName: "n",
            Description: "The name to greet.",
            IsRequired: true)
    ];

    public override CommandOutput Execute(CommandInput input)
    {
        if (ShouldPrintHelpAndExit(input))
        {
            return new CommandOutput(0, "", "");
        }

        var name = input.Arguments.GetValueOrDefault("name", "World");
        Printer.Print($"Hello, {name}!", ConsoleColor.Green);

        return new CommandOutput(0, "Greeting sent successfully.", "");
    }
}
```

## How to Build and Run

1.  **Build the project**:
    ```bash
    dotnet build
    ```

2.  **Run the application**:
    - To see a list of available commands:
      ```bash
      dotnet run --project MtsCli.Executor/MtsCli.Executor.csproj -- list
      ```
    - To run a specific command:
      ```bash
      dotnet run --project MtsCli.Executor/MtsCli.Executor.csproj -- <command_flag> [options]
      ```
      For example, to run the `DirectoryRemover` command:
      ```bash
      dotnet run --project MtsCli.Executor/MtsCli.Executor.csproj -- remove-dir -p "C:\path\to\your\project"
      ```
