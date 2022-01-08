using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ShioriBot.Net.Extension
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

        public static TSource MaxBy<TSource, TValue>([NotNull] this IEnumerable<TSource> source, [NotNull] Func<TSource, TValue> func)
            where TValue : IComparable<TValue>
        {
            var firstFlag = true;
            TValue maxValue = default;
            TSource maxSource = default;

            foreach (var element in source)
            {
                var value = func(element);

                if (firstFlag || value.CompareTo(maxValue) > 0)
                {
                    maxValue = value;
                    maxSource = element;
                    firstFlag = false;
                }
            }

            return maxSource;
        }
    }
}
