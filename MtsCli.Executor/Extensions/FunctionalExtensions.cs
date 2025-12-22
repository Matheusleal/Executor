using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtsCli.Executor.Extensions;
public static class FunctionalExtensions
{
    public static TResult Pipe<TSource, TResult>(this TSource source, Func<TSource, TResult> func)
    {
        return func(source);
    }
}
