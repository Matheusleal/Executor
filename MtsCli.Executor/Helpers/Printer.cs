namespace MtsCli.Executor.Helpers;

public static class Printer
{
    public static void Print(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }

    public static void PrintInline(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = previousColor;
    }

    public static void PrintWithLabel(string label, string message, ConsoleColor labelColor = ConsoleColor.Gray, ConsoleColor messageColor = ConsoleColor.Green)
    {
        PrintInline(label, labelColor);
        Print(message, messageColor);
    }

    public static void BreakLine()
    {
        Console.WriteLine();
    }

    public static void PrintTable(IEnumerable<string[]> rows)
    {
        if (rows == null || !rows.Any())
        {
            Print("Nothing to show here.", ConsoleColor.Yellow);
            return;
        }

        // Set the max length by column
        int columnCount = rows.Max(r => r.Length);
        int[] columnWidths = new int[columnCount];

        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length; i++)
            {
                int length = row[i]?.Length ?? 0;
                if (length > columnWidths[i])
                    columnWidths[i] = length;
            }
        }

        string separator = "+" + string.Join("+", columnWidths.Select(w => new string('-', w + 2))) + "+";

        Print(separator, ConsoleColor.DarkGray);
        bool isHeader = true;

        foreach (var row in rows)
        {
            string line = "|";
            for (int i = 0; i < columnCount; i++)
            {
                string cell = i < row.Length ? row[i] : "";
                line += " " + cell.PadRight(columnWidths[i]) + " |";
            }

            Print(line, isHeader ? ConsoleColor.Cyan : ConsoleColor.Gray);

            if (isHeader)
            {
                Print(separator, ConsoleColor.DarkGray);
                isHeader = false;
            }
        }

        Print(separator, ConsoleColor.DarkGray);
    }
}
