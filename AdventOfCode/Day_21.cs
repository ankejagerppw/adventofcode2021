using System.Text.RegularExpressions;
using AoCHelper;

namespace AdventOfCode;

public class Day_21 : BaseDay
{
    private const int QuantumMinValueForWinner = 21;
    private const int MaxValueOnBoard = 10;

    private Dictionary<int, int> _playerPositions;

    private class Game
    {
        private int _playerIdToThrow;
        public Dictionary<int, int> Positions;
        public Dictionary<int, int> Scores;
        public bool GameFinished;
        public int? PlayerIdToWin;
        public long GameCount;

        public Game()
        {
            Positions = new Dictionary<int, int>();
            Scores = new Dictionary<int, int>();
            _playerIdToThrow = 1;
            GameFinished = false;
            GameCount = 1L;
        }

        private void SwitchPlayer()
        {
            _playerIdToThrow = _playerIdToThrow == 1 ? 2 : 1;
        }

        public void ThrowDie(int dieValue)
        {
            Positions[_playerIdToThrow] = (Positions[_playerIdToThrow] + dieValue) % MaxValueOnBoard;
            if (Positions[_playerIdToThrow] == 0)
            {
                Positions[_playerIdToThrow] = MaxValueOnBoard;
            }

            Scores[_playerIdToThrow] += Positions[_playerIdToThrow];
            if (Scores[_playerIdToThrow] >= QuantumMinValueForWinner)
            {
                GameFinished = true;
                PlayerIdToWin = _playerIdToThrow;
            }
            else
            {
                SwitchPlayer();
            }
        }

        public static Game Copy(Game game)
        {
            return new Game
            {
                Positions = new Dictionary<int, int>(game.Positions),
                Scores = new Dictionary<int, int>(game.Scores),
                _playerIdToThrow = game._playerIdToThrow,
                GameFinished = game.GameFinished,
                PlayerIdToWin = game.PlayerIdToWin,
                GameCount = game.GameCount
            };
        }
    }

    public Day_21()
    {
        Regex regex = new Regex("Player (?<playerNumber>\\d+) starting position: (?<startingPosition>\\d{1,2})");

        _playerPositions = File
            .ReadAllLines(InputFilePath)
            .Select(s => regex.Match(s))
            .Where(m => m.Success)
            .ToDictionary(m => int.Parse(m.Groups["playerNumber"].Value), m => int.Parse(m.Groups["startingPosition"].Value));
    }

    public override ValueTask<string> Solve_1()
    {
        int lastValueOfDie = 0;
        int timesThrown = 0;
        Dictionary<int, int> playerPositions = new Dictionary<int, int>(_playerPositions);
        Dictionary<int, int> playerScores = playerPositions.ToDictionary(p => p.Key, p => 0);

        void ThrowDieForPlayer(int playerId)
        {
            int NextValueOfDie()
            {
                lastValueOfDie++;
                if (lastValueOfDie > 100) lastValueOfDie = 1;
                return lastValueOfDie;
            }

            int[] thrownValues = { NextValueOfDie(), NextValueOfDie(), NextValueOfDie() };
            playerPositions[playerId] = (playerPositions[playerId] + thrownValues.Sum()) % 10;
            if (playerPositions[playerId] == 0)
            {
                playerPositions[playerId] = 10;
            }

            playerScores[playerId] += playerPositions[playerId];
            timesThrown += 3;

            // Console.WriteLine($"Player {playerId} rolls {string.Join("+", thrownValues)} and moves to space {playerPositions[playerId]} for a total score of {playerScores[playerId]}.");
        }

        while (playerScores.Values.All(v => v < 1000))
        {
            foreach (int playerId in playerPositions.Keys.OrderBy(p => p))
            {
                ThrowDieForPlayer(playerId);
                if (playerScores[playerId] >= 1000) break;
            }
        }

        return new ValueTask<string>($"Result: {playerScores.Single(p => p.Value < 1000).Value * timesThrown}");
    }

    public override ValueTask<string> Solve_2()
    {
        Dictionary<int, long> playerWins = _playerPositions.ToDictionary(p => p.Key, p => 0L);

        Game game = new Game
        {
            Positions = new Dictionary<int, int>(_playerPositions),
            Scores = _playerPositions.ToDictionary(p => p.Key, p => 0)
        };

        int[] possibleDieValues = { 1, 2, 3 };
        List<int> allPossibleSums = (
            from throw1 in possibleDieValues
            from throw2 in possibleDieValues
            from throw3 in possibleDieValues
            select throw1 + throw2 + throw3).ToList();
        Dictionary<int,int> allPossibleSumsCount =
            allPossibleSums
            .Distinct()
            .ToDictionary(i => i, i => allPossibleSums.Count(aps => aps == i));

        Queue<Game> queue = new Queue<Game>();
        queue.Enqueue(game);

        while (queue.TryDequeue(out Game currentGame))
        {
            foreach (int throwSum in allPossibleSumsCount.Keys)
            {
                Game g = Game.Copy(currentGame);
                g.GameCount *= allPossibleSumsCount[throwSum];
                g.ThrowDie(throwSum);
                if (g.GameFinished)
                {
                    playerWins[g.PlayerIdToWin.Value] += g.GameCount;
                }
                else
                {
                    queue.Enqueue(g);
                }
            }
        }

        return new ValueTask<string>($"Max universes to win: {playerWins.Values.Max()}");
    }
}