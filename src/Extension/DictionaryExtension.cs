using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PriconneBotConsoleApp.Extension
{
    public static class DictionaryExtension
    {
        public static TValue GetValueOrInitialize<TKey, TValue>(
            [NotNull] this IDictionary<TKey, TValue> dictionary,
            [NotNull] TKey key,
            [NotNull] Func<TValue> initializer)
            => dictionary.TryGetValue(key, out var result)
                ? result
                : dictionary[key] = initializer();

        /// <summary>
        /// <see cref="IDictionary{TKey,TValue}.TryGetValue"/>を複数のキーで検索できるようにしたバージョン
        ///
        /// paramsに対応するため、out引数を先に書く必要がある点に注意
        /// </summary>
        /// <param name="dictionary">検索対象の辞書</param>
        /// <param name="value">検索結果 (無い場合はdefaultになる)</param>
        /// <param name="keys">キーの候補 (前方から順に検索される)</param>
        /// <typeparam name="TKey">辞書のキーの型</typeparam>
        /// <typeparam name="TValue">辞書の値の型</typeparam>
        /// <returns></returns>
        public static bool TryGetValueMany<TKey, TValue>(
            [NotNull] this IDictionary<TKey, TValue> dictionary,
            [NotNull] out TValue value,
            [NotNull] params TKey[] keys)
        {
            foreach (var key in keys)
            {
                if (dictionary.TryGetValue(key, out value))
                {
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static void Deconstruct<TKey, TValue>(
            [NotNull] this KeyValuePair<TKey, TValue> pair,
            [NotNull] out TKey key,
            [NotNull] out TValue value)
            => (key, value) = (pair.Key, pair.Value);
    }
}
