using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using TaleWorlds.Library;

// Need to set up a common project to share between the scenes module and core
namespace SeparatistCrisis.Scenes.Tasks
{
    public sealed class SCParallelDriver: IParallelDriver
    {
        private DefaultParallelDriver _driver = new DefaultParallelDriver(); // Game uses NativeParallelDriver but we currently only need this for the SettlementPositionScript

        public void For(int fromInclusive, int toExclusive, TWParallel.ParallelForAuxPredicate body, int grainSize)
        {
            this._driver.For(fromInclusive, toExclusive, body, grainSize);
        }

        public void ForWithoutRenderThread(int fromInclusive, int toExclusive, TWParallel.ParallelForAuxPredicate body, int grainSize)
        {
            this._driver.For(fromInclusive, toExclusive, body, grainSize);
        }

        public void For(int fromInclusive, int toExclusive, float deltaTime, TWParallel.ParallelForWithDtAuxPredicate body, int grainSize)
        {
            this._driver.For(fromInclusive, toExclusive, deltaTime, body, grainSize);
        }

        public async Task ForRes(int fromInclusive, int toExclusive, TWParallel.ParallelForAuxPredicate body, int grainSize)
        {
            ParallelLoopResult res = Parallel.ForEach<Tuple<int, int>>(Partitioner.Create(fromInclusive, toExclusive, grainSize), Common.ParallelOptions, delegate (Tuple<int, int> range, ParallelLoopState loopState)
            {
                body(range.Item1, range.Item2);
            });
        }

        public async Task ForWhenAll(int fromInclusive, int toExclusive, TWParallel.ParallelForAuxPredicate body, int grainSize)
        {
            await Task.WhenAll(new Task[] { Task.Run(() => this.ForRes(fromInclusive, toExclusive, body, grainSize)) });
        }

        public ulong GetMainThreadId()
        {
            return this._driver.GetMainThreadId();
        }

        public ulong GetCurrentThreadId()
        {
            return this._driver.GetCurrentThreadId();
        }
    }
}
