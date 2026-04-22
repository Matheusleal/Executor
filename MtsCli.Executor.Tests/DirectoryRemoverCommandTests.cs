using MtsCli.Executor.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace MtsCli.Executor.Tests;

public class DirectoryRemoverCommandTests
{
    private class TestDirectoryRemoverCommand : DirectoryRemoverCommand
    {
        public TestDirectoryRemoverCommand(IAnsiConsole console) : base(console) { }
        
        public int PublicExecute(CommandContext context, Settings settings)
        {
            return Execute(context, settings, CancellationToken.None);
        }
    }

    [Fact]
    public void Should_Delete_Bin_And_Obj_Directories()
    {
        // Given
        var tempPath = Path.Combine(Path.GetTempPath(), "MtsCliTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(tempPath, "bin"));
        Directory.CreateDirectory(Path.Combine(tempPath, "obj"));
        Directory.CreateDirectory(Path.Combine(tempPath, "src"));

        var console = new TestConsole();
        var command = new TestDirectoryRemoverCommand(console);
        var settings = new DirectoryRemoverCommand.Settings
        {
            Path = tempPath,
            Directories = "bin;obj",
            Verbose = true
        };

        try
        {
            // When
            var result = command.PublicExecute(null, settings);

            // Then
            Assert.Equal(0, result);
            Assert.False(Directory.Exists(Path.Combine(tempPath, "bin")));
            Assert.False(Directory.Exists(Path.Combine(tempPath, "obj")));
            Assert.True(Directory.Exists(Path.Combine(tempPath, "src")));
            Assert.Contains("Cleanup completed successfully.", console.Output);
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void Should_Return_Error_If_Path_Does_Not_Exist()
    {
        // Given
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var console = new TestConsole();
        var command = new TestDirectoryRemoverCommand(console);
        var settings = new DirectoryRemoverCommand.Settings
        {
            Path = nonExistentPath,
            Directories = "bin;obj"
        };

        // When
        var result = command.PublicExecute(null, settings);

        // Then
        Assert.Equal(1, result);
        Assert.Contains("Error:", console.Output);
    }
}
