using MtsCli.Executor.CliLib;
using MtsCli.Executor.Extensions;
using MtsCli.Executor.Helpers;

namespace MtsCli.Executor.Commands;
public class DirectoryRemover : CommandSync
{
    public override string Name => "Directory remover";
    public override string Flag => "remove-dir";
    public override string ShortFlag => "rm";
    public override string Description => "A tool to delete folders, by default \"bin\" and \"obj\"";
    public override List<Option> Options => [
        new Option(
            Name: "path",
            ShortName: "p",
            Description: "Path to the root directory of the .NET project (default: current directory)",
            IsRequired: true),
        new Option(
            Name: "directories",
            ShortName: "d",
            Description: "Semicolon-separated list of directories to delete (default: bin;obj;node_modules)",
            IsRequired: false),
        new Option(
            Name: "verbose",
            ShortName: "v",
            Description: "Print the folder path to be deleted (default: false)",
            IsRequired: false)
        ];

    public override CommandOutput Execute(CommandInput input)
    {
        if (ShouldPrintHelpAndExit(input))
            return new CommandOutput(0, "", "");

        try
        {
            var args = input.Arguments;

            var path = GetPath(args, "path", Directory.GetCurrentDirectory());
            var directories = GetDirectories(args, "directories", ["bin", "obj", "node_modules"]);
            var verbose = GetVerbose(args, "verbose", false);

            if (!Directory.Exists(path))
                return new CommandOutput(1, "", $"The specified path does not exist: {path}");

            var results = directories
                .Select(x => Directory.GetDirectories(path, x, SearchOption.AllDirectories))
                .SelectMany(x => x)
                .OrderByDescending(x => x)
                .ToList()
                .Print(dirs => $"Found {dirs.Count} directories to delete.", ConsoleColor.Green)
                .Select(dir => TryDeleteDirectory(dir, verbose))
                .Pipe(results =>
                    {
                        var deleted = results.Count(x => x.result);
                        var failed = results.Count(x => !x.result);

                        Printer.Print("\nCleanup Summary: ", ConsoleColor.Yellow);
                        Printer.PrintInline("Deleted ", ConsoleColor.White);
                        Printer.PrintInline($"{deleted}", ConsoleColor.Green);
                        Printer.PrintInline(" directories successfully. Failed to delete ", ConsoleColor.White);
                        Printer.PrintInline($"{failed}", ConsoleColor.Red);
                        Printer.Print(" directories.\n", ConsoleColor.White);

                        return results;
                    });

            if (verbose)
                ShowErrorOutput(results);

            return new CommandOutput(0, "Cleanup completed successfully.", "");
        }
        catch (Exception ex)
        {
            return new CommandOutput(1, "", $"An error occurred: {ex.Message}");
        }
    }

    private static (bool result, string path, string message) TryDeleteDirectory(string dir, bool verbose)
    {
        try
        {
            if (verbose)
                Printer.Print($"Deleting: {dir}", ConsoleColor.Gray);

            Directory.Delete(dir, true);

            return (true, string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, dir, ex.Message);
        }
    }

    private static void ShowErrorOutput(IEnumerable<(bool result, string path, string message)> results)
    {
        Printer.Print("Failed to delete the following directories:", ConsoleColor.Red);

        var errors = results.Where(x => !x.result);

        foreach (var error in errors)
        {
            (var result, string path, string message) = error;

            Printer.Print($"- {path}: {message}", ConsoleColor.Red);
        }
    }

    private static string GetPath(Dictionary<string, string> args, string key, string defaultValue) =>
        args.TryGetValue(key, out string? value) ? value : defaultValue;

    private static List<string> GetDirectories(Dictionary<string, string> args, string key, List<string> defaultValue) =>
        args.TryGetValue(key, out string? value) ? [.. value.Split(';', StringSplitOptions.RemoveEmptyEntries)] : defaultValue;

    private static bool GetVerbose(Dictionary<string, string> args, string key, bool defaultValue) =>
        args.TryGetValue(key, out string? value) && bool.TryParse(value, out var result) ? result : defaultValue;
}
