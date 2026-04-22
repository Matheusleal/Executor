# MtsCli Executor

MtsCli Executor is a command-line interface (CLI) tool built with [Spectre.Console.Cli](https://spectreconsole.net/cli/). It is designed for extensibility, allowing developers to add new commands by creating new classes and registering them in the application configuration.

## How it Works

The tool uses Spectre.Console.Cli to handle command-line argument parsing, command discovery (via registration), and help generation.

- **Command Registration**: Commands are registered in `Program.cs` using the `CommandApp.Configure` method.
- **Strongly-typed Settings**: Each command has a corresponding `CommandSettings` class that defines its options and arguments with attributes.
- **Rich Output**: Integrated with [Spectre.Console](https://spectreconsole.net/) for beautiful terminal output, including tables, colors, and exceptions.

## How to Add a New Command

Adding a new command is straightforward. Follow these steps:

1.  **Create a new class** in the `MtsCli.Executor/Commands` directory. The name of the file should be descriptive of the command followed by `Command.cs` (e.g., `MyNewCommand.cs`).

2.  **Define a Settings class** inheriting from `CommandSettings` inside your command class. Use `[CommandOption]` and `[CommandArgument]` attributes.

3.  **Inherit from `Command<TSettings>`** (or `AsyncCommand<TSettings>`).

4.  **Implement the `Execute` method** (or `ExecuteAsync`).

5.  **Register the command** in `Program.cs`.

### Example: A Simple "Hello World" Command

**File**: `MtsCli.Executor/Commands/HelloWorldCommand.cs`

```csharp
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MtsCli.Executor.Commands;

public sealed class HelloWorldCommand : Command<HelloWorldCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("The name to greet.")]
        [CommandOption("-n|--name")]
        [DefaultValue("World")]
        public string Name { get; set; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine($"Hello, [green]{settings.Name}[/]!");
        return 0;
    }
}
```

**Registration in `Program.cs`**:

```csharp
app.Configure(config =>
{
    config.AddCommand<HelloWorldCommand>("hello")
        .WithDescription("Prints a greeting.");
});
```

## How to Build and Run

1.  **Build the project**:
    ```bash
    dotnet build
    ```

2.  **Run the application**:
    - To see the help menu and available commands:
      ```bash
      dotnet run --project MtsCli.Executor/MtsCli.Executor.csproj -- --help
      ```
    - To run a specific command:
      ```bash
      dotnet run --project MtsCli.Executor/MtsCli.Executor.csproj -- <command> [options]
      ```
      For example, to run the `remove-dir` command:
      ```bash
      dotnet run --project MtsCli.Executor/MtsCli.Executor.csproj -- remove-dir -p "C:\path\to\your\project"
      ```
