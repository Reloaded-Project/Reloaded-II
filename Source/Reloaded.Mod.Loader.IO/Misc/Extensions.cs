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
    }
}
