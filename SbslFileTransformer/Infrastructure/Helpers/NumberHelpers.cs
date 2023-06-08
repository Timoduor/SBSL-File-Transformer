using System;
using System.Threading;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class RandomNumberGen2
    {
        private static Random _global = new Random();
        private static ThreadLocal<Random> _local = new ThreadLocal<Random>(() =>
        {
            int seed;
            lock (_global) seed = _global.Next();
            return new Random(seed);
        });


        public static int Next()
        {
            return _local.Value.Next();
        }
    }
}
