using Boardgame.Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BoardgameDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            var display = new List<string>();

            while (true)
            {
                var board = Board.NewBoard();

                //board[0, 0] = new Square(5);
                //board[1, 0] = Square.EmptySquare();
                //board[2, 0] = Square.EmptySquare();
                //board[3, 0] = new Square(2);
                //board[4, 0] = new Square(9);
                //board[5, 0] = new Square(4);

                //board[0, 1] = Square.EmptySquare();
                //board[1, 1] = Square.EmptySquare();
                //board[2, 1] = Square.EmptySquare();
                //board[3, 1] = new Square(5);
                //board[4, 1] = new Square(5);
                //board[5, 1] = new Square(4);

                //board[0, 2] = Square.EmptySquare();
                //board[1, 2] = Square.EmptySquare();
                //board[2, 2] = new Square(6);
                //board[3, 2] = Square.EmptySquare();
                //board[4, 2] = new Square(5);
                //board[5, 2] = new Square(1);

                //board[0, 3] = Square.EmptySquare();
                //board[1, 3] = Square.EmptySquare();
                //board[2, 3] = new Square(5);
                //board[3, 3] = new Square(2);
                //board[4, 3] = new Square(8);
                //board[5, 3] = new Square(2);

                //board[0, 4] = Square.EmptySquare();
                //board[1, 4] = Square.EmptySquare();
                //board[2, 4] = new Square(5);
                //board[3, 4] = new Square(4);
                //board[4, 4] = Square.EmptySquare();
                //board[5, 4] = new Square(3);

                //board[0, 5] = new Square(2);
                //board[1, 5] = new Square(4);
                //board[2, 5] = new Square(9);
                //board[3, 5] = new Square(4);
                //board[4, 5] = new Square(6);
                //board[5, 5] = new Square(2);

                //WriteBoard(new List<Board>() { board });



                Turn turn = new Turn(null, board);

                var p = 0;

                while (turn != null)
                {

                    var futureSpots = turn.Board.RandomSpots();

                    var nextMoves = new List<Move>(turn.Moves());
                    var blockSquares = new Square[] { Square.BlockSquare(), Square.BlockSquare(), Square.BlockSquare() };
                    Square[] newSquares = new Square[] { Square.Random(), Square.Random(), Square.Random() };

                    var newDup = newSquares.Select(x => x.Number).Distinct().Count();
                    if (newDup == 1)
                    {
                        blockSquares = newSquares;
                    }

                    var futureMoves = new ConcurrentBag<(Move, Move)>();

                    Parallel.ForEach(nextMoves, m =>
                    {
                        var b = m.Board.Copy(false).FillRandomSpots(blockSquares, futureSpots);
                        foreach (var nm in (new Turn(turn, b).Moves())) { futureMoves.Add((m, nm)); }
                    });

                    // Hit 60k
                    //          var bestMoves = futureMoves.OrderBy(m => m.Item2.Board.Count)
                    //.ThenBy(m => m.Item2.Board.Count)

                    //var stillBestMove = futureMoves.OrderBy(m => m.Item2.Board.WeightedCount).ThenBy(m => m.Item2.Board.Sum).ThenBy(m => m.Item2.Board.CenterSum).FirstOrDefault().Item1;

                    //var simpleBestMove = futureMoves.OrderBy(m => m.Item2.Board.Count).OrderBy(m => m.Item1.Board.Count).FirstOrDefault().Item1;

                    //var bestMoves = futureMoves.OrderBy(m=>m.Item2.Board.Count)
                    //                    .ThenBy(m => m.Item2.Board.WeightedSum)
                    //                    .ThenBy(m => m.Item2.Board.Count)
                    //                    .FirstOrDefault();

                    //var bestMoves = futureMoves.OrderBy(m => m.Item2.Board.Count)
                    //                            .ThenBy(m => m.Item2.Board.Sum)
                    //                            .ThenBy(m => m.Item2.Board.CenterCount).FirstOrDefault();

                    //var bestMoves = futureMoves.OrderBy(m => m.Item2.Board.NineCount)
                    //    .ThenBy(m => m.Item2.Board.Count)
                    //    .ThenBy(m => m.Item2.Board.CenterCount)
                    //    .ThenBy(m => m.Item1.Board.Count)
                    //    .ThenBy(m => m.Item2.Board.Sum)
                    //    .FirstOrDefault();

                    //var bestMoves = futureMoves.OrderBy(m => m.Item2.Board.Count)
                    //        //.ThenBy(m => m.Item2.Board.NineCount)
                    //        .ThenBy(m => m.Item1.Board.Count)
                    //        //.ThenBy(m=>m.Item2.Board.cente)
                    //        .ThenBy(m => m.Item2.Board.Sum)
                    //        .ThenBy(m => m.Item1.Board.Sum)
                    //        .FirstOrDefault();

                    // BEST!!!
                    var bestMoves = futureMoves
                            .OrderBy(m => m.Item2.Board.WeightedCount)
                            .ThenBy(m => m.Item2.Board.WeightedSum)
                            .ThenBy(m => m.Item2.Board.CenterSum)
                            .ThenBy(m => m.Item1.Board.WeightedCount)
                            .ThenBy(m => m.Item1.Board.WeightedSum)
                            .ThenBy(m => m.Item1.Board.CenterSum)
                            .FirstOrDefault();

                    //.OrderBy(m => m.Item2.Board.Count)
                    //    //.ThenBy(m => m.Item1.Board.Count)
                    //    //.ThenBy(m=>m.Item2.Board.cente)
                    //    .ThenBy(m => m.Item2.Board.Sum)
                    //    //.ThenBy(m => m.Item1.Board.CenterSum)
                    //    .FirstOrDefault();

                    var bestMove = bestMoves.Item1;

                    if (bestMove == null)
                    {
                        Console.WriteLine($"Depth {turn.Depth} Points {p}");
                        WriteBoard(new Board[] { turn.Board });

                        display.Add($"Depth {turn.Depth} Points {p}");
                        break;
                    }

                    //if (turn.Board.Count > 12 && ((bestMoves.Item1.Board.Count + 1) >= turn.Board.Count))
                    //{
                    //    WriteBoard(new Board[] { turn.Board, bestMoves.Item1.Board, bestMoves.Item2.Board });
                    //    Console.WriteLine("Trouble!");
                    //    //Console.ReadLine();
                    //}



                    p += bestMove.Board.Points;

                    //WriteBoard(new Board[] { turn.Board.FillRandomSpots(blockSquares, futureSpots), bestMove.Board, bestMove.Board.Copy(false) });

                    //Console.WriteLine($"Depth {turn.Depth} Points {p}");
                    //WriteBoard(new Board[] { turn.Board.FillRandomSpots(blockSquares, futureSpots), bestMoves.Item1.Board, bestMoves.Item1.Board.Copy(false).FillRandomSpots(blockSquares, futureSpots), bestMoves.Item2.Board });

                    //if (turn.Depth % 5 == 0)
                    //{
                    //    Console.ReadLine();
                    //}

                    var nextBoard = bestMove.Board.Copy(false);

                    nextBoard.FillRandomSpots(newSquares, futureSpots);

                    //if (newSquares.Select(x => x.Number).Distinct().Count() == 2)
                    //{
                    //    Console.WriteLine($"All Match! {string.Join(' ', newSquares)}");
                    //    WriteBoard(new Board[] { turn.Board.FillRandomSpots(new Square[] { Square.BlockSquare(), Square.BlockSquare(), Square.BlockSquare() }, futureSpots), bestMoves.Item1.Board, bestMoves.Item2.Board });
                    //    //Console.ReadLine();
                    //}

                    //if (newDup == 1)
                    //{
                    //    WriteBoard(new Board[] { turn.Board, bestMoves.Item1.Board, bestMoves.Item2.Board, bestMoves.Item1.Board.Copy(false).FillRandomSpots(newSquares, futureSpots) });
                    //    //Console.ReadLine();
                    //}

                    turn = new Turn(turn, nextBoard);

                    //var simpleBestMove = nextMoves.OrderBy(m => m.Board.Sum).ThenBy(m => m.Board.CenterSum).FirstOrDefault();

                    ////Console.Clear();

                    //if (simpleBestMove != null)
                    //{
                    //    turn = new Turn(turn, simpleBestMove.Board);

                    //    Console.WriteLine($"Depth {turn.Depth} Points {turn.Points}");

                    //    WriteBoard(new List<Board> { turn.ParentTurn?.Board, simpleBestMove.Board, simpleBestMove.Board.Copy(false) });
                    //}
                    //else
                    //{
                    //    break;
                    //}

                    //Board.FillRandomSpots(new Square[] { Square.Random(), Square.Random(), Square.Random() });



                    if (turn.Depth % 50 == 0)
                    {
                        Console.WriteLine($"Depth {turn.Depth} Points {p} Count {turn.Board.Count}");
                    }


                }
                Console.Clear();
                display.ForEach(d => Console.WriteLine(d));

            }
        }

        private static void WriteBoard(IEnumerable<Board> boards)
        {
            foreach (var b in boards) { Console.WriteLine($"Count: {b.Count} Points: {b.Points} Sum: {b.CenterSum}"); }

            for (byte y = 0; y < 6; y++)
            {
                foreach (var b in boards)
                {
                    if (b == null) { break; }
                    Console.Write("|");
                    for (byte x = 0; x < 6; x++)
                    {
                        var t = b[x, y];
                        if (t.IsMatched)
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        else if (t.IsTrail)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        else if (t.IsNew)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        else if (!t.IsEmpty && t.Number == 0)
                        {
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        Console.Write($" {t} ");
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    Console.Write("|       |");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
