using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SocialNet
{
    public static class Generate
    {
        private static Random Rand = new Random();

        public static bool boolean => Rand.Next(2) == 0;

        public static int Int() => Rand.Next();
        public static int Int(int max) => Rand.Next(max);
        public static int Int(int min, int max) => Rand.Next(min, max);

        public static long Long()
        {
            var b = new byte[8];
            Rand.NextBytes(b);
            return Math.Abs(BitConverter.ToInt64(b, 0));
        }
        public static long Long(long max) => Long() % max;
        public static long Long(long min, long max) => Long(max - min) + min;

        public static DateTime NewDateTime(DateTime min, DateTime max) => new DateTime(Long(min.Ticks, max.Ticks));
    }
}