using System.Text.RegularExpressions;
using AoCHelper;

namespace AdventOfCode;

public class Day_19 : BaseDay
{
    private const int BeaconMinOverlap = 12;
    private readonly List<Scanner> _scanners;
    private readonly LocationComparer _locationComparer = new LocationComparer();
    private Scanner _baseScanner;
    private List<Location> _scannerLocations;

    private class Location
    {
        public Location(int firstCoord, int secondCoord, int thirdCoord)
        {
            FirstCoord = firstCoord;
            SecondCoord = secondCoord;
            ThirdCoord = thirdCoord;
        }

        // NOT naming the coordinates x, y, z because this might get confusing as scanners can rotate
        public int FirstCoord { get; set; }
        public int SecondCoord { get; set; }
        public int ThirdCoord { get; set; }

        public override string ToString()
        {
            return $"({FirstCoord},{SecondCoord},{ThirdCoord})";
        }
    }

    private class Scanner
    {
        public int ScannerId { get; set; }
        public List<Location> BeaconLocations { get; set; }
        public Location RealLocation { get; set; }

        public Scanner(int scannerId)
        {
            ScannerId = scannerId;
            BeaconLocations = new List<Location>();
        }
    }

    private class LocationComparer : IEqualityComparer<Location>
    {
        public bool Equals(Location x, Location y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.FirstCoord == y.FirstCoord && x.SecondCoord == y.SecondCoord && x.ThirdCoord == y.ThirdCoord;
        }

        public int GetHashCode(Location obj)
        {
            return HashCode.Combine(obj.FirstCoord, obj.SecondCoord, obj.ThirdCoord);
        }
    }

    public Day_19()
    {
        Regex scannerRegex = new Regex("--- scanner (?<scannerId>\\d+) ---");
        Regex locationRegex = new Regex("(?<firstCoord>-{0,1}\\d+),(?<secondCoord>-{0,1}\\d+),(?<thirdCoord>-{0,1}\\d+)");
        string[] input = File.ReadAllLines(InputFilePath).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        _scanners = new List<Scanner>();
        Scanner scanner = null;
        foreach (string s in input)
        {
            Match m = scannerRegex.Match(s);
            if (m.Success)
            {
                if (scanner != null)
                {
                    _scanners.Add(scanner);
                }
                scanner = new Scanner(int.Parse(m.Groups["scannerId"].Value));
            }
            else
            {
                Match locationMatch = locationRegex.Match(s);
                if (locationMatch.Success)
                {
                    scanner.BeaconLocations.Add(
                        new Location(
                            int.Parse(locationMatch.Groups["firstCoord"].Value),
                            int.Parse(locationMatch.Groups["secondCoord"].Value),
                            int.Parse(locationMatch.Groups["thirdCoord"].Value)));
                }
                else
                {
                    throw new ApplicationException($"Input line does not match any regex! -> {s}");
                }
            }
        }

        if (scanner != null)
        {
            _scanners.Add(scanner);
        }

        _scannerLocations = new List<Location>();
    }

    public override ValueTask<string> Solve_1()
    {
        if (_baseScanner == null)
        {
            FindBeaconsAndScanners();
        }

        return new ValueTask<string>($"Nbr of beacons: {_baseScanner.BeaconLocations.Distinct(_locationComparer).Count()}");
    }

    public override ValueTask<string> Solve_2()
    {
        if (_baseScanner == null)
        {
            FindBeaconsAndScanners();
        }

        long ManhattanDistance(Location location1, Location location2)
        {
            if (location1 == null || location2 == null) return 0L;
            return Math.Abs(location1.FirstCoord - location2.FirstCoord)
                   + Math.Abs(location1.SecondCoord - location2.SecondCoord)
                   + Math.Abs(location1.ThirdCoord - location2.ThirdCoord);
        }

        long maxDistance = 0L;
        for (int idx1 = 0; idx1 < _scannerLocations.Count - 1; idx1++)
        {
            for (int idx2 = idx1 + 1; idx2 < _scannerLocations.Count; idx2++)
            {
                maxDistance = Math.Max(maxDistance, ManhattanDistance(_scannerLocations[idx1], _scannerLocations[idx2]));
            }
        }

        return new ValueTask<string>($"Max Manhattan distance: {maxDistance}");
    }

    private void FindBeaconsAndScanners()
    {
        if (!_scanners.Any())
        {
            throw new ApplicationException("There should at least be 1 scanner!");
        }

        if (_scanners.Select(s => s.ScannerId).Distinct().Count() < _scanners.Count)
        {
            throw new ApplicationException("Every scanner should have a unique id");
        }

        _baseScanner = _scanners.First();
        _baseScanner.RealLocation = new Location(0, 0, 0);
        _scannerLocations.Add(_baseScanner.RealLocation);

        List<Scanner> notResolvedScanners = new List<Scanner>(_scanners.Where(s => s.ScannerId != _baseScanner.ScannerId));

        while (notResolvedScanners.Any())
        {
            Console.WriteLine($"Nbr of not resolved scanners: {notResolvedScanners.Count}");
            List<int> newlySolvedScannerIds = new List<int>();
            foreach (Scanner notResolvedScanner in notResolvedScanners)
            {
                bool matchFound = false;
                List<Location> notResolvedScannerBeacons = notResolvedScanner.BeaconLocations;
                // for (int resolvedScannerIdx = 0; resolvedScannerIdx < resolvedScanners.Count && !matchFound; resolvedScannerIdx++)
                // {
                //     Scanner resolvedScanner = resolvedScanners[resolvedScannerIdx];
                List<Location> resolvedBeacons = _baseScanner.BeaconLocations;
                for (int x = 0; x < 4 && !matchFound; x++)
                {
                    for (int y = 0; y < 4 && !matchFound; y++)
                    {
                        for (int z = 0; z < 4 && !matchFound; z++)
                        {
                            List<Location> limitedNbrNotResolvedBeacons = notResolvedScannerBeacons.Take(notResolvedScannerBeacons.Count - BeaconMinOverlap + 1).ToList();
                            List<Location> limitedNbrResolvedBeacons = resolvedBeacons.Take(resolvedBeacons.Count - BeaconMinOverlap + 1).ToList();
                            for (int notResolvedBeaconIdx = 0; notResolvedBeaconIdx < limitedNbrNotResolvedBeacons.Count && !matchFound; notResolvedBeaconIdx++)
                            {
                                Location notResolvedBeacon = limitedNbrNotResolvedBeacons[notResolvedBeaconIdx];
                                for (int resolvedBeaconIdx = 0; resolvedBeaconIdx < limitedNbrResolvedBeacons.Count && !matchFound; resolvedBeaconIdx++)
                                {
                                    Location resolvedBeacon = limitedNbrResolvedBeacons[resolvedBeaconIdx];
                                    (int firstCoord, int secondCoord, int thirdCoord) diff = (
                                        notResolvedBeacon.FirstCoord - resolvedBeacon.FirstCoord,
                                        notResolvedBeacon.SecondCoord - resolvedBeacon.SecondCoord,
                                        notResolvedBeacon.ThirdCoord - resolvedBeacon.ThirdCoord
                                    );

                                    List<Location> modifiedNotResolvedBeaconLocations = notResolvedScannerBeacons
                                        .Select(b => new Location(
                                            b.FirstCoord - diff.firstCoord,
                                            b.SecondCoord - diff.secondCoord,
                                            b.ThirdCoord - diff.thirdCoord))
                                        .ToList();

                                    int overlaps = modifiedNotResolvedBeaconLocations.Intersect(resolvedBeacons, _locationComparer).Count();
                                    // Console.WriteLine($"Nbr of overlaps: {overlaps}");
                                    if (overlaps >= BeaconMinOverlap)
                                    {
                                        Console.WriteLine("Found enough overlaps!!");
                                        _scannerLocations.Add(new Location(0 - diff.firstCoord, 0 - diff.secondCoord, 0 - diff.thirdCoord));
                                        _baseScanner.BeaconLocations.AddRange(modifiedNotResolvedBeaconLocations);
                                        // resolvedScanners.Add(notResolvedScanner);
                                        newlySolvedScannerIds.Add(notResolvedScanner.ScannerId);

                                        matchFound = true;
                                    }
                                }
                            }

                            if (!matchFound)
                            {
                                // no match found
                                // rotate 90 degrees over z-axis
                                notResolvedScannerBeacons = notResolvedScannerBeacons
                                    .Select(b => new Location(0 - b.SecondCoord, b.FirstCoord, b.ThirdCoord))
                                    .ToList();
                            }
                        }

                        if (!matchFound)
                        {
                            // rotate 90 degrees over y-axis
                            notResolvedScannerBeacons = notResolvedScannerBeacons
                                .Select(b => new Location(0 - b.ThirdCoord, b.SecondCoord, b.FirstCoord))
                                .ToList();
                        }
                    }

                    if (!matchFound)
                    {
                        // rotate 90 degrees over x-axis
                        notResolvedScannerBeacons = notResolvedScannerBeacons
                            .Select(b => new Location(b.FirstCoord, 0 - b.ThirdCoord, b.SecondCoord))
                            .ToList();
                    }
                }
            }

            notResolvedScanners = notResolvedScanners
                .Where(s => !newlySolvedScannerIds.Contains(s.ScannerId))
                .ToList();
        }
    }
}