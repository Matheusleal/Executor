using MtsCli.Executor.CliLib;
using MtsCli.Executor.Helpers;

namespace MtsCli.Executor.Commands;
public class ListAvailableModules : CommandSync
{
    public override string Name => "List modules";
    public override string Flag => "list";
    public override string ShortFlag => "l";
    public override string Description => "List all available modules";
    public override List<Option> Options => [];

    public override CommandOutput Execute(CommandInput input)
    {
        if (ShouldPrintHelpAndExit(input))
            return new CommandOutput(0, "", "");

        Printer.Print("Available modules:", ConsoleColor.Cyan);

        input
            .Arguments
            .ToList()
            .ForEach(arg =>
                Printer.Print(arg.Key, ConsoleColor.DarkBlue));

        return new CommandOutput(0, "", "");
    }
}
