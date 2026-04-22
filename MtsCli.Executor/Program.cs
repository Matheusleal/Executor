using MtsCli.Executor.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection;

var start = DateTime.UtcNow;
PrintStartupMessage();

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("executor");
    
    config.AddCommand<DirectoryRemoverCommand>("remove-dir")
        .WithAlias("rm")
        .WithDescription("A tool to delete folders, by default \"bin\" and \"obj\"");
});

var result = app.Run(args);

PrintExitMessage(start);

return result;

static void PrintStartupMessage()
{
    var assembly = Assembly.GetExecutingAssembly();
    var version = assembly.GetName().Version?.ToString(3) ?? "1.0.0";
    var createdBy = "Matheus Leal";

    AnsiConsole.Clear();
    AnsiConsole.MarkupLine("[grey]========================================[/]");
    AnsiConsole.MarkupLine("[cyan]            EXECUTOR CLI TOOL           [/]");
    AnsiConsole.MarkupLine("[grey]========================================[/]");
    AnsiConsole.MarkupLine($"[white]Version:[/] {version}");
    AnsiConsole.MarkupLine($"[white]Created by:[/] {createdBy}");
    AnsiConsole.MarkupLine("");
    AnsiConsole.MarkupLine($"[white]Starting at:[/] {DateTime.Now:yyyy/MM/dd HH:mm:ss}");
    AnsiConsole.MarkupLine("[grey]run 'executor --help' to see available commands[/]");
    AnsiConsole.MarkupLine("[grey]----------------------------------------[/]");
}

static void PrintExitMessage(DateTime startTime)
{
    var endTime = DateTime.UtcNow;
    var duration = endTime - startTime;

    AnsiConsole.MarkupLine("[grey]----------------------------------------[/]");
    AnsiConsole.MarkupLine($"[grey]Finished at: {DateTime.Now:yyyy/MM/dd HH:mm:ss}[/]");
    AnsiConsole.MarkupLine($"[grey]Total duration: {duration.TotalSeconds:F2} seconds[/]");
    AnsiConsole.MarkupLine("[grey]========================================[/]");
}
