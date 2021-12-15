using AoCHelper;

namespace AdventOfCode;

public class Day_15 : BaseDay
{
    private readonly int[,] _risksTask1;
    private readonly int[,] _risksTask2;

    public Day_15()
    {
        string[] inputStr = File
            .ReadAllLines(InputFilePath)
            .ToArray();
        int nbrRows = inputStr.Length;
        int nbrCols = inputStr[0].Length;
        _risksTask1 = new int[nbrRows, nbrCols];
        _risksTask2 = new int[nbrRows * 5, nbrCols * 5];
        for (int rowIdx = 0; rowIdx < nbrRows; rowIdx++)
        {
            for (int colIdx = 0; colIdx < nbrCols; colIdx++)
            {
                int currentValueRow = int.Parse(inputStr[rowIdx][colIdx].ToString());
                _risksTask1[rowIdx, colIdx] = currentValueRow;
                for (int dimRows = 0; dimRows < 5; dimRows++)
                {
                    if (currentValueRow > 9)
                    {
                        currentValueRow = 1;
                    }

                    int currentValueCol = currentValueRow;
                    for (int dimCols = 0; dimCols < 5; dimCols++)
                    {
                        if (currentValueCol > 9)
                        {
                            currentValueCol = 1;
                        }

                        _risksTask2[rowIdx + (nbrRows * dimRows), colIdx + (nbrCols * dimCols)] = currentValueCol;
                        currentValueCol++;
                    }
                    currentValueRow++;
                }
            }
        }
    }

    public override ValueTask<string> Solve_1()
    {
        (int, int) endingPos = (_risksTask1.GetLength(0) - 1, _risksTask1.GetLength(1) - 1);
        Dictionary<(int row, int col), long> shortestPaths2 = CalcLowestRiskPath((0, 0), _risksTask1);

        return new ValueTask<string>($"{shortestPaths2[endingPos]}");
    }

    public override ValueTask<string> Solve_2()
    {
        (int, int) endingPos = (_risksTask2.GetLength(0) - 1, _risksTask2.GetLength(1) - 1);
        Dictionary<(int row, int col), long> shortestPaths = CalcLowestRiskPath((0, 0), _risksTask2);

        return new ValueTask<string>($"{shortestPaths[endingPos]}");
    }

    private static IEnumerable<(int row, int col)> AdjacentElements((int row, int col) currentPos, int nbrRows, int nbrCols)
    {
        if (currentPos.row > 0) yield return (currentPos.row - 1, currentPos.col);
        if (currentPos.row < nbrRows - 1) yield return (currentPos.row + 1, currentPos.col);
        if (currentPos.col > 0) yield return (currentPos.row, currentPos.col - 1);
        if (currentPos.col < nbrCols - 1) yield return (currentPos.row, currentPos.col + 1);
    }

    private static Dictionary<(int row, int col), long> CalcLowestRiskPath((int row, int col) startingPos, int[,] risks)
    {
        Queue<(int row, int col)> queue = new Queue<(int row, int col)>();
        // all risks starting from startingPos
        Dictionary<(int row, int col), long> calculatedRisks = new Dictionary<(int row, int col), long> { { startingPos, 0 } };
        queue.Enqueue(startingPos);
        (int, int) endingPos = (risks.GetLength(0) - 1, risks.GetLength(1) - 1);

        while (queue.TryDequeue(out (int row, int col) currentPos))
        {
            foreach ((int row, int col) adjacent in
                     AdjacentElements(currentPos, risks.GetLength(0), risks.GetLength(1)).ToList())
            {
                long newRisk = calculatedRisks[currentPos] + risks[adjacent.row, adjacent.col];
                if (!calculatedRisks.ContainsKey(adjacent) || calculatedRisks[adjacent] > newRisk)
                {
                    calculatedRisks[adjacent] = newRisk;
                }
                else
                {
                    continue;
                }

                if (!queue.Contains(adjacent) && adjacent != endingPos)
                {
                    queue.Enqueue(adjacent);
                }
            }
        }

        return calculatedRisks;
    }

}