using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    [SuppressMessage("ReSharper", "SuggestVarOrType_Elsewhere")]
    public class Day_10 : BaseDay
    {
        private static readonly Dictionary<char, char> BracketSets = new()
        {
            { ')', '(' },
            { ']', '[' },
            { '}', '{' },
            { '>', '<' },
        };

        private string[] _input;

        public Day_10()
        {
            _input = File
                .ReadAllLines(InputFilePath)
                .ToArray();
        }

        public override ValueTask<string> Solve_1()
        {
            int PointsForWrongBracket(char bracket)
            {
                switch (bracket)
                {
                    case ')': return 3;
                    case ']': return 57;
                    case '}': return 1197;
                    default: return 25137;
                }
            }

            long sum = 0;
            foreach (string s in _input)
            {
                Stack<char> stack = new Stack<char>();
                bool isCorrect = true;
                for (int charIdx = 0; charIdx < s.Length && isCorrect; charIdx++)
                {
                    char bracket = s[charIdx];
                    // if closing bracket
                    if (BracketSets.ContainsKey(bracket))
                    {
                        if (!stack.TryPop(out char bracketAtTop) || BracketSets[bracket] != bracketAtTop)
                        {
                            sum += PointsForWrongBracket(bracket);
                            isCorrect = false;
                        }
                    }
                    else
                    {
                        // if opening bracket
                        stack.Push(bracket);
                    }
                }
            }

            return new ValueTask<string>($"{sum}");
        }

        public override ValueTask<string> Solve_2()
        {
            long AddPointsForClosingBracket(long result, char bracket)
            {
                int points = 0;
                switch (bracket)
                {
                    case '(': points = 1; break;
                    case '[': points = 2; break;
                    case '{': points = 3; break;
                    case '<': points = 4; break;
                }

                return result * 5 + points;
            }

            List<long> allScores = new List<long>();
            foreach (string s in _input)
            {
                Stack<char> stack = new Stack<char>();
                bool isCorrect = true;
                for (int charIdx = 0; charIdx < s.Length && isCorrect; charIdx++)
                {
                    char bracket = s[charIdx];
                    // if closing bracket
                    if (BracketSets.ContainsKey(bracket))
                    {
                        isCorrect = stack.TryPop(out char bracketAtTop) && BracketSets[bracket] == bracketAtTop;
                    }
                    else
                    {
                        // if opening bracket
                        stack.Push(bracket);
                    }
                }

                // isCorrect means there was no error found
                if (isCorrect)
                {
                    long result = 0L;
                    while (stack.TryPop(out char openingBracket))
                    {
                        result = AddPointsForClosingBracket(result, openingBracket);
                    }
                    allScores.Add(result);
                }
            }

            return new ValueTask<string>($"{allScores.OrderBy(s => s).Skip(allScores.Count / 2).First()}");
        }
    }
}