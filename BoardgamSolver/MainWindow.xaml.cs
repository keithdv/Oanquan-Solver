using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BoardgamSolver
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);


        public bool ScreenEnabled
        {
            get { return (bool)GetValue(ScreenEnabledProperty); }
            set { SetValue(ScreenEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScreenEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScreenEnabledProperty =
            DependencyProperty.Register("ScreenEnabled", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));






        public Square[] CurrentBoard
        {
            get { return (Square[])GetValue(CurrentBoardProperty); }
            set { SetValue(CurrentBoardProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentBoard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentBoardProperty =
            DependencyProperty.Register("CurrentBoard", typeof(Square[]), typeof(MainWindow));


        public byte[] NextNumbers
        {
            get { return (byte[])GetValue(NextNumbersProperty); }
            set { SetValue(NextNumbersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NextNumbers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NextNumbersProperty =
            DependencyProperty.Register("NextNumbers", typeof(byte[]), typeof(MainWindow));




        public Square[] MoveOneBoard
        {
            get { return (Square[])GetValue(MoveOneBoardProperty); }
            set { SetValue(MoveOneBoardProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveOneBoard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveOneBoardProperty =
            DependencyProperty.Register("MoveOneBoard", typeof(Square[]), typeof(MainWindow));




        public Square[] MoveTwoBoard
        {
            get { return (Square[])GetValue(MoveTwoBoardProperty); }
            set { SetValue(MoveTwoBoardProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveTwoBoard.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveTwoBoardProperty =
            DependencyProperty.Register("MoveTwoBoard", typeof(Square[]), typeof(MainWindow));



        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ScreenEnabled = true;
            FindIphoneWindow();


        }

        protected override async void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (ScreenEnabled)
            {
                if (e.Key == Key.M)
                {
                    await NextMove();
                }
            }
        }

        private void FindIphoneWindow()
        {

            string className = "TPlayerForm";

            string windowName = @"Keith’s iPhone";

            IntPtr hwnd = FindWindow(className, windowName);

            GetWindowRect(hwnd, out iphoneScreen);


        }

        RECT iphoneScreen;

        int Points = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {



            //for (int x = 0; x < 5; x++)
            //{
            //    for (int y = 0; y < 5; y++)
            //    {
            //        var pos = boardPoints[x, y];
            //        var match = board.RecognitionResult.Lines.Where(l => l.BoundingBox[0] < pos.X && l.BoundingBox[1] < pos.Y && l.BoundingBox[4] > pos.X && l.BoundingBox[5] > pos.Y);

            //        if (match.Any())
            //        {
            //            foreach (var m in match)
            //            {
            //                Debug.WriteLine($"Matched [{x}, {y}] to {m.Text}");
            //            }
            //        }
            //    }
            //}

        }

        public async Task NextMove()
        {
            Directory.GetFiles(@".\", @"*.png").ToList().ForEach((f) =>
            {
                File.Delete(f);
            });

            var check = new List<(int, int)>();

            var movedBoard = MoveOneBoard ?? CurrentBoard;

            for (int y = 0; y < 6; y += 1)
            {
                for (int x = 0; x < 6; x += 1)
                {
                    var num = movedBoard[x + (y * 6)];
                    if (num.IsEmpty || num.IsMatched || num.IsTrail)
                    {
                        check.Add((x, y));
                    }
                }
            }

            MoveOneBoard = null;
            MoveTwoBoard = null;

            var nums = CurrentBoard.Select(n => n.Number).ToArray();

            var nextNums = await CaptureScreen.CaptureNextNumbers(iphoneScreen);
            await CaptureScreen.Capture(iphoneScreen, nums, check);

            var squares = new Square[6, 6];
            var curBoard = new Square[36];
            var futureSpots = new List<(int, int)>();

            for (int y = 0; y < 6; y += 1)
            {
                for (int x = 0; x < 6; x += 1)
                {
                    var num = nums[x + (y * 6)];
                    if (num > 0 && num < 10)
                    {
                        squares[x, y] = new Square(num) { IsEmpty = false };
                    }
                    else
                    {
                        squares[x, y] = new Square(0) { IsEmpty = true };
                    }

                    curBoard[x + (y * 6)] = squares[x, y];

                    if (num == 10)
                    {
                        futureSpots.Add((x, y));
                    }
                }
            }

            CurrentBoard = curBoard;

            await Task.Yield();

            var board = new Board(squares);

            var blockSquares = new Square[] { Square.BlockSquare(), Square.BlockSquare(), Square.BlockSquare() };
            Square[] newSquares = new Square[] { new Square(nextNums[0]), new Square(nextNums[1]), new Square(nextNums[2]) };

            NextNumbers = nextNums;

            var newDup = newSquares.Select(x => x.Number).Distinct().Count();
            if (newDup == 1 && nextNums[0] != 0)
            {
                blockSquares = newSquares;
            }


            var nextMoves = Turn.Moves(board, blockSquares, futureSpots);

            // BEST!!!
            var bestMoves = nextMoves
                    .OrderBy(m => m.Board.Count)
                    .ThenBy(m => m.ParentMove.Board.Count)
                    .ThenByDescending(m => m.ParentMove.Board.Points)
                    .ThenByDescending(m => m.Board.Points)
                    .ThenBy(m => m.Board.CenterCount)
                    .ThenBy(m => m.Board.Sum)
                    .FirstOrDefault();

            if (bestMoves != null)
            {
                var bestMove = bestMoves.ParentMove;

                var newNumbers = new Square[36];
                var secondMove = new Square[36];

                for (int y = 0; y < 6; y += 1)
                {
                    for (int x = 0; x < 6; x += 1)
                    {
                        var s = bestMove.Board.Squares[x, y];
                        newNumbers[x + (y * 6)] = s;
                        secondMove[x + (y * 6)] = bestMoves.Board[x, y];
                    }
                }

                MoveOneBoard = newNumbers;
                MoveTwoBoard = secondMove;

            }
            else
            {
                MoveOneBoard = null;
                MoveTwoBoard = null;
            }

        }


        private async void IPhoneScreen_Click(object sender, RoutedEventArgs e)
        {
            ScreenEnabled = false;

            FindIphoneWindow();

            //CaptureScreen.FoundNumbers.Clear();

            var nextNums = await CaptureScreen.CaptureNextNumbers(iphoneScreen);

            NextNumbers = nextNums;

            var nums = await CaptureScreen.Capture(iphoneScreen);

            var newNumbers = new Square[36];

            for (var i = 0; i < 36; i++)
            {
                newNumbers[i] = new Square(nums[i]);
            }

            CurrentBoard = newNumbers;

            ScreenEnabled = true;
        }

        private async void Move_Click(object sender, RoutedEventArgs e)
        {
            ScreenEnabled = false;
            await NextMove();
            ScreenEnabled = true;
        }

        bool stop = false;
        private async void ContMove_Click(object sender, RoutedEventArgs e)
        {
            stop = false;
            ScreenEnabled = false;

            while (!stop)
            {
                await NextMove();

                int hash = 0;
                int previousHash = 0;

                do
                {
                    using (var screenStream = new MemoryStream())
                    {
                        using (var screenBmp = new Bitmap(300, 60, PixelFormat.Format32bppArgb))
                        {
                            using (var bmpGraphics = Graphics.FromImage(screenBmp))
                            {
                                bmpGraphics.CopyFromScreen(iphoneScreen.Left + 300, iphoneScreen.Top + 400, 0, 0, screenBmp.Size);
                            }

                            hash = CaptureScreen.GetHash(screenBmp);

                            if (previousHash == 0) { previousHash = hash; }

                            if (Math.Abs(hash - previousHash) < 3)
                            {

                                await Task.Delay(100);
                            }
                            else
                            {
                                await Task.Delay(1000);
                                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                                {
                                    bmpGraphics.CopyFromScreen(iphoneScreen.Left + 300, iphoneScreen.Top + 400, 0, 0, screenBmp.Size);
                                }

                                hash = CaptureScreen.GetHash(screenBmp);
                            }

                        }
                    }

                } while (Math.Abs(hash - previousHash) < 3 && !stop);

                previousHash = hash;
            }
            ScreenEnabled = true;
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            stop = true;
        }
    }
}
