using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_13 : BaseDay
    {
        private const string Horizontal = "HORIZONTAL";
        private const string Vertical = "VERTICAL";

        private readonly HashSet<(int row, int col)> _dots;
        private readonly List<(string fold, int coordinate)> _folds;

        public Day_13()
        {
            Regex regexDots = new Regex("(?<col>\\d+),(?<row>\\d+)");
            string[] input = File
                .ReadAllLines(InputFilePath)
                .ToArray();

            _dots = input
                .Select(i => regexDots.Match(i))
                .Where(m => m.Success)
                .Select(m => (int.Parse(m.Groups["row"].Value), int.Parse(m.Groups["col"].Value)))
                .ToHashSet();

            Regex regexFolds = new Regex("fold along (?<foldtype>x|y)=(?<lineNbr>\\d+)");
            _folds = input
                .Select(i => regexFolds.Match(i))
                .Where(m => m.Success)
                .Select(m => (m.Groups["foldtype"].Value == "y" ? Horizontal : Vertical, int.Parse(m.Groups["lineNbr"].Value)))
                .ToList();
        }

        public override ValueTask<string> Solve_1()
        {
            // only first fold
            (string foldType, int lineNbr) fold = _folds.First();
            HashSet<(int, int)> afterFold = DoFold(fold, _dots);

            return new ValueTask<string>($"{afterFold.Count}");
        }

        public override ValueTask<string> Solve_2()
        {
            HashSet<(int row, int col)> afterFold = new HashSet<(int row, int col)>(_dots);
            foreach ((string fold, int coordinate) fold in _folds)
            {
                afterFold = DoFold(fold, afterFold);
            }

            int nbrRows = afterFold.Max(af => af.row);
            int nbrCols = afterFold.Max(af => af.col);

            for (int rowIdx = 0; rowIdx <= nbrRows; rowIdx++)
            {
                for (int colIdx = 0; colIdx <= nbrCols; colIdx++)
                {
                    Console.Write($"{(afterFold.Contains((rowIdx, colIdx)) ? "#" : ".")}");
                }
                Console.WriteLine();
            }

            return new ValueTask<string>("See what nice string is printed");
        }

        private static HashSet<(int row, int col)> DoFold((string foldType, int lineNbr) fold, HashSet<(int row, int col)> currentDots)
        {
            HashSet<(int row, int col)> result;
            if (fold.foldType == Vertical)
            {
                // calculate shift as the fold line might be before the middle
                int shiftNbrCols = fold.lineNbr - (currentDots.Max(d => d.col) - fold.lineNbr);
                shiftNbrCols = shiftNbrCols < 0 ? Math.Abs(shiftNbrCols) : 0;
                result = currentDots
                    .Where(d => d.col != fold.lineNbr)
                    .Select(d =>
                        (d.row,
                        shiftNbrCols + (d.col < fold.lineNbr ? d.col : fold.lineNbr - (d.col - fold.lineNbr))))
                    .ToHashSet();
            }
            else
            {
                // calculate shift as the fold line might be before the middle
                int shiftNbrRows = fold.lineNbr - (currentDots.Max(d => d.row) - fold.lineNbr);
                shiftNbrRows = shiftNbrRows < 0 ? Math.Abs(shiftNbrRows) : 0;
                result = currentDots
                    .Where(d => d.row != fold.lineNbr)
                    .Select(d =>
                        (shiftNbrRows + (d.row < fold.lineNbr ? d.row : fold.lineNbr - (d.row - fold.lineNbr)),
                            d.col))
                    .ToHashSet();
            }

            return result;
        }
    }
}