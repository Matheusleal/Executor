using MtsCli.Executor.Helpers;

namespace MtsCli.Executor.CliLib;

public record Option(string Name, string ShortName, string Description, bool IsRequired);
public record CommandInput(string[] RawInput, CommandBase Command, Dictionary<string, string> Arguments);
public record CommandOutput(int ExitCode, string Output, string Error);

public abstract class CommandBase
{
    public abstract string Name { get; }
    public abstract string Flag { get; }
    public abstract string ShortFlag { get; }
    public abstract string Description { get; }
    public abstract List<Option> Options { get; }

    protected bool ShouldPrintHelpAndExit(CommandInput input)
    {
        var isHelpRequested = input.Arguments.ContainsKey("help") || input.Arguments.ContainsKey("h");
        if (isHelpRequested)
        {
            PrintHelp();
            return true;
        }
        return false;
    }

    private void PrintHelp()
    {
        Printer.Print($"Command name: {Name}", ConsoleColor.Yellow);
        Printer.Print($"Flag: --{Flag}, -{ShortFlag}", ConsoleColor.Yellow);
        Printer.Print($"Description: {Description}\n", ConsoleColor.Yellow);

        var options = new List<string[]> {
            new[] { "Flag", "Short", "Description", "Required" }
        };

        if (Options.Count > 0)
        {
            Options.ForEach(opt => options.Add([
                $"--{opt.Name}",
                $"-{opt.ShortName}",
                opt.Description,
                opt.IsRequired ? "Yes" : "No"
            ]));

            Printer.Print("Options:", ConsoleColor.Yellow);
            Printer.PrintTable(options);
        }
        else
        {
            Printer.PrintInline("No options available for this command.", ConsoleColor.Yellow);
        }
        Console.WriteLine();
    }
}

public abstract class CommandSync : CommandBase
{
    public abstract CommandOutput Execute(CommandInput input);
}

public abstract class CommandAsync : CommandBase
{
    public abstract Task<CommandOutput> ExecuteAsync(CommandInput input);
}
