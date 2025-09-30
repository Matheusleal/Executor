namespace MtsCli.Executor.CliLib;

public static partial class Commander
{
    public static List<CommandBase> LoadCommands()
    {
        try
        {
            return [.. GetImplementations<CommandBase>()
                .Select(x => Activator.CreateInstance(x) as CommandBase)];
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public static CommandInput Parse(this List<CommandBase> commands, string[] input)
    {
        var args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var parts = input.Skip(1).ToList();

        if (parts.Count == 0)
            throw new ArgumentException("Input cannot be empty.");

        var commandPart = parts[0];

        var command = commands
            .FirstOrDefault(c =>
                c.Flag.Equals(commandPart, StringComparison.OrdinalIgnoreCase) ||
                c.ShortFlag.Equals(commandPart, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Command '{commandPart}' not found.");
        
        var tempOptions = new List<Option>([..command.Options, new Option("help", "h", "Show help information", false)]);

        for (int i = 1; i < parts.Count; i++)
        {
            var part = parts[i];
            if (part.StartsWith("-"))
            {
                var optionName = part.TrimStart('-');
                var option = tempOptions
                    .FirstOrDefault(o =>
                        o.Name.Equals(optionName, StringComparison.OrdinalIgnoreCase) ||
                        o.ShortName.Equals(optionName, StringComparison.OrdinalIgnoreCase))
                        ?? throw new ArgumentException($"Unknown option '{optionName}' for command '{command.Name}'.");

                string optionValue = "true"; // Default value for flags
                if (i + 1 < parts.Count && !parts[i + 1].StartsWith("-"))
                {
                    optionValue = parts[i + 1];
                    i++;
                }
                args[option.Name] = optionValue;
            }
            else
            {
                throw new ArgumentException($"Unexpected argument '{part}'.");
            }
        }

        if (!tempOptions.Any(o => o.Name.Equals("help", StringComparison.OrdinalIgnoreCase)) && args.ContainsKey("help"))
        {
            // Check for required options
            foreach (var opt in tempOptions.Where(o => o.IsRequired))
            {
                if (!args.ContainsKey(opt.Name))
                    throw new ArgumentException($"Missing required option '{opt.Name}' for command '{command.Name}'.");
            }
        }

        return new CommandInput(input, command, args);
    }

    public static async Task<CommandOutput> Execute(this CommandInput input)
    {
        if (input.Command is CommandSync syncCmd)
        {
            await Task.CompletedTask;

            return syncCmd.Execute(input);
        }

        return await ((CommandAsync)input.Command).ExecuteAsync(input);
    }

    private static List<Type?> GetImplementations<TClassType>()
    {
        var classType = typeof(TClassType);

        try
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t is { IsClass: true, IsAbstract: false }
                    && classType.IsAssignableFrom(t)
                )
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading implementations of {classType.Name}: {ex.Message}");
            throw;
        }
    }
}
