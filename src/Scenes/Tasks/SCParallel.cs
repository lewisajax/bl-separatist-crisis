using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace SeparatistCrisis.Scenes.Tasks
{
    // Wrapper for TWParallel
    public static class SCParallel
    {
        private static SCParallelDriver _driver;

        public static void InitializeAndSetImplementation()
        {
            SCParallelDriver driver = new SCParallelDriver(); // TWParallel will still use NativeParallelDriver
            SCParallel._driver = driver;
        }

        public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
        {
            return TWParallel.ForEach<TSource>(source, body);
        }

        public static void For(int fromInclusive, int toExclusive, TWParallel.ParallelForAuxPredicate body, int grainSize = 16)
        {
            TWParallel.For(fromInclusive, toExclusive, body, grainSize);
        }

        public static void ForWithoutRenderThread(int fromInclusive, int toExclusive, TWParallel.ParallelForAuxPredicate body, int grainSize = 16)
        {
            TWParallel.ForWithoutRenderThread(fromInclusive, toExclusive, body, grainSize);
        }

        public static void For(int fromInclusive, int toExclusive, float deltaTime, TWParallel.ParallelForWithDtAuxPredicate body, int grainSize = 16)
        {
            TWParallel.For(fromInclusive, toExclusive, deltaTime, body, grainSize);
        }

        public async static Task ForWhenAll(int fromInclusive, int toExclusive, TWParallel.ParallelForAuxPredicate body, int grainSize = 16)
        {
            await SCParallel._driver.ForWhenAll(fromInclusive, toExclusive, body, grainSize);
        }
    }
}
