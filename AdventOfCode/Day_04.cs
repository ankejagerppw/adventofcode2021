using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AoCHelper;

namespace AdventOfCode
{
    public class Day_04 : BaseDay
    {
        private string[] _input;
        private int[] _numbersDrawn;
        private List<BingoCard> _bingoCards;

        private class BingoCardItem
        {
            public int Number { get; }
            public bool Marked { get; set; }

            public BingoCardItem(string number)
            {
                Number = int.Parse(number);
                Marked = false;
            }
        }

        private class BingoCard
        {
            private BingoCardItem[][] Items;

            public bool Bingo { get; private set; }

            public BingoCard(BingoCardItem[][] numbers)
            {
                Items = numbers;
                Bingo = false;
            }

            /// <summary>
            /// Mark a number on the card as drawn and return true if it's bingo.
            /// </summary>
            /// <param name="nbr">Number that is drawn</param>
            /// <returns>True if horizontally/vertically is bingo, false otherwise.</returns>
            public bool MarkDrawn(int nbr)
            {
                foreach (BingoCardItem[] bingoCardRow in Items)
                {
                    foreach (BingoCardItem bingoCardItem in bingoCardRow)
                    {
                        if (bingoCardItem.Number == nbr)
                        {
                            bingoCardItem.Marked = true;
                        }
                    }
                }

                // horizontol bingo
                Bingo = Items.Any(row => row.All(item => item.Marked));

                if (Bingo)
                {
                    return true;
                }

                // vertical bingo
                for (int idx = 0; idx < Items[0].Length && !Bingo; idx++)
                {
                    Bingo = Items
                        .Select(row => row[idx])
                        .All(item => item.Marked);
                }

                return Bingo;
            }

            public int SumUnmarkedNumbers()
            {
                return Items
                    .SelectMany(row => row.Where(r => !r.Marked).Select(item => item.Number))
                    .Sum();
            }
        }

        public Day_04()
        {
            _input = File
                .ReadAllLines(InputFilePath)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            _numbersDrawn = _input[0].Split(",").Select(int.Parse).ToArray();
            _bingoCards = new List<BingoCard>();

            for (int idx = 1; idx < _input.Length - 5; idx += 5)
            {
                BingoCardItem[][] nbrs = {
                    _input[idx].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => new BingoCardItem(s)).ToArray(),
                    _input[idx + 1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => new BingoCardItem(s)).ToArray(),
                    _input[idx + 2].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => new BingoCardItem(s)).ToArray(),
                    _input[idx + 3].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => new BingoCardItem(s)).ToArray(),
                    _input[idx + 4].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => new BingoCardItem(s)).ToArray()
                };
                _bingoCards.Add(new BingoCard(nbrs));
            }
        }

        public override ValueTask<string> Solve_1()
        {
            bool bingo = false;
            int result = 0;
            for (int idx = 0; idx < _numbersDrawn.Length && !bingo; idx++)
            {
                int nbrDrawn = _numbersDrawn[idx];
                for (int bingoCardIdx = 0; bingoCardIdx < _bingoCards.Count && !bingo; bingoCardIdx++)
                {
                    BingoCard bingoCard = _bingoCards[bingoCardIdx];
                    bingo = bingoCard.MarkDrawn(nbrDrawn);
                    if (bingo)
                    {
                        result = bingoCard.SumUnmarkedNumbers() * nbrDrawn;
                    }
                }
            }

            return new ValueTask<string>($"{result}");
        }

        public override ValueTask<string> Solve_2()
        {
            int result = 0;
            int totalNbrCards = _bingoCards.Count;
            int nbrMarkedAsBingo = 0;
            List<BingoCard> bingoCardsToPlayWith = new List<BingoCard>(_bingoCards);
            for (int idx = 0; idx < _numbersDrawn.Length && nbrMarkedAsBingo != totalNbrCards; idx++)
            {
                int nbrDrawn = _numbersDrawn[idx];

                foreach (BingoCard card in bingoCardsToPlayWith)
                {
                    if (card.MarkDrawn(nbrDrawn))
                    {
                        nbrMarkedAsBingo++;
                        if (nbrMarkedAsBingo == totalNbrCards)
                        {
                            result = card.SumUnmarkedNumbers() * nbrDrawn;
                        }
                    }
                }

                bingoCardsToPlayWith = bingoCardsToPlayWith.Where(c => !c.Bingo).ToList();
            }

            return new ValueTask<string>($"{result}");
        }
    }
}