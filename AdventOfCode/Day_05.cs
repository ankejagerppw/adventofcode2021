using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_05 : BaseDay
    {
        private string[] _input;
        private List<VentLine> _ventLines;

        private class VentLine
        {
            public int x1 { get; }
            public int y1 { get; }
            public int x2 { get; }
            public int y2 { get; }

            // format: x1,y1 -> x2,y2
            public VentLine(string s)
            {
                Regex regex = new Regex("(?<x1>\\d+),(?<y1>\\d+) -> (?<x2>\\d+),(?<y2>\\d+)");
                Match match = regex.Match(s);
                if (match.Success)
                {
                    x1 = int.Parse(match.Groups["x1"].Value);
                    y1 = int.Parse(match.Groups["y1"].Value);
                    x2 = int.Parse(match.Groups["x2"].Value);
                    y2 = int.Parse(match.Groups["y2"].Value);
                }
            }

            public bool OnHorizontalOrVerticalLine()
            {
                return x1 == x2 || y1 == y2;
            }

            public bool OnDiagonalLine()
            {
                int minX = x1 <= x2 ? x1 : x2;
                int minY = y1 <= y2 ? y1 : y2;
                int maxX = x1 > x2 ? x1 : x2;
                int maxY = y1 > y2 ? y1 : y2;
                return x1 != x2 && y1 != y2 && (maxX - minX == maxY - minY);
            }
        }

        public Day_05()
        {
            _input = File
                .ReadAllLines(InputFilePath)
                .ToArray();

            _ventLines = _input
                .Select(s => new VentLine(s))
                .ToList();
        }

        public override ValueTask<string> Solve_1()
        {
            List<VentLine> horizontalOrVerticalLines = _ventLines.Where(vl => vl.OnHorizontalOrVerticalLine()).ToList();
            Dictionary<int, List<int>> coordinates = CalculateCoordinatesHorizontalVertical(horizontalOrVerticalLines);

            // calculate nbrs of duplicates for each entry in dictionary
            int result = coordinates
                .Sum(c => c.Value.GroupBy(x => x).Count(x => x.Count() > 1));

            return new ValueTask<string>($"{result}");
        }


        public override ValueTask<string> Solve_2()
        {
            List<VentLine> horizontalOrVerticalLines = _ventLines.Where(vl => vl.OnHorizontalOrVerticalLine()).ToList();
            Dictionary<int, List<int>> coordinates = CalculateCoordinatesHorizontalVertical(horizontalOrVerticalLines);

            List<VentLine> diagonalLines = _ventLines.Where(vl => vl.OnDiagonalLine()).ToList();
            foreach (VentLine ventLine in diagonalLines)
            {
                int offset = 0;
                int nextX = ventLine.x1 < ventLine.x2 ? 1 : -1;
                if (ventLine.y1 < ventLine.y2)
                {
                    for (int horizontalLine = ventLine.y1; horizontalLine <= ventLine.y2; horizontalLine++)
                    {
                        if (!coordinates.ContainsKey(horizontalLine))
                        {
                            coordinates.Add(horizontalLine, new List<int>());
                        }

                        coordinates[horizontalLine].Add(ventLine.x1 + offset);
                        offset += nextX;
                    }
                }
                else
                {
                    for (int horizontalLine = ventLine.y1; horizontalLine >= ventLine.y2; horizontalLine--)
                    {
                        if (!coordinates.ContainsKey(horizontalLine))
                        {
                            coordinates.Add(horizontalLine, new List<int>());
                        }

                        coordinates[horizontalLine].Add(ventLine.x1 + offset);
                        offset += nextX;
                    }
                }
            }

            // calculate nbrs of duplicates for each entry in dictionary
            int result = coordinates
                .Sum(c => c.Value.GroupBy(x => x).Count(x => x.Count() > 1));

            return new ValueTask<string>($"{result}");
        }

        private static Dictionary<int, List<int>> CalculateCoordinatesHorizontalVertical(List<VentLine> horizontalOrVerticalLines)
        {
            Dictionary<int, List<int>> coordinates = new Dictionary<int, List<int>>();
            foreach (VentLine ventLine in horizontalOrVerticalLines)
            {
                int minY = ventLine.y1 <= ventLine.y2 ? ventLine.y1 : ventLine.y2;
                int maxY = ventLine.y1 >= ventLine.y2 ? ventLine.y1 : ventLine.y2;
                for (int horizontalLine = minY; horizontalLine <= maxY; horizontalLine++)
                {
                    if (!coordinates.ContainsKey(horizontalLine))
                    {
                        coordinates.Add(horizontalLine, new List<int>());
                    }

                    int minX = ventLine.x1 <= ventLine.x2 ? ventLine.x1 : ventLine.x2;
                    int maxX = ventLine.x1 >= ventLine.x2 ? ventLine.x1 : ventLine.x2;
                    coordinates[horizontalLine].AddRange(Enumerable.Range(minX, maxX - minX + 1));
                }
            }

            return coordinates;
        }
    }
}