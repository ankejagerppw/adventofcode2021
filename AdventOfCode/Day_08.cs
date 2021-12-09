using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_08 : BaseDay
    {
        private readonly string[] _input;
        private Dictionary<List<string>, List<string>> _signalPatterns;

        public Day_08()
        {
            string SortStringByChar(string s) => new string(s.OrderBy(c => c).ToArray());

            _input = File.ReadAllLines(InputFilePath)
                .Select(s => s.Substring(s.IndexOf(" | ") + 3))
                .SelectMany(s => s.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                .ToArray();

            _signalPatterns = File.ReadAllLines(InputFilePath)
                .ToDictionary(
                    s => s.Substring(0, s.IndexOf(" | "))
                                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                    .Select(SortStringByChar)
                                    .ToList(),
                    s => s.Substring(s.IndexOf(" | ") + 3)
                                    .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                                    .Select(SortStringByChar)
                                    .ToList());
        }

        public override ValueTask<string> Solve_1()
        {
            return new ValueTask<string>(
                $"{_input.Count(i => i.Length is 2 or 3 or 4 or 7)}");
        }

        public override ValueTask<string> Solve_2()
        {
            bool ContainsAllChars(string searchStr, string findChars)
            {
                return findChars.All(searchStr.Contains);
            }

            long sum = 0;
            foreach (KeyValuePair<List<string>,List<string>> signalPattern in _signalPatterns)
            {
                Dictionary<string, string> mappedDigits = new Dictionary<string, string>();
                List<string> patterns = signalPattern.Key;
                // length 2, 3, 4, 7 unique

                mappedDigits.Add("1", patterns.Single(s => s.Length == 2));
                mappedDigits.Add("7", patterns.Single(s => s.Length == 3));
                mappedDigits.Add("4", patterns.Single(s => s.Length == 4));
                mappedDigits.Add("8", patterns.Single(s => s.Length == 7));

                List<string> grp235 = patterns.Where(s => s.Length == 5).ToList();
                List<string> grp069 = patterns.Where(s => s.Length == 6).ToList();

                mappedDigits.Add("3", grp235.Single(s => ContainsAllChars(s, mappedDigits["1"])));
                grp235.Remove(mappedDigits["3"]);

                mappedDigits.Add("9", grp069.Single(s => ContainsAllChars(s, mappedDigits["3"])));
                grp069.Remove(mappedDigits["9"]);
                mappedDigits.Add("0", grp069.Single(s => ContainsAllChars(s, mappedDigits["7"])));
                grp069.Remove(mappedDigits["0"]);
                mappedDigits.Add("6", grp069.Single());

                mappedDigits.Add("5", grp235.Single(s => ContainsAllChars(mappedDigits["6"], s)));
                grp235.Remove(mappedDigits["5"]);
                mappedDigits.Add("2", grp235.Single());

                string resultingDigit = signalPattern.Value
                    .Aggregate(
                        "",
                        (current, mappedDigit) => current + mappedDigits.Single(md => md.Value == mappedDigit).Key);
                sum += int.Parse(resultingDigit);
            }

            return new ValueTask<string>($"{sum}");
        }
    }
}