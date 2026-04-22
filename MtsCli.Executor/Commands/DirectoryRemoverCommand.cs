using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MtsCli.Executor.Commands;

public sealed class DirectoryRemoverCommand : Command<DirectoryRemoverCommand.Settings>
{
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
                AnsiConsole.MarkupLine($"[red]Error:[/] The specified path does not exist: [yellow]{path}[/]");
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
                    AnsiConsole.MarkupLine($"[grey]Deleting: {dir}[/]");
                }

                try
                {
                    Directory.Delete(dir, true);
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to delete:[/] [yellow]{dir}[/] - [red]{ex.Message}[/]");
                    failedCount++;
                }

            }
            AnsiConsole.MarkupLine($"[green]Deleted {deletedCount} directories.[/]");
            
            if (deletedCount > 0)
                AnsiConsole.MarkupLine($"[red]Failed to delete {failedCount} directories.[/]");

            AnsiConsole.MarkupLine("[green]Cleanup completed successfully.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}
