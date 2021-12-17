using AoCHelper;

namespace AdventOfCode;

public class Day_16 : BaseDay
{
    private string _binaryString;

    public Day_16()
    {
        string input = File.ReadAllText(InputFilePath);
        _binaryString = string.Join(
            string.Empty,
            input.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
    }

    public override ValueTask<string> Solve_1()
    {
        (List<long>, int, long sumVersions) packet = HandlePacket(_binaryString, null, 1);

        return new ValueTask<string>($"Sum of versions: {packet.sumVersions}");
    }

    public override ValueTask<string> Solve_2()
    {
        (List<long> packageValues, int, long) packet = HandlePacket(_binaryString, null, 1);

        return new ValueTask<string>($"Sum of versions: {packet.packageValues.Single()}");
    }

    private static (List<long> packageValues, int packageLength, long sumVersions) HandlePacket(string packetStr, int? nbrOfPackages, int idx)
    {
        List<long> packageValues = new List<long>();
        long sumVersions = 0;

        if (packetStr.Length <= 6 || packetStr.All(c => c == '0'))
        {
            return (packageValues, packetStr.Length, 0);
        }

        // int sumVersion = 0;
        int packageLength = 0;
        bool continueWithPacket = true;
        int handledPackages = 0;

        while (continueWithPacket)
        {
            int packetVersion = Convert.ToInt32(packetStr.Substring(0, 3), 2);
            sumVersions += packetVersion;
            int packetTypeId = Convert.ToInt32(packetStr.Substring(3, 3), 2);
            packageLength += 6;

            string restOfPacket = packetStr.Substring(6);
            if (packetTypeId != 4)
            {
                // operator package
                char c = restOfPacket[0];
                packageLength++;
                restOfPacket = restOfPacket[1..];
                List<long> resultingValues;
                if (c == '0')
                {
                    int lengthOfSubpackagesInBits = Convert.ToInt32(restOfPacket.Substring(0, 15), 2);
                    Console.WriteLine($"{string.Join(String.Empty, Enumerable.Repeat(" ", idx - 1))}PacketTypeId: {packetTypeId} / Length of subpackages in bits: {lengthOfSubpackagesInBits}");
                    (List<long> packageValues, int packageLength, long sumVersions) packet = HandlePacket(restOfPacket.Substring(15, lengthOfSubpackagesInBits), null, idx + 1);
                    sumVersions += packet.sumVersions;
                    packageLength += 15 + lengthOfSubpackagesInBits;
                    resultingValues = packet.packageValues;

                    packetStr = restOfPacket.Substring(15 + lengthOfSubpackagesInBits);
                }
                else
                {
                    int nbrOfSubpackages = Convert.ToInt32(restOfPacket.Substring(0, 11), 2);
                    packageLength += 11;
                    Console.WriteLine($"{string.Join(string.Empty, Enumerable.Repeat(" ", idx - 1))}PacketTypeId: {packetTypeId} / Number of subpackages in bits: {nbrOfSubpackages}");
                    (List<long> packageValues, int packageLength, long sumVersions) packet = HandlePacket(restOfPacket[11..], nbrOfSubpackages, idx + 1);
                    sumVersions += packet.sumVersions;
                    packageLength += packet.packageLength;
                    packetStr = restOfPacket[(11 + packet.packageLength)..];
                    resultingValues = packet.packageValues;
                }
                Console.WriteLine($"{string.Join(string.Empty, Enumerable.Repeat(" ", idx - 1))}PacketTypeId: {packetTypeId} Resulting values: {string.Join(", ", resultingValues)}");
                long value = CalcValueBasedOnOperator(packetTypeId, resultingValues);
                packageValues.Add(value);
            }
            else
            {
                Console.WriteLine($"{string.Join(string.Empty, Enumerable.Repeat(" ", idx - 1))}PacketTypeId: {packetTypeId}");
                // literal package
                int origLength = restOfPacket.Length;
                bool lastPackageProcessed = restOfPacket.Length < 5;
                string binaryRepresentation = string.Empty;
                while (!lastPackageProcessed)
                {
                    char firstBit = restOfPacket[0];
                    binaryRepresentation += restOfPacket.Substring(1, 4);
                    lastPackageProcessed = firstBit == '0' || restOfPacket.Length < 5;
                    restOfPacket = restOfPacket.Length > 5 ? restOfPacket[5..] : restOfPacket;
                }
                long literalValue = Convert.ToInt64(binaryRepresentation, 2);
                packageValues.Add(literalValue);
                Console.WriteLine($"{string.Join(string.Empty, Enumerable.Repeat(" ", idx ))}Literal value: {literalValue}");

                packageLength += origLength - restOfPacket.Length;
                packetStr = restOfPacket;
            }

            continueWithPacket = packetStr.Length > 6 && packetStr.Any(c => c != '0');
            handledPackages++;
            if (nbrOfPackages != null)
            {
                continueWithPacket = continueWithPacket && handledPackages < nbrOfPackages;
            }
        }

        return (packageValues, packageLength, sumVersions);
    }

    private static long CalcValueBasedOnOperator(int packetTypeId, List<long> values)
    {
        return packetTypeId switch
        {
            0 => values.Sum(),
            1 => values.Aggregate<long, long>(1, (current, value) => current * value),
            2 => values.Min(),
            3 => values.Max(),
            5 => values.First() > values.Last() ? 1 : 0,
            6 => values.First() < values.Last() ? 1 : 0,
            7 => values.First() == values.Last() ? 1 : 0,
            _ => 0
        };
    }
}