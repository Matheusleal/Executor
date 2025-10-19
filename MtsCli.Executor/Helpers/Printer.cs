namespace MtsCli.Executor.Helpers;

public static class Printer
{
    public static ConsoleColor DefaultColor => Console.ForegroundColor;

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

        int windowWidth = Console.WindowWidth - 1;
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

        int totalWidth = columnWidths.Sum() + (3 * columnCount) + 1;

        if (totalWidth > windowWidth)
        {
            double ratio = (double)(windowWidth - (3 * columnCount) - 1) / columnWidths.Sum();
            for (int i = 0; i < columnWidths.Length; i++)
                columnWidths[i] = Math.Max(5, (int)(columnWidths[i] * ratio)); // mínimo 5 chars
        }

        string separator = "+" + string.Join("+", columnWidths.Select(w => new string('-', w + 2))) + "+";

        Print(separator, ConsoleColor.DarkGray);
        bool isHeader = true;

        foreach (var row in rows)
        {
            List<string[]> wrappedCells = new();
            int maxLines = 1;

            for (int i = 0; i < columnCount; i++)
            {
                string cell = i < row.Length ? row[i] ?? "" : "";
                var wrapped = SplitToLines(cell, columnWidths[i]).ToArray();
                wrappedCells.Add(wrapped);
                if (wrapped.Length > maxLines)
                    maxLines = wrapped.Length;
            }

            for (int lineIndex = 0; lineIndex < maxLines; lineIndex++)
            {
                string line = "|";
                for (int i = 0; i < columnCount; i++)
                {
                    string cellLine = lineIndex < wrappedCells[i].Length ? wrappedCells[i][lineIndex] : "";
                    line += " " + cellLine.PadRight(columnWidths[i]) + " |";
                }

                Print(line, isHeader ? ConsoleColor.Cyan : ConsoleColor.Gray);
            }

            Print(separator, ConsoleColor.DarkGray);
            if (isHeader)
                isHeader = false;
        }
    }

    private static IEnumerable<string> SplitToLines(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            yield return "";
        else
        {
            for (int i = 0; i < text.Length; i += maxWidth)
                yield return text.Substring(i, Math.Min(maxWidth, text.Length - i));
        }
    }
}
