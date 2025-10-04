using MtsCli.Executor.Helpers;

namespace MtsCli.Executor.Extensions;
public static class PrintExtensions
{
    public static T Print<T>(this T obj, Func<T, string> template,  ConsoleColor color = ConsoleColor.Gray)
    {
        var message = template(obj);

        Printer.Print(message, color);
        
        return obj;
    }

    public static T PrintInline<T>(this T obj, Func<T, string> template,  ConsoleColor color = ConsoleColor.Gray)
    {
        var message = template(obj);

        Printer.PrintInline(message, color);
        
        return obj;
    }

    public static T PrintWithLabel<T>(this T obj, 
        Func<T, string> template, 
        string label, 
        ConsoleColor templateColor = ConsoleColor.Gray, 
        ConsoleColor labelColor = ConsoleColor.Gray)
    {
        var message = template(obj);

        Printer.PrintWithLabel(label, message, labelColor, templateColor);
        
        return obj;
    }

    public static T PrintTable<T>(this T obj, Func<T, IEnumerable<string[]>> template)
    {
        var message = template(obj);

        Printer.PrintTable(message);
        
        return obj;
    }
}
