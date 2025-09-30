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
}
