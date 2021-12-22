using AoCHelper;

namespace AdventOfCode;

public class Day_20 : BaseDay
{
    private char[] _enhancementAlgorithm;
    private HashSet<(int row, int col)> _pixelsLit;

    public Day_20()
    {
        string[] input = File.ReadAllLines(InputFilePath).ToArray();
        _enhancementAlgorithm = input[0].ToArray();

        // _image = new Dictionary<(int row, int col), char>();
        _pixelsLit = new HashSet<(int row, int col)>();
        for (int rowIdx = 2; rowIdx < input.Length; rowIdx++)
        {
            for (int colIdx = 0; colIdx < input[2].Length; colIdx++)
            {
                if (input[rowIdx][colIdx] == '#')
                {
                    _pixelsLit.Add((rowIdx, colIdx));
                }
            }
        }
    }

    public override ValueTask<string> Solve_1()
    {
        HashSet<(int row, int col)> enhancedImage = EnhanceImage(2);
        return new ValueTask<string>($"Nbr of pixels lit: {enhancedImage.Count}");
    }

    public override ValueTask<string> Solve_2()
    {
        HashSet<(int row, int col)> enhancedImage = EnhanceImage(50);
        return new ValueTask<string>($"Nbr of pixels lit: {enhancedImage.Count}");
    }

    private HashSet<(int row, int col)> EnhanceImage(int nbrOfEnhancements)
    {
        HashSet<(int row, int col)> enhancedImage = new HashSet<(int row, int col)>(_pixelsLit);
        int minRow = enhancedImage.Min(p => p.row);
        int maxRow = enhancedImage.Max(p => p.row);
        int minCol = enhancedImage.Min(p => p.col);
        int maxCol = enhancedImage.Max(p => p.col);

        bool IsInsideBoundsOfImage(int row, int col) => row >= minRow && row <= maxRow && col >= minCol && col <= maxCol;

        IEnumerable<(int row, int col)> SurroundingPixels((int row, int col) pixel)
        {
            for (int rowNbr = pixel.row - 1; rowNbr <= pixel.row + 1; rowNbr++)
            {
                for (int colNbr = pixel.col - 1; colNbr <= pixel.col + 1; colNbr++)
                {
                    yield return (rowNbr, colNbr);
                }
            }
        }

        int PositionInEnhancementAlgorithm((int row, int col) pixel, bool isInfinite)
        {
            IEnumerable<(int row, int col)> surroundingPixels = SurroundingPixels(pixel);
            string binaryString = string.Empty;
            foreach ((int row, int col) px in surroundingPixels.OrderBy(sp => sp.row).ThenBy(sp => sp.col).ToList())
            {
                bool insideBoundsOfImage = IsInsideBoundsOfImage(px.row, px.col);
                binaryString += insideBoundsOfImage && enhancedImage.Contains(px) ? '1' : (!insideBoundsOfImage && isInfinite ? '1' : '0');
            }

            return Convert.ToInt32(binaryString, 2);
        }

        bool NewInfiniteState(bool isInfinite)
        {
            // needed to determine what should be done when at borders (found this through trial and error)
            return _enhancementAlgorithm[0] switch
            {
                '.' => false,
                '#' when _enhancementAlgorithm.Last() == '.' => !isInfinite,
                _ => true
            };
        }

        bool isInfinite = false;
        for (int enhancementTimes = 0; enhancementTimes < nbrOfEnhancements; enhancementTimes++)
        {
            HashSet<(int row, int col)> tempImage = new HashSet<(int row, int col)>();
            for (int rowIdx = minRow - 1; rowIdx <= maxRow + 1; rowIdx++)
            {
                for (int colIdx = minCol - 1; colIdx <= maxCol + 1; colIdx++)
                {
                    if (_enhancementAlgorithm[PositionInEnhancementAlgorithm((rowIdx, colIdx), isInfinite)] == '#')
                    {
                        tempImage.Add((rowIdx, colIdx));
                    }
                }
            }

            enhancedImage = new HashSet<(int row, int col)>(tempImage);
            minRow = enhancedImage.Min(p => p.row);
            maxRow = enhancedImage.Max(p => p.row);
            minCol = enhancedImage.Min(p => p.col);
            maxCol = enhancedImage.Max(p => p.col);

            isInfinite = NewInfiniteState(isInfinite);
        }

        return enhancedImage;
    }
}