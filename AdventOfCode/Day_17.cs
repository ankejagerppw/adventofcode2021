using System.Text.RegularExpressions;
using AoCHelper;

namespace AdventOfCode;

public class Day_17 : BaseDay
{
    private readonly List<(int x, int y)> _area;
    private int _minX;
    private int _maxX;
    private int _minY;
    private int _maxY;

    public Day_17()
    {
        string input = File.ReadAllText(InputFilePath);
        Regex r = new Regex("target area: x=(?<x1>.+)\\.\\.(?<x2>.+), y=(?<y1>.+)\\.\\.(?<y2>.+)");
        Match match = r.Match(input);
        _area = new List<(int x, int y)>();
        if (!match.Success)
            return;

        int x1 = int.Parse(match.Groups["x1"].Value);
        int x2 = int.Parse(match.Groups["x2"].Value);
        _minX = Math.Min(x1, x2);
        _maxX = Math.Max(x1, x2);

        int y1 = int.Parse(match.Groups["y1"].Value);
        int y2 = int.Parse(match.Groups["y2"].Value);
        _minY = Math.Min(y1, y2);
        _maxY = Math.Max(y1, y2);
        for (int x = _minX; x <= _maxX; x++)
        {
            for (int y = _minY; y <= _maxY; y++)
            {
                _area.Add((x, y));
            }
        }
    }

    public override ValueTask<string> Solve_1()
    {
        int Gauss(int n) => (n * (n - 1)) / 2;

        int maxYVelocity = Gauss(Math.Max(Math.Abs(_maxY), Math.Abs(_minY)));

        return new ValueTask<string>($"Max possible y position: {maxYVelocity}");
    }

    public override ValueTask<string> Solve_2()
    {
        // first try to get the range for the x velocity
        Dictionary<int, List<int>> CalcPossibleXVelocities()
        {
            Dictionary<int, List<int>> possibleVelocities = new Dictionary<int, List<int>>();

            for (int x = 1; x <= _maxX; x++)
            {
                int maxNbrOfSteps = x;
                List<int> steps = new List<int>();
                int xPos = 0;
                for (int step = 1; step <= maxNbrOfSteps; step++)
                {
                    xPos += x - (step - 1);
                    if (xPos >= _minX && xPos <= _maxX)
                    {
                        steps.Add(step);
                    }
                }

                if (steps.Any())
                {
                    possibleVelocities.Add(x, steps);
                }
            }

            return possibleVelocities;
        }

        Dictionary<int, List<int>> CalcPossibleYVelocities()
        {
            int minYVelocity = _minY;
            int maxYVelocity = Math.Max(Math.Abs(_maxY), Math.Abs(_minY) - 1);
            Dictionary<int, List<int>> possibleVelocities = new Dictionary<int, List<int>>();

            for (int yVelocity = Math.Min(minYVelocity, 1); yVelocity <= maxYVelocity; yVelocity++)
            {
                int maxNbrOfSteps = 3 * maxYVelocity;
                List<int> steps = new List<int>();
                int yPos = 0;
                for (int step = 1; step <= maxNbrOfSteps; step++)
                {
                    yPos += yVelocity - (step - 1);
                    if (yPos >= _minY && yPos <= _maxY)
                    {
                        steps.Add(step);
                    }
                }

                if (steps.Any())
                {
                    possibleVelocities.Add(yVelocity, steps);
                }
            }

            return possibleVelocities;
        }

        Dictionary<int,List<int>> xVelocities = CalcPossibleXVelocities();
        Dictionary<int,List<int>> yVelocities = CalcPossibleYVelocities();
        HashSet<(int, int)> velocities = new HashSet<(int, int)>();

        // xVelocities that stabilize after xVelocity steps
        // pick yVelocities that reach the y-target area with at least xVelocity steps
        List<int> xv = xVelocities
            .Where(v => v.Value.Contains(v.Key))
            .Select(v => v.Key)
            .ToList();
        foreach (int xVelocity in xv)
        {
            List<int> selectedYVelocities = yVelocities
                .Where(yV => yV.Value.Any(y => y >= xVelocity))
                .Select(yv => yv.Key)
                .ToList();
            foreach (int yVelocity in selectedYVelocities)
            {
                velocities.Add((xVelocity, yVelocity));
            }
        }

        foreach (int xVelocity in xVelocities.Keys)
        {
            List<int> xSteps = xVelocities[xVelocity].Where(xv => xv < xVelocity).ToList();
            List<int> selectedYVelocities = yVelocities
                .Where(yv => yv.Value.Intersect(xSteps).Any())
                .Select(yv => yv.Key)
                .ToList();
            foreach (int selectedYVelocity in selectedYVelocities)
            {
                velocities.Add((xVelocity, selectedYVelocity));
            }
        }

        return new ValueTask<string>($"Distinct velocities possible: {velocities.Count}");
    }
}