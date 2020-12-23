using System;
using System.Linq;

namespace Reloaded.Mod.Loader.Tests.SETUP.Utilities
{
    public static class RandomString
    {
        private static Random _random = new Random();

        public static string AlphaNumeric(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
