using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_02 : BaseDay
    {
        private enum Direction
        {
            up,
            down,
            forward
        }

        private class PositionChange
        {
            public PositionChange(string directionWithNumber)
            {
                string[] split = directionWithNumber.Split(" ");
                Direction = Enum.Parse<Direction>(split[0]);
                Number = int.Parse(split[1]);

                if (Direction == Direction.up)
                {
                    Direction = Direction.down;
                    Number *= -1;
                }
            }

            public readonly Direction Direction;
            public readonly int Number;
        }

        private PositionChange[] _input;

        public Day_02()
        {
            _input = File
                .ReadAllLines(InputFilePath)
                .Select(s => new PositionChange(s))
                .ToArray();
        }

        public override ValueTask<string> Solve_1()
        {
            int horizontalPosition  = _input.Where(pc => pc.Direction == Direction.forward).Sum(pc => pc.Number);
            int depth = _input.Where(pc => pc.Direction == Direction.down).Sum(pc => pc.Number);
            return new ValueTask<string>($"{horizontalPosition * depth}");
        }

        public override ValueTask<string> Solve_2()
        {
            int aim = 0;
            int horizontalPosition = 0;
            int depth = 0;
            foreach (PositionChange positionChange in _input)
            {
                switch (positionChange.Direction)
                {
                    case Direction.down:
                        aim += positionChange.Number;
                        break;
                    case Direction.forward:
                        horizontalPosition += positionChange.Number;
                        depth += aim * positionChange.Number;
                        break;
                }
            }

            return new ValueTask<string>($"{horizontalPosition * depth}");
        }
    }
}