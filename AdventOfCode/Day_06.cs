using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_06 : BaseDay
    {
        private const int ResetValue = 6;
        private const int NewFishTimerValue = 8;
        private int[] _input;

        public Day_06()
        {
            _input = File
                .ReadAllLines(InputFilePath)
                .SelectMany(line => line.Split(","))
                .Select(int.Parse)
                .ToArray();
        }

        public override ValueTask<string> Solve_1()
        {
            return new ValueTask<string>($"{CalculateNbrFishAfterGivenDays(80)}");
        }

        public override ValueTask<string> Solve_2()
        {
            return new ValueTask<string>($"{CalculateNbrFishAfterGivenDays(256)}");
        }

        private long CalculateNbrFishAfterGivenDays(int days)
        {
            Dictionary<int, long> timers = _input.GroupBy(i => i).ToDictionary(i => i.Key, i => (long)i.Count());

            for (int day = 0; day < days; day++)
            {
                Dictionary<int, long> newTimers = new Dictionary<int, long>();
                long nbrReset = 0L;
                if (timers.ContainsKey(0))
                {
                    nbrReset = timers[0];
                }

                newTimers = timers
                    .Where(t => t.Key != 0)
                    .ToDictionary(t => t.Key - 1, t => t.Value);
                if (newTimers.ContainsKey(ResetValue))
                {
                    newTimers[ResetValue] += nbrReset;
                }
                else
                {
                    newTimers.Add(ResetValue, nbrReset);
                }

                newTimers.Add(NewFishTimerValue, nbrReset);

                timers = newTimers;
            }

            return timers.Sum(t => t.Value);
        }
    }
}