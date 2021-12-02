using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode;

public class Day_01 : BaseDay
{
    private int[] _input;

    public Day_01()
    {
        _input = File
            .ReadLines(InputFilePath)
            .Select(int.Parse)
            .ToArray();
    }

    public override ValueTask<string> Solve_1()
    {
        int incremented = 0;
        for (int idx = 1; idx < _input.Length; idx++)
        {
            if (_input[idx - 1] < _input[idx])
            {
                incremented++;
            }
        }

        return new($"{incremented}");
    }

    public override ValueTask<string> Solve_2()
    {
        int incremented = 0;
        for (int idx = 0; idx <= _input.Length - 4; idx++)
        {
            if (_input[idx] < _input[idx + 3])
            {
                incremented++;
            }
        }

        return new($"{incremented}");
    }
}
