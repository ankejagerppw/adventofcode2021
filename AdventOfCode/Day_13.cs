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

        private readonly bool[,] _dots;
        private readonly List<(string fold, int coordinate)> _folds;

        public Day_13()
        {
            Regex regexDots = new Regex("(?<col>\\d+),(?<row>\\d+)");
            string[] input = File
                .ReadAllLines(InputFilePath)
                .ToArray();

            var dots = input
                .Select(i => regexDots.Match(i))
                .Where(m => m.Success)
                .Select(m => new
                {
                    row = int.Parse(m.Groups["row"].Value),
                    col = int.Parse(m.Groups["col"].Value)
                })
                .ToList();
            int maxRow = dots.Max(d => d.row);
            int maxCol = dots.Max(d => d.col);
            _dots = new bool[maxRow + 1, maxCol + 1];

            foreach (var dot in dots)
            {
                _dots[dot.row, dot.col] = true;
            }

            Regex regexFolds = new Regex("fold along (?<foldtype>x|y)=(?<lineNbr>\\d+)");
            var folds = input
                .Select(i => regexFolds.Match(i))
                .Where(m => m.Success)
                .Select(m => new
                {
                    foldType = m.Groups["foldtype"].Value,
                    lineNbr = int.Parse(m.Groups["lineNbr"].Value)
                })
                .ToList();
            _folds = new List<(string fold, int coordinate)>();
            foreach (var fold in folds)
            {
                string foldType = fold.foldType == "y" ? Horizontal : Vertical;
                _folds.Add((foldType, fold.lineNbr));
            }
        }

        public override ValueTask<string> Solve_1()
        {
            // only first fold
            (string foldType, int lineNbr) fold = _folds.First();
            bool[,] afterFold = DoFold(fold, _dots);
            int count = afterFold.Cast<bool>().Count(af => af);

            return new ValueTask<string>($"{count}");
        }

        public override ValueTask<string> Solve_2()
        {
            bool[,] afterFold = _folds.Aggregate(_dots, (current, fold) => DoFold(fold, current));
            for (int rowIdx = 0; rowIdx < afterFold.GetLength(0); rowIdx++)
            {
                for (int colIdx = 0; colIdx < afterFold.GetLength(1); colIdx++)
                {
                    Console.Write($"{(afterFold[rowIdx, colIdx] ? "#" : ".")}");
                }

                Console.WriteLine();
            }

            return new ValueTask<string>("See what nice string is printed");
        }

        private static bool[,] DoFold((string foldType, int lineNbr) fold, bool[,] currentDots)
        {
            bool[,] result;
            int currentNbrRows = currentDots.GetLength(0);
            int currentNbrCols = currentDots.GetLength(1);
            if (fold.foldType == Vertical)
            {
                int nbrCols = Math.Max(fold.lineNbr, currentNbrCols - fold.lineNbr - 1);
                result = new bool[currentNbrRows, nbrCols];
                for (int rowIdx = 0; rowIdx < currentNbrRows; rowIdx++)
                {
                    for (int colOffset = 1;
                        fold.lineNbr - colOffset >= 0 || fold.lineNbr + colOffset < currentNbrCols;
                        colOffset++)
                    {
                        int minColIdx = fold.lineNbr - colOffset;
                        int maxColIdx = fold.lineNbr + colOffset;
                        bool b = minColIdx >= 0 && currentDots[rowIdx, minColIdx];
                        b |= maxColIdx < currentNbrCols && currentDots[rowIdx, maxColIdx];

                        result[rowIdx, minColIdx] = b;
                    }
                }
            }
            else
            {
                int nbrRows = Math.Max(fold.lineNbr, currentNbrRows - fold.lineNbr - 1);
                result = new bool[nbrRows, currentNbrCols];
                for (int rowOffset = 1;
                    fold.lineNbr - rowOffset >= 0 || fold.lineNbr + rowOffset < currentNbrRows;
                    rowOffset++)
                {
                    for (int colIdx = 0; colIdx < currentNbrCols; colIdx++)
                    {
                        int minRowIdx = fold.lineNbr - rowOffset;
                        int maxRowIdx = fold.lineNbr + rowOffset;
                        bool b = minRowIdx >= 0 && currentDots[minRowIdx, colIdx];
                        b |= maxRowIdx < currentNbrRows && currentDots[maxRowIdx, colIdx];

                        result[minRowIdx, colIdx] = b;
                    }
                }
            }

            return result;
        }
    }
}