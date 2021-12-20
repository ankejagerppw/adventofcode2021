using AoCHelper;

namespace AdventOfCode;

public class Day_18 : BaseDay
{
    private string[] _snailFishNumberStrings;

    private abstract class SnailFishNumber
    {
        public Pair Parent { get; set; }

        public abstract SingleNumber GetNumberMostRight();

        public abstract SingleNumber GetNumberMostLeft();

        public abstract Pair GetLeftMostNestedPair(int depth);

        public abstract SingleNumber GetLeftMostNumberTooHigh();

        public abstract long Magnitude();

        public static SnailFishNumber Parse(string s)
        {
            int charIdx = 0;

            SnailFishNumber ParseString()
            {
                if (charIdx >= s.Length)
                {
                    return null;
                }

                char c = s[charIdx++];
                if (char.IsDigit(c))
                {
                    return new SingleNumber(int.Parse(c.ToString()));
                }

                SnailFishNumber left = null;
                SnailFishNumber right = null;
                if (c == '[')
                {
                    left = ParseString();
                }

                c = s[++charIdx];
                right = ParseString();
                charIdx++; // char ]
                Pair p = new Pair(left, right);
                left.Parent = p;
                right.Parent = p;

                return p;
            }

            return ParseString();
        }
    }

    private class SingleNumber : SnailFishNumber
    {
        public SingleNumber(int number)
        {
            Number = number;
        }

        public int Number { get; set; }
        public override SingleNumber GetNumberMostRight()
        {
            return this;
        }

        public override SingleNumber GetNumberMostLeft()
        {
            return this;
        }

        public override Pair GetLeftMostNestedPair(int depth)
        {
            return null;
        }

        public override SingleNumber GetLeftMostNumberTooHigh()
        {
            return Number >= 10 ? this : null;
        }

        public override long Magnitude()
        {
            return Number;
        }

        public override string ToString()
        {
            return $"{Number}";
        }
    }

    private class Pair : SnailFishNumber
    {
        private SnailFishNumber _leftItem;
        private SnailFishNumber _rightItem;

        public SnailFishNumber LeftItem
        {
            get => _leftItem;
            set
            {
                if (_leftItem != null)
                {
                    _leftItem.Parent = null;
                }
                _leftItem = value;
                if (_leftItem != null)
                {
                    _leftItem.Parent = this;
                }
            }
        }

        public SnailFishNumber RightItem
        {
            get => _rightItem;
            set
            {
                if (_rightItem != null)
                {
                    _rightItem.Parent = null;
                }
                _rightItem = value;
                if (_rightItem != null)
                {
                    _rightItem.Parent = this;
                }
            }
        }

        public Pair(SnailFishNumber leftItem, SnailFishNumber rightItem)
        {
            LeftItem = leftItem;
            RightItem = rightItem;
        }

        public override SingleNumber GetNumberMostRight()
        {
            return RightItem.GetNumberMostRight();
        }

        public override SingleNumber GetNumberMostLeft()
        {
            return LeftItem.GetNumberMostLeft();
        }

        public override Pair GetLeftMostNestedPair(int depth)
        {
            if (depth == 0)
            {
                return this;
            }

            Pair pair = LeftItem.GetLeftMostNestedPair(depth - 1);
            if (pair == null)
            {
                pair = RightItem.GetLeftMostNestedPair(depth - 1);
            }

            return pair;
        }

        public override SingleNumber GetLeftMostNumberTooHigh()
        {
            return LeftItem.GetLeftMostNumberTooHigh() ?? RightItem.GetLeftMostNumberTooHigh();
        }

        public override long Magnitude()
        {
            return 3 * LeftItem.Magnitude() + 2 * RightItem.Magnitude();
        }

        public SingleNumber GetNumberToTheLeft()
        {
            if (Parent == null) return null;
            if (Parent.LeftItem == this) return Parent.GetNumberToTheLeft();
            return Parent.LeftItem.GetNumberMostRight();
        }

        public SingleNumber GetNumberToTheRight()
        {
            if (Parent == null) return null;
            if (Parent.RightItem == this) return Parent.GetNumberToTheRight();
            return Parent.RightItem.GetNumberMostLeft();
        }

        public override string ToString()
        {
            return $"[{LeftItem},{RightItem}]";
        }
    }

    public Day_18()
    {
        _snailFishNumberStrings = File
            .ReadAllLines(InputFilePath)
            .ToArray();
    }

    public override ValueTask<string> Solve_1()
    {
        SnailFishNumber result = SnailFishNumber.Parse(_snailFishNumberStrings[0]);
        for (int idx = 1; idx < _snailFishNumberStrings.Length; idx++)
        {
            result = new Pair(result, SnailFishNumber.Parse(_snailFishNumberStrings[idx]));
            Reduce(result);
        }

        return new ValueTask<string>($"Magnitude: {result.Magnitude()}");
    }

    public override ValueTask<string> Solve_2()
    {
        long maxMagnitude = 0L;
        for (int idxNbr1 = 0; idxNbr1 < _snailFishNumberStrings.Length - 1; idxNbr1++)
        {
            for (int idxNbr2 = idxNbr1 + 1; idxNbr2 < _snailFishNumberStrings.Length; idxNbr2++)
            {
                SnailFishNumber result1And2 = new Pair(
                    SnailFishNumber.Parse(_snailFishNumberStrings[idxNbr1]),
                    SnailFishNumber.Parse(_snailFishNumberStrings[idxNbr2]));
                Reduce(result1And2);
                SnailFishNumber result2And1 = new Pair(
                    SnailFishNumber.Parse(_snailFishNumberStrings[idxNbr2]),
                    SnailFishNumber.Parse(_snailFishNumberStrings[idxNbr1]));
                Reduce(result2And1);

                long max = Math.Max(result1And2.Magnitude(), result2And1.Magnitude());
                maxMagnitude = Math.Max(maxMagnitude, max);
            }
        }

        return new ValueTask<string>($"Max magnitude: {maxMagnitude}");
    }

    private static void Reduce(SnailFishNumber snailFishNumber)
    {
        bool continueReducing = true;

        // Console.WriteLine($"Before reducing: {snailFishNumber}");
        while (continueReducing)
        {
            Pair leftMostNestedPair = snailFishNumber.GetLeftMostNestedPair(4);
            if (leftMostNestedPair != null)
            {
                PerformExplosion(leftMostNestedPair);
                // Console.WriteLine($"Explode: {snailFishNumber}");
                continue;
            }

            SingleNumber numberTooHigh = snailFishNumber.GetLeftMostNumberTooHigh();
            if (numberTooHigh != null)
            {
                PerformSplit(numberTooHigh);
                // Console.WriteLine($"Split: {snailFishNumber}");
                continue;
            }

            continueReducing = false;
        }
    }

    private static void PerformExplosion(Pair pair)
    {
        if (pair.Parent == null)
        {
            return;
        }

        SingleNumber numberToTheLeft = pair.GetNumberToTheLeft();
        SingleNumber numberToTheRight = pair.GetNumberToTheRight();

        int leftNumberOfCurrentPair = ((SingleNumber)pair.LeftItem).Number;
        int rightNumberOfCurrentPair = ((SingleNumber)pair.RightItem).Number;

        if (pair.Parent.LeftItem == pair)
        {
            pair.Parent.LeftItem = new SingleNumber(0);
        }
        else
        {
            pair.Parent.RightItem = new SingleNumber(0);
        }

        if (numberToTheLeft != null)
        {
            numberToTheLeft.Number += leftNumberOfCurrentPair;
        }
        if (numberToTheRight != null)
        {
            numberToTheRight.Number += rightNumberOfCurrentPair;
        }
    }

    private static void PerformSplit(SingleNumber singleNumber)
    {
        int leftNbr = singleNumber.Number / 2;
        int rightNbr = (int)Math.Ceiling(singleNumber.Number / 2.0);

        Pair parent = singleNumber.Parent;
        Pair newPair = new Pair(new SingleNumber(leftNbr), new SingleNumber(rightNbr));
        if (parent.LeftItem == singleNumber)
        {
            parent.LeftItem = newPair;
        }
        else
        {
            parent.RightItem = newPair;
        }
    }
}