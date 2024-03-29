﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_12 : BaseDay
    {
        private static readonly string START = "start";
        private static readonly string END = "end";

        private Dictionary<string, List<string>> _inputPaths;

        public Day_12()
        {
            string[] inputStrings = File
                .ReadAllLines(InputFilePath)
                .ToArray();

            _inputPaths = new();
            foreach (string inputString in inputStrings)
            {
                string[] path = inputString.Split("-");
                if (path.Length != 2) continue;

                string startingPoint = path[0];
                string endingPoint = path[1];

                if (string.Equals(startingPoint, endingPoint)) continue;

                if (!string.Equals(startingPoint, END) && !string.Equals(endingPoint, START))
                {
                    if (!_inputPaths.ContainsKey(startingPoint))
                    {
                        _inputPaths.Add(startingPoint, new List<string> { endingPoint });
                    }
                    else
                    {
                        _inputPaths[startingPoint].Add(endingPoint);
                    }
                }

                if (string.Equals(startingPoint, START) || string.Equals(endingPoint, END)) continue;

                if (!_inputPaths.ContainsKey(endingPoint))
                {
                    _inputPaths.Add(endingPoint, new List<string> { startingPoint });
                }
                else
                {
                    _inputPaths[endingPoint].Add(startingPoint);
                }
            }
        }

        public override ValueTask<string> Solve_1()
        {
            List<List<string>> possiblePaths = new List<List<string>>();

            void DiscoverPossiblePaths(string s, List<string> existingPath)
            {
                existingPath.Add(s);
                if (string.Equals(s, END))
                {
                    possiblePaths.Add(existingPath);
                }

                List<string> possibilities;
                if (!_inputPaths.TryGetValue(s, out possibilities))
                {
                    return;
                }

                HashSet<string> visitedSmallCaves = existingPath.Where(SmallCave).ToHashSet();
                possibilities = possibilities.Except(visitedSmallCaves).ToList();

                foreach (string possibility in possibilities)
                {
                    List<string> tempPath = new List<string>(existingPath);
                    DiscoverPossiblePaths(possibility, tempPath);
                }
            }

            DiscoverPossiblePaths(START, new());

            // foreach (List<string> possiblePath in possiblePaths)
            // {
            //     Console.WriteLine(string.Join("-", possiblePath));
            // }

            return new ValueTask<string>($"Nbr of distinct possible paths: {possiblePaths.Count}");
        }

        public override ValueTask<string> Solve_2()
        {
            int nbrPaths = 0;

            void DiscoverPossiblePaths(string s, Dictionary<string, int> smallCavesCount, int depth = 0)
            {
                if (string.Equals(s, END))
                {
                    nbrPaths++;
                    return;
                }

                List<string> possibilities;
                if (!_inputPaths.TryGetValue(s, out possibilities))
                {
                    return;
                }

                if (SmallCave(s))
                {
                    if (!smallCavesCount.ContainsKey(s))
                    {
                        smallCavesCount.Add(s, 1);
                    }
                    else
                    {
                        if (smallCavesCount.Any(d => d.Value == 2))
                        {
                            return;
                        }

                        smallCavesCount[s] += 1;
                    }
                }

                foreach (string possibility in possibilities)
                {
                    DiscoverPossiblePaths(possibility, new Dictionary<string, int>(smallCavesCount), depth+1);
                }
            }

            DiscoverPossiblePaths(START, new());

            return new ValueTask<string>($"Nbr of distinct possible paths: {nbrPaths}");
        }

        private static bool SmallCave(string s) => !String.Equals(s, START) && !string.Equals(s, END) && s.All(char.IsLower);

    }
}