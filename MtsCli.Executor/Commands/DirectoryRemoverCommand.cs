using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MtsCli.Executor.Commands;

public class DirectoryRemoverCommand : Command<DirectoryRemoverCommand.Settings>
{
    private readonly IAnsiConsole _console;

    public DirectoryRemoverCommand(IAnsiConsole console)
    {
        _console = console ?? AnsiConsole.Console;
    }

    public DirectoryRemoverCommand() : this(null)
    {
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Path to the root directory of the .NET project")]
        [CommandOption("-p|--path")]
        [DefaultValue(".")]
        public string Path { get; set; }

        [Description("Semicolon-separated list of directories to delete (default: bin;obj)")]
        [CommandOption("-d|--directories")]
        [DefaultValue("bin;obj")]
        public string Directories { get; set; }

        [Description("Print the folder path to be deleted")]
        [CommandOption("-v|--verbose")]
        [DefaultValue(false)]
        public bool Verbose { get; set; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var path = Path.GetFullPath(settings.Path);
            var directoryList = settings.Directories.Split(';', StringSplitOptions.RemoveEmptyEntries);

            if (!Directory.Exists(path))
            {
                _console.MarkupLine($"[red]Error:[/] The specified path does not exist: [yellow]{path}[/]");
                return 1;
            }

            var directoriesToDelete = directoryList
                .Select(x => Directory.GetDirectories(path, x, SearchOption.AllDirectories))
                .SelectMany(x => x)
                .OrderByDescending(x => x);

            int deletedCount = 0;
            int failedCount = 0;
            foreach (var dir in directoriesToDelete)
            {
                if (settings.Verbose)
                {
                    _console.MarkupLine($"[grey]Deleting: {dir}[/]");
                }

                try
                {
                    Directory.Delete(dir, true);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    _console.MarkupLine($"[red]Failed to delete:[/] [yellow]{dir}[/] - [red]{ex.Message}[/]");
                    failedCount++;
                }

            }
            _console.MarkupLine($"[green]Deleted {deletedCount} directories.[/]");
            
            if (deletedCount > 0)
                _console.MarkupLine($"[red]Failed to delete {failedCount} directories.[/]");

            _console.MarkupLine("[green]Cleanup completed successfully.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            return 1;
        }
    }
}
