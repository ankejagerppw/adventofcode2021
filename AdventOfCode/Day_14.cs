using AoCHelper;

namespace AdventOfCode
{
    public class Day_14 : BaseDay
    {
        private readonly Dictionary<string, char> _pairInsertionRules;
        private readonly (Dictionary<string, long> currentPairCounts, Dictionary<char, long> letterCounts) _startData;

        public Day_14()
        {
            string[] input = File
                .ReadAllLines(InputFilePath)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            _pairInsertionRules = input
                .Skip(1)
                .Select(i => i.Split(" -> "))
                .ToDictionary(i => i[0], i => i[1][0]);

            List<string> pairs = new List<string>();
            string polymerTemplate = input[0];
            for (int strIdx = 1; strIdx < polymerTemplate.Length; strIdx++)
            {
                pairs.Add(polymerTemplate.Substring(strIdx - 1, 2));
            }

            _startData =
                (pairs
                        .GroupBy(p => p)
                        .ToDictionary(p => p.Key, p => p.LongCount()),
                    polymerTemplate
                        .Select(c => c)
                        .Distinct()
                        .ToDictionary(c => c, c => polymerTemplate.LongCount(ptc => ptc == c)));

        }

        public override ValueTask<string> Solve_1()
        {
            (Dictionary<string, long> currentPairCounts, Dictionary<char, long> letterCounts) data =
                (new Dictionary<string, long>(_startData.currentPairCounts),
                    new Dictionary<char, long>(_startData.letterCounts));

            for (int step = 1; step <= 10; step++)
            {
                data = ProcessStep(data.currentPairCounts, data.letterCounts);
            }

            return new ValueTask<string>($"{data.letterCounts.Max(lc => lc.Value) - data.letterCounts.Min(lc => lc.Value)}");
        }

        public override ValueTask<string> Solve_2()
        {
            (Dictionary<string, long> currentPairCounts, Dictionary<char, long> letterCounts) data =
                (new Dictionary<string, long>(_startData.currentPairCounts),
                    new Dictionary<char, long>(_startData.letterCounts));

            for (int step = 1; step <= 40; step++)
            {
                data = ProcessStep(data.currentPairCounts, data.letterCounts);
            }

            return new ValueTask<string>($"{data.letterCounts.Max(lc => lc.Value) - data.letterCounts.Min(lc => lc.Value)}");
        }

        private (Dictionary<string, long>, Dictionary<char, long>) ProcessStep(Dictionary<string, long> currentPairCounts, Dictionary<char, long> letterCounts)
        {
            Dictionary<string, long> tempPairCounts = new Dictionary<string, long>(currentPairCounts);
            foreach (string pair in currentPairCounts.Where(cpc => cpc.Value > 0).Select(cpc => cpc.Key).ToList())
            {
                if (!_pairInsertionRules.TryGetValue(pair, out char result)) continue;
                List<string> newPairs = new List<string>
                {
                    $"{pair[0]}{result}",
                    $"{result}{pair[1]}"
                };
                tempPairCounts[pair] -= currentPairCounts[pair];
                foreach (string newPair in newPairs)
                {
                    if (tempPairCounts.ContainsKey(newPair))
                    {
                        tempPairCounts[newPair] += currentPairCounts[pair];
                    }
                    else
                    {
                        tempPairCounts.Add(newPair, currentPairCounts[pair]);
                    }
                }

                if (letterCounts.ContainsKey(result))
                {
                    letterCounts[result] += currentPairCounts[pair];
                }
                else
                {
                    letterCounts.Add(result, currentPairCounts[pair]);
                }
            }

            return (tempPairCounts, letterCounts);
        }
    }
}