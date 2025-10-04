using System.Reflection;

using MtsCli.Executor.CliLib;
using MtsCli.Executor.Helpers;


var arguments = Environment.GetCommandLineArgs();

await Run(arguments);

static async Task Run(string[] arguments)
{
    var start = DateTime.UtcNow;
    PrintStartupMessage(start);

    if (arguments.Length == 1)
    {
        Printer.Print("No arguments provided. Use 'list' command to see available commands.", ConsoleColor.Yellow);
        return;
    }

    try
    {
        var commands = Commander.LoadCommands();
        var parsedCommand = commands.Parse(arguments);

        Printer.PrintWithLabel("Running command: ", parsedCommand.Command.Name, ConsoleColor.Yellow, ConsoleColor.Green);

        // Special case for "list" command to show arguments without executing
        if (parsedCommand.Command.Flag == "list")
        {
            PrintListOfCommands(commands);
            return;
        }

        var result = await parsedCommand.Execute();

        if (result != null)
            if (result.ExitCode == 0)
                Printer.Print(result.Output, ConsoleColor.Green);
            else
                Printer.Print(result.Error, ConsoleColor.Red);

        PrintExitMessage(start);
    }
    catch (Exception ex)
    {
        HandleException(ex);
    }
}

static void PrintListOfCommands(List<CommandBase> commands)
{
    var rows = new List<string[]>
    {
        new[] { "Name", "Flag", "ShortFlag", "Description" },
    };
    rows.AddRange(
        commands.Select(cmd => new[]
        {
            cmd.Name,
            cmd.Flag,
            cmd.ShortFlag,
            cmd.Description
        }));

    Printer.PrintTable(rows);
}

static void PrintStartupMessage(DateTime startTime)
{
    var assembly = Assembly.GetExecutingAssembly();
    var version = assembly.GetName().Version?.ToString(3) ?? "no-version";
    var createdBy = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown";

    Console.Clear();
    Printer.Print("========================================", ConsoleColor.DarkGray);
    Printer.Print("            EXECUTOR CLI TOOL           ", ConsoleColor.Cyan);
    Printer.Print("========================================", ConsoleColor.DarkGray);
    Printer.PrintWithLabel("Version: ", version);
    Printer.PrintWithLabel("Created by: ", createdBy);
    Printer.BreakLine();
    Printer.PrintWithLabel("Starting at: ", $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
    Printer.Print("run 'executor list' to see available commands", ConsoleColor.Gray);
    Printer.Print("----------------------------------------", ConsoleColor.DarkGray);
}

static void PrintExitMessage(DateTime startTime)
{
    var endTime = DateTime.UtcNow;
    var duration = endTime - startTime;

    Printer.Print("----------------------------------------", ConsoleColor.DarkGray);
    Printer.Print($"Finished at: {endTime:yyyy/MM/dd HH:mm:ss}", ConsoleColor.Gray);
    Printer.Print($"Total duration: {duration.TotalSeconds} seconds", ConsoleColor.Gray);
    Printer.Print("========================================", ConsoleColor.DarkGray);
}

static void HandleException(Exception ex)
{
    Printer.Print("An error occurred:", ConsoleColor.Red);
    Printer.Print(ex.Message, ConsoleColor.Red);
    if (ex.InnerException != null)
    {
        Printer.Print("Inner Exception:", ConsoleColor.Red);
        Printer.Print(ex.InnerException.Message, ConsoleColor.Red);
    }
}