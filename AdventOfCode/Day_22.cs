using System.Text;
using System.Text.RegularExpressions;
using AoCHelper;

namespace AdventOfCode;

public class Day_22 : BaseDay
{
    private static readonly Regex RebootStepRegex =
        new Regex("(?<power>on|off) x=(?<x1>-?\\d+)\\.\\.(?<x2>-?\\d+),y=(?<y1>-?\\d+)\\.\\.(?<y2>-?\\d+),z=(?<z1>-?\\d+)\\.\\.(?<z2>-?\\d+)");

    private List<RebootStep> _rebootSteps;
    private CuboidComparer _cuboidComparer = new CuboidComparer();

    private enum Power
    {
        ON,
        OFF
    }

    private class Cuboid
    {
        public long xMin { get; set; }
        public long xMax { get; set; }
        public long yMin { get; set; }
        public long yMax { get; set; }
        public long zMin { get; set; }
        public long zMax { get; set; }

        public long NbrOfCubes => (xMax - xMin + 1) * (yMax - yMin + 1) * (zMax - zMin + 1);

        public bool IntersectsWith(Cuboid otherCuboid)
        {
            if (otherCuboid == null) return false;
            return (xMin <= otherCuboid.xMax && otherCuboid.xMin <= xMax) // intersect on x-axis
                   && (yMin <= otherCuboid.yMax && otherCuboid.yMin <= yMax) // intersect on y-axis
                   && (zMin <= otherCuboid.zMax && otherCuboid.zMin <= zMax); // intersect on z-axis
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Cuboid x={xMin}..{xMax},y={yMin}..{yMax},z={zMin}..{zMax}");
            for (long x = xMin; x <= xMax; x++)
            {
                for (long y = yMin; y <= yMax; y++)
                {
                    for (long z = zMin; z <= zMax; z++)
                    {
                        sb.AppendLine($"{x},{y},{z}");
                    }
                }
            }

            // sb.AppendLine();
            return sb.ToString();
        }
    }

    private class CuboidComparer : IEqualityComparer<Cuboid>
    {
        public bool Equals(Cuboid x, Cuboid y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.xMin == y.xMin && x.xMax == y.xMax && x.yMin == y.yMin && x.yMax == y.yMax && x.zMin == y.zMin && x.zMax == y.zMax;
        }

        public int GetHashCode(Cuboid obj)
        {
            return HashCode.Combine(obj.xMin, obj.xMax, obj.yMin, obj.yMax, obj.zMin, obj.zMax);
        }
    }

    private class RebootStep
    {
        public Power Power { get; set; }
        public Cuboid Cuboid { get; set; }

        public static RebootStep Parse(string s)
        {
            Match match = RebootStepRegex.Match(s);
            if (!match.Success) return null;
            RebootStep rebootStep = new RebootStep();
            long x1 = long.Parse(match.Groups["x1"].Value);
            long x2 = long.Parse(match.Groups["x2"].Value);
            long y1 = long.Parse(match.Groups["y1"].Value);
            long y2 = long.Parse(match.Groups["y2"].Value);
            long z1 = long.Parse(match.Groups["z1"].Value);
            long z2 = long.Parse(match.Groups["z2"].Value);
            rebootStep.Power = Enum.Parse<Power>(match.Groups["power"].Value.ToUpper());
            rebootStep.Cuboid = new Cuboid
            {
                xMin = Math.Min(x1, x2),
                xMax = Math.Max(x1, x2),
                yMin = Math.Min(y1, y2),
                yMax = Math.Max(y1, y2),
                zMin = Math.Min(z1, z2),
                zMax = Math.Max(z1, z2)
            };

            return rebootStep;
        }
    }

    public Day_22()
    {
        _rebootSteps = File
            .ReadAllLines(InputFilePath)
            .Select(RebootStep.Parse)
            .ToList();
    }

    public override ValueTask<string> Solve_1()
    {
        const int minRange = -50;
        const int maxRange = 50;

        HashSet<(long x, long y, long z)> cuboidsOn = new HashSet<(long x, long y, long z)>();

        bool FullyOutsideRange(RebootStep rebootStep)
        {
            return rebootStep.Cuboid.xMax < minRange || rebootStep.Cuboid.xMin > maxRange
                  || rebootStep.Cuboid.yMax < minRange || rebootStep.Cuboid.yMin > maxRange
                  || rebootStep.Cuboid.zMax < minRange || rebootStep.Cuboid.zMin > maxRange;
        }

        IEnumerable<(long x, long y, long z)> CubesPositions(RebootStep rebootStep)
        {
            for (long x = Math.Max(rebootStep.Cuboid.xMin, minRange); x <= Math.Min(rebootStep.Cuboid.xMax, maxRange); x++)
            {
                for (long y = Math.Max(rebootStep.Cuboid.yMin, minRange); y <= Math.Min(rebootStep.Cuboid.yMax, maxRange); y++)
                {
                    for (long z = Math.Max(rebootStep.Cuboid.zMin, minRange); z <= Math.Min(rebootStep.Cuboid.zMax, maxRange); z++)
                    {
                        yield return (x, y, z);
                    }
                }
            }
        }

        List<RebootStep> rebootStepsWithinRange =
            _rebootSteps
                .Where(s => !FullyOutsideRange(s))
                .ToList();

        // int stepCounter = 1;
        // int totalSteps = rebootStepsWithinRange.Count;
        foreach (RebootStep step in rebootStepsWithinRange)
        {
            // Console.WriteLine($"Reboot step {stepCounter++} of {totalSteps}");
            IEnumerable<(long x, long y, long z)> cubesWithinRange = CubesPositions(step);
            if (step.Power == Power.ON)
            {
                cuboidsOn.UnionWith(cubesWithinRange);
            }
            else
            {
                cuboidsOn.ExceptWith(cubesWithinRange);
            }
        }

        return new ValueTask<string>($"Nbr of cubes turned on: {cuboidsOn.Count}");
    }

    public override ValueTask<string> Solve_2()
    {
        HashSet<Cuboid> cuboidsOn = new HashSet<Cuboid>();

        IEnumerable<(long xMin, long xMax)> AllPossibleXPairs(Cuboid existingCuboid, Cuboid newCuboid)
        {
            if (existingCuboid.xMin >= newCuboid.xMin && existingCuboid.xMax <= newCuboid.xMax)
            {
                yield return (existingCuboid.xMin, existingCuboid.xMax);
            }
            else if (existingCuboid.xMin >= newCuboid.xMin && existingCuboid.xMax > newCuboid.xMax)
            {
                yield return (existingCuboid.xMin, newCuboid.xMax);
                yield return (newCuboid.xMax + 1, existingCuboid.xMax);
            }
            else if (existingCuboid.xMin < newCuboid.xMin && existingCuboid.xMax <= newCuboid.xMax)
            {
                yield return (existingCuboid.xMin, newCuboid.xMin - 1);
                yield return (newCuboid.xMin, existingCuboid.xMax);
            }
            else if (existingCuboid.xMin < newCuboid.xMin && existingCuboid.xMax > newCuboid.xMax)
            {
                yield return (existingCuboid.xMin, newCuboid.xMin - 1);
                yield return (newCuboid.xMin, newCuboid.xMax);
                yield return (newCuboid.xMax + 1, existingCuboid.xMax);
            }
        }

        IEnumerable<(long yMin, long yMax)> AllPossibleYPairs(Cuboid existingCuboid, Cuboid newCuboid)
        {
            if (existingCuboid.yMin >= newCuboid.yMin && existingCuboid.yMax <= newCuboid.yMax)
            {
                yield return (existingCuboid.yMin, existingCuboid.yMax);
            }
            else if (existingCuboid.yMin >= newCuboid.yMin && existingCuboid.yMax > newCuboid.yMax)
            {
                yield return (existingCuboid.yMin, newCuboid.yMax);
                yield return (newCuboid.yMax + 1, existingCuboid.yMax);
            }
            else if (existingCuboid.yMin < newCuboid.yMin && existingCuboid.yMax <= newCuboid.yMax)
            {
                yield return (existingCuboid.yMin, newCuboid.yMin - 1);
                yield return (newCuboid.yMin, existingCuboid.yMax);
            }
            else if (existingCuboid.yMin < newCuboid.yMin && existingCuboid.yMax > newCuboid.yMax)
            {
                yield return (existingCuboid.yMin, newCuboid.yMin - 1);
                yield return (newCuboid.yMin, newCuboid.yMax);
                yield return (newCuboid.yMax + 1, existingCuboid.yMax);
            }
        }

        IEnumerable<(long zMin, long zMax)> AllPossibleZPairs(Cuboid existingCuboid, Cuboid newCuboid)
        {
            if (existingCuboid.zMin >= newCuboid.zMin && existingCuboid.zMax <= newCuboid.zMax)
            {
                yield return (existingCuboid.zMin, existingCuboid.zMax);
            }
            else if (existingCuboid.zMin >= newCuboid.zMin && existingCuboid.zMax > newCuboid.zMax)
            {
                yield return (existingCuboid.zMin, newCuboid.zMax);
                yield return (newCuboid.zMax + 1, existingCuboid.zMax);
            }
            else if (existingCuboid.zMin < newCuboid.zMin && existingCuboid.zMax <= newCuboid.zMax)
            {
                yield return (existingCuboid.zMin, newCuboid.zMin - 1);
                yield return (newCuboid.zMin, existingCuboid.zMax);
            }
            else if (existingCuboid.zMin < newCuboid.zMin && existingCuboid.zMax > newCuboid.zMax)
            {
                yield return (existingCuboid.zMin, newCuboid.zMin - 1);
                yield return (newCuboid.zMin, newCuboid.zMax);
                yield return (newCuboid.zMax + 1, existingCuboid.zMax);
            }
        }

        IEnumerable<Cuboid> Except(Cuboid existingCuboid, Cuboid newCuboid)
        {
            IEnumerable<(long xMin, long xMax)> allPossibleXPairs = AllPossibleXPairs(existingCuboid, newCuboid).ToList();
            IEnumerable<(long yMin, long yMax)> allPossibleYPairs = AllPossibleYPairs(existingCuboid, newCuboid).ToList();
            IEnumerable<(long zMin, long zMax)> allPossibleZPairs = AllPossibleZPairs(existingCuboid, newCuboid).ToList();
            foreach ((long xMin, long xMax) xPair in allPossibleXPairs)
            {
                foreach ((long yMin, long yMax) yPair in allPossibleYPairs)
                {
                    foreach ((long zMin, long zMax) zPair in allPossibleZPairs)
                    {
                        if (!(xPair.xMin >= newCuboid.xMin && xPair.xMax <= newCuboid.xMax
                            && yPair.yMin >= newCuboid.yMin && yPair.yMax <= newCuboid.yMax
                            && zPair.zMin >= newCuboid.zMin && zPair.zMax <= newCuboid.zMax))
                            yield return new Cuboid
                            {
                                xMin = xPair.xMin,
                                xMax = xPair.xMax,
                                yMin = yPair.yMin,
                                yMax = yPair.yMax,
                                zMin = zPair.zMin,
                                zMax = zPair.zMax
                            };
                    }
                }
            }
        }

        List<RebootStep> allRebootSteps = new List<RebootStep>(_rebootSteps);

        // int stepCounter = 1;
        // int totalSteps = allRebootSteps.Count;
        foreach (RebootStep step in allRebootSteps)
        {
            // Console.WriteLine($"Reboot step {stepCounter++} of {totalSteps}");
            HashSet<Cuboid> newCuboidsOn = new HashSet<Cuboid>(_cuboidComparer);
            foreach (Cuboid cuboidOn in cuboidsOn)
            {
                if (cuboidOn.IntersectsWith(step.Cuboid))
                {
                    // add all parts of cuboidOn except the intersection
                    newCuboidsOn.UnionWith(Except(cuboidOn, step.Cuboid));
                }
                else
                {
                    // there is no intersection, so we can just add cuboidOn
                    newCuboidsOn.Add(cuboidOn);
                }
            }

            if (step.Power == Power.ON)
            {
                newCuboidsOn.Add(step.Cuboid);
            }

            cuboidsOn = new HashSet<Cuboid>(newCuboidsOn, _cuboidComparer);
        }

        return new ValueTask<string>($"Nbr of cubes turned on: {cuboidsOn.Select(c => c.NbrOfCubes).Sum()}");
    }
}