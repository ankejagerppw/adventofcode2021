using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_11 : BaseDay
    {
        private readonly Dictionary<(int row, int col), int> _input;
        private readonly int _nbrRows;
        private readonly int _nbrCols;

        private int TotalNbrOctopuses => _nbrCols * _nbrRows;

        public Day_11()
        {
            string[] inputStrings = File
                .ReadAllLines(InputFilePath)
                .ToArray();
            _nbrRows = inputStrings.Length;
            _nbrCols = inputStrings[0].Length;
            _input = new Dictionary<(int row, int col), int>();
            for (int rowIdx = 0; rowIdx < _nbrRows; rowIdx++)
            {
                for (int colIdx = 0; colIdx < _nbrCols; colIdx++)
                {
                    _input.Add((rowIdx, colIdx), int.Parse(inputStrings[rowIdx][colIdx].ToString()));
                }
            }
        }

        public override ValueTask<string> Solve_1()
        {
            int sumFlashes = 0;
            Dictionary<(int row, int col), int> currentSituation = new Dictionary<(int row, int col), int>(_input);

            for (int step = 1; step <= 100; step++)
            {
                List<(int row, int col)> flashed = new List<(int row, int col)>();
                foreach (KeyValuePair<(int row, int col), int> keyValuePair in currentSituation)
                {
                    currentSituation[keyValuePair.Key] += 1;
                }

                List<(int row, int col)> toFlash = currentSituation
                    .Where(cs => cs.Value > 9)
                    .Select(cs => cs.Key)
                    .ToList();

                currentSituation = PerformFlashes(toFlash, flashed, currentSituation);

                foreach (KeyValuePair<(int row, int col),int> octopus in currentSituation)
                {
                    if (currentSituation[octopus.Key] > 9)
                    {
                        currentSituation[octopus.Key] = 0;
                        sumFlashes++;
                    }
                }
            }

            for (int x = 0; x < _nbrRows; x++)
            {
                for (int y = 0; y < _nbrCols; y++)
                {
                    Console.Write($"{currentSituation[(x, y)]}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            return new ValueTask<string>($"Nbr of flashes after 100 steps: {sumFlashes}");
        }

        public override ValueTask<string> Solve_2()
        {
            Dictionary<(int row, int col), int> currentSituation = new Dictionary<(int row, int col), int>(_input);

            bool allFlashingTogether = false;
            int step = 0;
            while (!allFlashingTogether)
            {
                step++;
                List<(int row, int col)> flashed = new List<(int row, int col)>();
                foreach (KeyValuePair<(int row, int col), int> keyValuePair in currentSituation)
                {
                    currentSituation[keyValuePair.Key] += 1;
                }

                List<(int row, int col)> toFlash = currentSituation
                    .Where(cs => cs.Value > 9)
                    .Select(cs => cs.Key)
                    .ToList();

                currentSituation = PerformFlashes(toFlash, flashed, currentSituation);

                int sumFlashes = 0;
                foreach (KeyValuePair<(int row, int col),int> octopus in currentSituation)
                {
                    if (currentSituation[octopus.Key] > 9)
                    {
                        currentSituation[octopus.Key] = 0;
                        sumFlashes++;
                    }
                }

                allFlashingTogether = sumFlashes == TotalNbrOctopuses;
            }

            for (int x = 0; x < _nbrRows; x++)
            {
                for (int y = 0; y < _nbrCols; y++)
                {
                    Console.Write($"{currentSituation[(x, y)]}");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            return new ValueTask<string>($"Nbr of steps after flashing all together: {step}");
        }

        private IEnumerable<(int row, int col)> SurroundingOctopuses((int rowIdx, int colIdx) octopus, Dictionary<(int row, int col), int> currentSituation)
        {
            for (int x = Math.Max(octopus.rowIdx - 1, 0); x <= Math.Min(octopus.rowIdx + 1, _nbrRows - 1); x++)
            {
                for (int y = Math.Max(octopus.colIdx - 1, 0); y <= Math.Min(octopus.colIdx + 1, _nbrCols - 1); y++)
                {
                    if (!(x == octopus.rowIdx && y == octopus.colIdx) && currentSituation[(x, y)] <= 9)
                    {
                        yield return (x, y);
                    }
                }
            }
        }

        private Dictionary<(int row, int col), int> PerformFlashes(
            List<(int row, int col)> toFlashOctopuses,
            List<(int row, int col)> flashedOctopuses,
            Dictionary<(int row, int col), int> currentSituation)
        {
            flashedOctopuses.AddRange(toFlashOctopuses);
            foreach ((int row, int col) toFlashOctopus in toFlashOctopuses)
            {
                IEnumerable<(int row, int col)> surroundingOctopuses = SurroundingOctopuses(toFlashOctopus, currentSituation);
                foreach ((int row, int col) surroundingOctopusToIncrease in surroundingOctopuses.Except(flashedOctopuses).ToList())
                {
                    currentSituation[(surroundingOctopusToIncrease.row, surroundingOctopusToIncrease.col)] += 1;
                }
            }

            List<(int row, int col)> newToFlash = currentSituation
                .Where(cs => cs.Value > 9)
                .Select(cs => cs.Key)
                .Except(flashedOctopuses)
                .ToList();
            if (newToFlash.Count > 0)
            {
                currentSituation = PerformFlashes(newToFlash, flashedOctopuses, currentSituation);
            }

            return currentSituation;
        }

    }
}