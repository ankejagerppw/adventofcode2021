using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_03 : BaseDay
    {
        private List<string> _input;

        public Day_03()
        {
            _input = File
                .ReadAllLines(InputFilePath)
                .ToList();
        }

        public override ValueTask<string> Solve_1()
        {
            StringBuilder gammaSb = new StringBuilder();
            StringBuilder epsilonSb = new StringBuilder();
            int lengthOfStrings = _input[0].Length;
            for (int strIdx = 0; strIdx < lengthOfStrings; strIdx++)
            {
                List<char> charsAtPos = _input
                    .Select(s => s[strIdx])
                    .ToList();
                if (charsAtPos.Count(c => c == '0') > charsAtPos.Count(c => c == '1'))
                {
                    gammaSb.Append('0');
                    epsilonSb.Append('1');
                }
                else
                {
                    gammaSb.Append('1');
                    epsilonSb.Append('0');
                }
            }

            int gamma = Convert.ToInt32(gammaSb.ToString(), 2);
            int epsilon = Convert.ToInt32(epsilonSb.ToString(), 2);

            return new ValueTask<string>($"{gamma * epsilon}");
        }

        public override ValueTask<string> Solve_2()
        {
            int lengthOfStrings = _input[0].Length;
            List<string> resultOxygen = new List<string>(_input);
            List<string> resultCo2 = new List<string>(_input);

            for (int strIdx = 0; strIdx < lengthOfStrings; strIdx++)
            {
                if (resultOxygen.Count > 1)
                {
                    resultOxygen = DoCalculation(resultOxygen, strIdx, true);
                }

                if (resultCo2.Count > 1)
                {
                    resultCo2 = DoCalculation(resultCo2, strIdx, false);
                }
            }

            int oxygen = Convert.ToInt32(resultOxygen.Single(), 2);
            int co2 = Convert.ToInt32(resultCo2.Single(), 2);
            return new ValueTask<string>($"{oxygen * co2}");
        }

        private static List<string> DoCalculation(List<string> input, int strIdx, bool calculateMostCommon)
        {
            List<string> zeroList = input.Where(s => s[strIdx] == '0').ToList();
            List<string> oneList = input.Where(s => s[strIdx] == '1').ToList();

            int nbrZeros = zeroList.Count;
            int nbrOnes = oneList.Count;

            return calculateMostCommon ?
                new List<string>(nbrOnes >= nbrZeros ? oneList : zeroList)
                : new List<string>(nbrZeros <= nbrOnes ? zeroList : oneList);
        }
    }
}