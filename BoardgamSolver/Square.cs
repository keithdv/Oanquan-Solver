using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardgamSolver
{


    public struct Square
    {

        public Square(byte value)
        {
            Number = value;
            IsTrail = false;
            IsMatched = false;
            IsNew = false;
            IsEmpty = value == 0;
        }

        public Square(Square old)
        {
            Number = old.Number;
            IsTrail = old.IsTrail;
            IsMatched = old.IsMatched;
            IsNew = false;
            IsEmpty = old.IsEmpty;
        }

        public byte Number { get; }
        public bool IsEmpty { get; set; }

        public bool IsTrail { get; private set; }

        public bool IsMatched { get; private set; }

        public bool IsNew { get; private set; }

        private static Random rnd { get; } = new Random((int)(DateTime.Now.Ticks % 999999999));
        public static Square Random(int max = 9)
        {
            return new Square((byte)(rnd.Next(max) + 1)) { IsNew = true };
        }

        public static Square EmptySquare()
        {
            var s = new Square(0) { IsEmpty = true };
            return s;
        }

        public static Square BlockSquare()
        {
            return new Square(0) { IsEmpty = false };
        }

        public Square Move()
        {
            if (Number < 2) { throw new Exception("Token cannot move and be less then 2"); }
            return new Square((byte)(Number - 1)) { IsTrail = true };
        }

        public static Square TrailToken()
        {
            return new Square(1)
            {
                IsTrail = true
            };
        }

        public static Square MatchToken(Square s)
        {
            return new Square(s.Number)
            {
                IsMatched = true,
                IsTrail = s.IsTrail
            };

        }

        public override string ToString()
        {
            return Number == 0 ? " " : Number.ToString();
        }
    }

    public class Board
    {

        public Board(Square[,] squares)
        {
            Squares = squares;
        }

        public static Board NewBoard()
        {
            var board = new Board(new Square[6, 6]);

            for (var x = 0; x < 6; x++)
            {
                for (var y = 0; y < 6; y++)
                {
                    board[x, y] = Square.EmptySquare();
                }
            }

            board.FillRandomSpots(new[] { Square.Random(4), Square.Random(4), Square.Random(4), Square.Random(4), Square.Random(4), Square.Random(4), Square.Random(4) });

            return board;
        }

        public Square this[int x, int y]
        {
            get { return Squares[x, y]; }
            set { Squares[x, y] = value; }
        }

        public Square[,] Squares { get; private set; }

        public byte LargeSum
        {
            get
            {
                byte c = 0;
                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsEmpty && !s.IsMatched && s.Number >= 8) { c++; }
                    }
                }
                return c;
            }
        }

        public byte ConnectedEmpties
        {
            get
            {
                byte c = 0;
                for (byte y = 1; y < 5; y++)
                {
                    for (byte x = 1; x < 5; x++)
                    {
                        // Up
                        if (y > 0 && Squares[x, y - 1].IsEmpty) { c++; }

                        // Left
                        if (x > 0 && Squares[x - 1, y].IsEmpty) { c++; }

                        // Down
                        if (y < 5 && Squares[x, y + 1].IsEmpty) { c++; }

                        // Right
                        if (x < 5 && Squares[x + 1, y].IsEmpty) { c++; }

                    }
                }
                return c;
            }
        }

        public byte Count
        {
            get
            {
                byte c = 0;
                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsEmpty && !s.IsMatched && s.Number != 0) { c++; }
                    }
                }
                return c;
            }
        }

        public byte WeightedCount
        {
            get
            {
                byte c = 0;
                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsEmpty && !s.IsMatched && s.Number != 1 && s.Number != 0) { c++; }
                    }
                }
                return c;
            }
        }

        public int Points
        {
            get
            {
                var p = new List<byte>();

                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (s.IsMatched) { p.Add(s.Number); }
                    }
                }

                var points = 0;

                foreach (var token in p.Distinct())
                {
                    points += token * token * p.Where(t => t == token).Count();
                }

                return points;
            }
        }

        public int WeightedSum
        {
            get
            {
                int c = 0;
                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsMatched)
                        {
                            c += (s.Number * 2);
                        }
                    }
                }
                return c;
            }
        }

        public int Sum
        {
            get
            {
                int c = 0;
                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsMatched)
                        {
                            c += s.Number;
                        }
                    }
                }
                return c;
            }
        }

        public int MatchedSum
        {
            get
            {
                int c = 0;
                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (s.IsMatched)
                        {
                            c += s.Number;
                        }
                    }
                }
                return c;
            }
        }

        public int CenterSum
        {
            get
            {
                int c = 0;
                for (byte y = 1; y < 5; y++)
                {
                    for (byte x = 1; x < 5; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsEmpty && !s.IsMatched)
                        {
                            c += (s.Number * 2);
                        }
                    }
                }
                return c;
            }
        }

        public int NineCount
        {
            get
            {
                int c = 0;
                for (byte y = 0; y < 6; y++)
                {
                    for (byte x = 0; x < 6; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsMatched && s.Number == 9)
                        {
                            c += 1;
                        }
                    }
                }
                return c;
            }
        }

        public int CenterCount
        {
            get
            {
                int c = 0;
                for (byte y = 1; y < 5; y++)
                {
                    for (byte x = 1; x < 5; x++)
                    {
                        var s = Squares[x, y];
                        if (!s.IsEmpty && !s.IsMatched && s.Number != 0)
                        {
                            c += 1;
                        }
                    }
                }
                return c;
            }
        }

        public Board Copy(bool includeMatched = true)
        {
            var newBoard = new Board(new Square[6, 6]);

            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    if (includeMatched || !this[x, y].IsMatched)
                    {
                        newBoard[x, y] = new Square(this[x, y]);
                    }
                    else
                    {
                        newBoard[x, y] = Square.EmptySquare();
                    }
                }
            }

            return newBoard;

        }

        private static Random rnd { get; } = new Random((int)(DateTime.Now.Ticks % 999999999));

        public List<(int, int)> RandomSpots(int number = 3)
        {


            var empties = new List<(int, int)>();
            for (var x = 0; x < 6; x++)
            {
                for (var y = 0; y < 6; y++)
                {
                    if (Squares[x, y].IsEmpty)
                    {
                        empties.Add((x, y));
                    }
                }
            }

            var spots = new List<(int, int)>();

            if (empties.Count == 0) { return spots; }

            for (var i = 0; i < number; i++)
            {
                var empty = empties[(byte)rnd.Next(empties.Count)];
                spots.Add(empty);
                empties.Remove(empty);
                if (empties.Count == 0) { return spots; }
            }

            return spots;
        }

        public Board FillRandomSpots(Square[] newSquares, IReadOnlyList<(int, int)> e = null)
        {
            var empties = e?.ToList() ?? RandomSpots(newSquares.Length);

            if (empties.Count == 0) { return this; }

            foreach (var s in newSquares)
            {
                var empty = empties[0];
                if (Squares[empty.Item1, empty.Item2].IsEmpty)
                {
                    Squares[empty.Item1, empty.Item2] = s;
                }
                else
                {
                    empties.AddRange(RandomSpots(1));
                }
                empties.RemoveAt(0);
                if (empties.Count == 0) { return this; }
            }

            return this;

        }

        public void CheckForMatches()
        {

            // Find all tokens that can disappear
            var newMatch = true;

            while (newMatch)
            {
                newMatch = false;
                for (byte x = 0; x < 6; x++)
                {
                    for (byte y = 0; y < 6; y++)
                    {
                        var curSquare = Squares[x, y];
                        if (!curSquare.IsEmpty && !curSquare.IsMatched)
                        {
                            // Up, Left, Right, Down

                            // Up
                            if (y > 0 && (Squares[x, y - 1].IsTrail || Squares[x, y - 1].IsMatched) && Squares[x, y - 1].Number == curSquare.Number)
                            {
                                Squares[x, y] = Square.MatchToken(curSquare);
                                newMatch = true;
                            }

                            // Left
                            if (x > 0 && (Squares[x - 1, y].IsTrail || Squares[x - 1, y].IsMatched) && Squares[x - 1, y].Number == curSquare.Number)
                            {
                                Squares[x, y] = Square.MatchToken(curSquare);
                                newMatch = true;
                            }

                            // Down
                            if (y < 5 && (Squares[x, y + 1].IsTrail || Squares[x, y + 1].IsMatched) && Squares[x, y + 1].Number == curSquare.Number)
                            {
                                Squares[x, y] = Square.MatchToken(curSquare);
                                newMatch = true;
                            }

                            // Right
                            if (x < 5 && (Squares[x + 1, y].IsTrail || Squares[x + 1, y].IsMatched) && Squares[x + 1, y].Number == curSquare.Number)
                            {
                                Squares[x, y] = Square.MatchToken(curSquare);
                                newMatch = true;
                            }

                        }
                    }
                }
            }

        }

    }

    public static class Turn
    {

        public static IEnumerable<Move> Moves(Board board, Square[] blockSquares, List<(int, int)> futureSpots)
        {

            ConcurrentBag<List<Move>> allMoves = new ConcurrentBag<List<Move>>();

            Parallel.For(0, 36, i =>
            {

                // Find all possible moves
                var moves = new List<Move>();

                int x = i % 6;
                int y = i / 6;

                if (!board[x, y].IsEmpty)
                {
                    Move(board, x, y, null, moves);
                }

                Parallel.ForEach(moves, m =>
                {
                    var secondMoves = new List<Move>();
                    m.Board.CheckForMatches();
                    var b = m.Board.Copy(false).FillRandomSpots(blockSquares, futureSpots);
                    Moves(b, m, secondMoves);
                    secondMoves.ForEach(m => m.Board.CheckForMatches());
                    allMoves.Add(secondMoves);
                });

            });

            return allMoves.SelectMany(x => x);
        }

        public static void Moves(Board board, Move parentMove, List<Move> moves)
        {
            for (int y = 0; y < 6; y += 1)
            {
                for (int x = 0; x < 6; x += 1)
                {
                    if (!board[x, y].IsEmpty)
                    {
                        Move(board, x, y, parentMove, moves);
                    }
                }
            }
        }

        public static void Move(Board board, int x, int y, Move parentMove, List<Move> moves)
        {

            var square = board[x, y];

            if (square.Number > 1)
            {
                // Up, Left, Right, Down

                // Up
                if (y > 0 && board[x, y - 1].IsEmpty)
                {
                    var b = board.Copy();
                    b[x, y] = Square.TrailToken();
                    b[x, y - 1] = board[x, y].Move();
                    moves.Add(new Move(parentMove, b));
                    Move(b, x, y - 1, parentMove, moves);
                }

                // Left
                if (x > 0 && board[x - 1, y].IsEmpty)
                {
                    var b = board.Copy();
                    b[x, y] = Square.TrailToken();
                    b[x - 1, y] = board[x, y].Move();
                    moves.Add(new Move(parentMove, b));
                    Move(b, x - 1, y, parentMove, moves);
                }

                // Down
                if (y < 5 && board[x, y + 1].IsEmpty)
                {
                    var b = board.Copy();
                    b[x, y] = Square.TrailToken();
                    b[x, y + 1] = board[x, y].Move();
                    moves.Add(new Move(parentMove, b));
                    Move(b, x, y + 1, parentMove, moves);
                }

                // Right
                if (x < 5 && board[x + 1, y].IsEmpty)
                {
                    var b = board.Copy();
                    b[x, y] = Square.TrailToken();
                    b[x + 1, y] = board[x, y].Move();
                    moves.Add(new Move(parentMove, b));
                    Move(b, x + 1, y, parentMove, moves);
                }

            }
        }

    }

    public class Move
    {
        public Move(Move parentMove, Board board)
        {
            Board = board;
            ParentMove = parentMove;
        }

        public Board Board { get; }
        public Move ParentMove { get; }

    }


}
