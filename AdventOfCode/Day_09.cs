using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;
// ReSharper disable All

namespace AdventOfCode
{
    public class Day_09 : BaseDay
    {
        private int[][] _input;
        private List<ArrayIndex> _arrayIndicesToExclude;

        private struct ArrayIndex
        {
            public ArrayIndex(int rowIdx, int colIdx)
            {
                RowIdx = rowIdx;
                ColIdx = colIdx;
            }

            public int RowIdx { get; }
            public int ColIdx { get; }

            public override string ToString()
            {
                return $"[{RowIdx}, {ColIdx}]";
            }
        }

        public Day_09()
        {
            _input = File
                .ReadAllLines(InputFilePath)
                .Select(s => s.ToCharArray().Select(c => int.Parse(c.ToString())).ToArray())
                .ToArray();
            _arrayIndicesToExclude = new List<ArrayIndex>();
        }

        public override ValueTask<string> Solve_1()
        {
            long sum = 0L;
            int cnt = 0;
            for (int rowIdx = 0; rowIdx < _input.Length; rowIdx++)
            {
                for (int colIdx = 0; colIdx < _input[rowIdx].Length; colIdx++)
                {
                    int el = _input[rowIdx][colIdx];
                    List<int> elementsToCompareWith = new List<int>();
                    if (rowIdx > 0)
                    {
                        elementsToCompareWith.Add(_input[rowIdx - 1][colIdx]);
                    }

                    if (rowIdx < _input.Length - 1)
                    {
                        elementsToCompareWith.Add(_input[rowIdx + 1][colIdx]);
                    }

                    if (colIdx > 0)
                    {
                        elementsToCompareWith.Add(_input[rowIdx][colIdx - 1]);
                    }

                    if (colIdx < _input[rowIdx].Length - 1)
                    {
                        elementsToCompareWith.Add(_input[rowIdx][colIdx + 1]);
                    }

                    if (elementsToCompareWith.All(comp => comp > el))
                    {
                        sum += el + 1;
                        cnt++;
                    }
                }
            }

            return new ValueTask<string>($"{sum} with {cnt} low points");
        }

        public override ValueTask<string> Solve_2()
        {
            Dictionary<ArrayIndex, List<ArrayIndex>> basins = new Dictionary<ArrayIndex, List<ArrayIndex>>();

            for (int rowIdx = 0; rowIdx < _input.Length; rowIdx++)
            {
                for (int colIdx = 0; colIdx < _input[rowIdx].Length; colIdx++)
                {
                    int el = _input[rowIdx][colIdx];
                    List<int> elementsToCompareWith = new List<int>();
                    if (rowIdx > 0)
                    {
                        elementsToCompareWith.Add(_input[rowIdx - 1][colIdx]);
                    }

                    if (rowIdx < _input.Length - 1)
                    {
                        elementsToCompareWith.Add(_input[rowIdx + 1][colIdx]);
                    }

                    if (colIdx > 0)
                    {
                        elementsToCompareWith.Add(_input[rowIdx][colIdx - 1]);
                    }

                    if (colIdx < _input[rowIdx].Length - 1)
                    {
                        elementsToCompareWith.Add(_input[rowIdx][colIdx + 1]);
                    }

                    if (elementsToCompareWith.All(comp => comp > el))
                    {
                        basins.Add(new ArrayIndex(rowIdx, colIdx), CalculateBasin(new ArrayIndex(rowIdx, colIdx)));
                    }
                }
            }

            List<int> basinSizes = basins.Select(b => b.Value.Distinct().Count()).ToList();

            List<int> orderByDescending = basinSizes
                .OrderByDescending(bs => bs).ToList();
            long result = orderByDescending
                .Take(3)
                .Aggregate<int, long>(
                    1L,
                    (current, basinSize) => current * basinSize);

            return new ValueTask<string>($"{result}");
        }

        private List<ArrayIndex> CalculateBasin(ArrayIndex currentArrayIndex)
        {
            int rowIdx = currentArrayIndex.RowIdx;
            int colIdx = currentArrayIndex.ColIdx;

            if (_input[rowIdx][colIdx] == 9)
            {
                return new List<ArrayIndex>();
            }

            List<ArrayIndex> basin = new List<ArrayIndex>
            {
                currentArrayIndex
            };
            _arrayIndicesToExclude.Add(currentArrayIndex);

            List<ArrayIndex> arrayIndices = new List<ArrayIndex>();
            if (rowIdx > 0
                && !_arrayIndicesToExclude.Exists(a => a.RowIdx == rowIdx - 1 && a.ColIdx == colIdx))
            {
                arrayIndices.Add(new ArrayIndex(rowIdx - 1, colIdx));
            }

            if (rowIdx < _input.Length - 1
                && !_arrayIndicesToExclude.Exists(a => a.RowIdx == rowIdx + 1 && a.ColIdx == colIdx))
            {
                arrayIndices.Add(new ArrayIndex(rowIdx + 1, colIdx));
            }

            if (colIdx > 0
                && !_arrayIndicesToExclude.Exists(a => a.RowIdx == rowIdx && a.ColIdx == colIdx - 1))
            {
                arrayIndices.Add(new ArrayIndex(rowIdx, colIdx - 1));
            }

            if (colIdx < _input[rowIdx].Length - 1
                && !_arrayIndicesToExclude.Exists(a => a.RowIdx == rowIdx && a.ColIdx == colIdx + 1))
            {
                arrayIndices.Add(new ArrayIndex(rowIdx, colIdx + 1));
            }

            foreach (ArrayIndex arrayIndex in arrayIndices)
            {
                basin.AddRange(CalculateBasin(arrayIndex));
            }

            return basin;
        }
    }
}