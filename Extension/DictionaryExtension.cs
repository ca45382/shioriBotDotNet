using System;
using System.Collections.Generic;

namespace PriconneBotConsoleApp.Extension
{
    public static class DictionaryExtension
    {
        public static TValue GetValueOrInitialize<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> initializer)
            => dictionary.TryGetValue(key, out var result)
                ? result
                : dictionary[key] = initializer();

        public static void Deconstruct<Tkey, TValue>(
            this KeyValuePair<Tkey, TValue> pair,
            out Tkey key,
            out TValue value)
            => (key, value) = (pair.Key, pair.Value);
    }
}
