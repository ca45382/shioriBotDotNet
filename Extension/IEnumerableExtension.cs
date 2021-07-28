using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PriconneBotConsoleApp.Extension
{
    public static class EnumerableExtension
    {
        public static void ForEach<T>([NotNull] this IEnumerable<T> source, [NotNull] Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }
    }
}
