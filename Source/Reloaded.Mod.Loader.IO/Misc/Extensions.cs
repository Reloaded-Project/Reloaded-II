using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reloaded.Mod.Loader.IO.Misc
{
    public static class Extensions
    {
        /// <summary>
        /// Checks if two sequences are equal using <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/> with support
        /// for null parameters.
        /// </summary>
        public static bool SequenceEqualWithNullSupport<T>(this IList<T> first, IList<T> second)
        {
            if (first == null || second == null)
                return (first == null && second == null);

            if (first.Count != second.Count)
                return false;

            return first.SequenceEqual(second);
        }

        /// <summary>
        /// Returns a hashcode for an item of type T. Returns 0 if the type is null.
        /// </summary>
        public static int GetHashCodeWithNullSupport<T>(this IEnumerable<T> items)
        {
            return items?.Aggregate(0, (current, item) => (current * 397) ^ item.GetHashCode()) ?? 0;
        }
    }
}
