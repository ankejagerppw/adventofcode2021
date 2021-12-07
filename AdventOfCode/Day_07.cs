using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_07 : BaseDay
    {
        private readonly ImmutableSortedDictionary<int, int> _horizontalPosCounts;

        public Day_07()
        {
            int[] input = File
                .ReadAllLines(InputFilePath)
                .SelectMany(l => l.Split(","))
                .Select(int.Parse)
                .ToArray();
            _horizontalPosCounts =
                input
                    .ToImmutableSortedDictionary(pos => pos, pos => input.Count(i => i == pos));
        }

        public override ValueTask<string> Solve_1()
        {
            long FuelCost(long pos) => _horizontalPosCounts
                    .Sum(hp => Math.Abs(hp.Key - pos) * hp.Value);

            long? bestCost = null;

            int lastPos = _horizontalPosCounts.Keys.Last();
            for (long pos = _horizontalPosCounts.Keys.First(); pos <= lastPos; pos++)
            {
                long cost = FuelCost(pos);
                if (bestCost == null || bestCost > cost)
                {
                    bestCost = cost;
                }
            }

            return new ValueTask<string>($"{bestCost ?? 0}");
        }

        public override ValueTask<string> Solve_2()
        {
            Dictionary<int, long> cachedCosts = new Dictionary<int, long>();

            long FuelCost(int pos) => _horizontalPosCounts
                .Sum(hp => IncreaseCost(Math.Abs(hp.Key - pos)) * hp.Value);

            // long IncreaseCost(int x) => Enumerable.Range(1, x).Sum();

            long IncreaseCost(int x)
            {
                if (x == 0)
                {
                    return 0;
                }

                if (!cachedCosts.ContainsKey(x))
                {
                    cachedCosts[x] = IncreaseCost(x - 1) + x;
                }

                return cachedCosts[x];
            }

            long? bestCost = null;

            int lastPos = _horizontalPosCounts.Keys.Last();
            for (int pos = _horizontalPosCounts.Keys.First(); pos <= lastPos; pos++)
            {
                long cost = FuelCost(pos);
                if (bestCost == null || bestCost > cost)
                {
                    bestCost = cost;
                }
            }

            return new ValueTask<string>($"{bestCost ?? 0}");
        }
    }
}