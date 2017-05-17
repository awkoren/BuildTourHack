using System;
using System.Linq;

namespace Microsoft.Knowzy.Repositories.Core
{
    public static class OrderRepositoryHelper
    {
        public static string GenerateString(int size)
        {
            var random = new Random();
            var alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var chars = Enumerable.Range(0, size)
                .Select(x => alphabet[random.Next(0, alphabet.Length)]);
            return new string(chars.ToArray());
        }
    }
}
