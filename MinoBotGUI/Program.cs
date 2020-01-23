using System;
using SFML.Graphics;
using SFML.Window;
using MinoTetris;
using MinoBot;
using SFML.System;
using SysCol = System.Drawing.Color;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace MinoBotGUI
{
    class Program
    {
        static RenderWindow window;
        static Tetris tetris;
        static Pathfinder pathfinder;
        static HashSet<TetriminoState> moves;
        static void Main(string[] args) {
            window = new RenderWindow(new VideoMode(300, 300), "MinoBot GUI", Styles.Close);
            window.Closed += OnClose;
            window.KeyPressed += OnKeyPressed;
            window.SetActive();
            window.SetFramerateLimit(60);
            //Thread.Sleep(5000);

            TetrisRNG tetRNG = new TetrisRNG(3);
            //while (tetRNG.GetPiece(0) != Tetrimino.T) tetRNG.NextPiece();
            tetris = new Tetris(tetRNG);
            TetrisDrawer.SetScale(new Vector2f(window.Size.X / 20, window.Size.Y / 20));
            pathfinder = new Pathfinder();
            TetrisBot bot = new TetrisBot(tetris, new Random(0));
            moves = pathfinder.FindAllMoves(tetris, 1, 1, 1);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Random rng = new Random(2);
            IEnumerator<Move> path = null;
            TetriminoState move = new TetriminoState(-1, -1, -1);
            while (window.IsOpen) {
                window.DispatchEvents();
                window.Clear();
                TetrisDrawer.DrawTetrisBoard(window, new Vector2f(0, 0), tetris, tetRNG, moves, move);
                if (stopWatch.ElapsedMilliseconds > 250) {
                    if (path != null && path.MoveNext()) {
                        Pathfinder.DoMove(tetris, path.Current);
                    } else {
                        if (move.x != -1) {
                            tetris.HardDrop();
                            bot.Update(tetris);
                        } else {
                            bot.Reset(tetris);
                        }

                        /*
                        moves = pathfinder.FindAllMoves(tetris, 1, 1, 1);
                        int target = rng.Next(moves.Count);
                        foreach (TetriminoState m in moves) {
                            move = m;
                            if (--target == 0) {
                                
                            }
                        }*/
                        using (CancellationTokenSource ts = new CancellationTokenSource()) {
                            CancellationToken ct = ts.Token;
                            Task.Run(() => {
                                int thinks = 0;
                                while (!ct.IsCancellationRequested) {
                                    bot.Think();
                                    thinks += 1;
                                }
                                Console.WriteLine(thinks + " thinks.");
                            });
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();
                            while (stopwatch.ElapsedMilliseconds < 1000) {
                                window.DispatchEvents();
                                window.Display();
                            }
                            stopWatch.Stop();
                            Console.WriteLine("Thought for " + stopwatch.ElapsedMilliseconds + "ms.");
                            ts.Cancel();
                        }
                        MinoBot.MonteCarlo.Node<TetrisState, TetriminoState> node = bot.GetMove(tetris);
                        Console.WriteLine("Selected node has:");
                        Console.WriteLine(" score: " + node.score);
                        Console.WriteLine(" simulations: " + (node.simulations - 1));

                        move = node.move;
                        moves = pathfinder.FindAllMoves(tetris, 1, 1, 1);
                        List<Move> pathList = pathfinder.GetPath(move.x, move.y, move.rot);
                        /*
                        Console.Clear();
                        //Console.WriteLine(move.x + ", " + move.y + ", " + move.rot);
                        foreach (Move mv in pathList) {
                            Console.WriteLine(Enum.GetName(mv.GetType(), mv));
                        }
                        */
                        path = pathList.GetEnumerator();
                    }
                    stopWatch.Restart();
                }
                window.Display();
            }
        }

        private static void OnKeyPressed(object sender, KeyEventArgs e) {
            /*
            switch (e.Code) {
                case Keyboard.Key.Space:
                    tetris.HardDrop();
                    moves = pathfinder.FindAllMoves(tetris, 1, 1, 1);
                    break;
                case Keyboard.Key.Left:
                    tetris.MoveLeft();
                    break;
                case Keyboard.Key.Right:
                    tetris.MoveRight();
                    break;
                case Keyboard.Key.Down:
                    tetris.SoftDrop();
                    break;
                case Keyboard.Key.Up:
                    tetris.TurnRight();
                    break;
                case Keyboard.Key.Z:
                    tetris.TurnLeft();
                    break;
                case Keyboard.Key.C:
                    tetris.Hold();
                    moves = pathfinder.FindAllMoves(tetris, 1, 1, 1);
                    break;
            }
            */
        }
        private static void OnClose(object sender, EventArgs e) {
            window.Close();
        }
    }
    class TetrisDrawer
    {
        private static RectangleShape tetrisRect;
        private static Color[] cellColors;
        static TetrisDrawer() {
            tetrisRect = new RectangleShape();
            cellColors = new Color[(byte) CellType.Count];
            cellColors[(byte) CellType.EMPTY] = Color.Black;
            cellColors[(byte) CellType.GARBAGE] = ConvertColor(SysCol.SlateGray);
            cellColors[(byte) CellType.J] = ConvertColor(SysCol.Blue);
            cellColors[(byte) CellType.L] = ConvertColor(SysCol.OrangeRed);
            cellColors[(byte) CellType.S] = ConvertColor(SysCol.Lime);
            cellColors[(byte) CellType.T] = ConvertColor(SysCol.BlueViolet);
            cellColors[(byte) CellType.Z] = ConvertColor(SysCol.Red);
            cellColors[(byte) CellType.I] = ConvertColor(SysCol.Cyan);
            cellColors[(byte) CellType.O] = ConvertColor(SysCol.Gold);
            tetrisRect.OutlineColor = cellColors[(byte) CellType.GARBAGE];
            SetScale(new Vector2f(1, 1));
        }
        public static void SetScale(Vector2f scale) {
            tetrisRect.Size = scale;
        }
        public static void DrawTetrisBoard(RenderTarget target, Vector2f pos, Tetris tetris, TetrisRNG rng, IEnumerable<TetriminoState> moves, TetriminoState move) {
            void DrawTetrimino(Tetrimino mino, int x, int y, int rot) {
                tetrisRect.FillColor = cellColors[(byte) mino.type];
                for (int i = mino.states.GetLength(1) - 1; i >= 0 ; i--) {
                    Pair<sbyte> block = mino.states[rot, i];
                    DrawCell(target, block.x + x, block.y + y, pos);
                }
            }
            for (int y = 0; y < 20; y++) {
                for (int x = 0; x < 10; x++) {
                    tetrisRect.FillColor = cellColors[(byte) tetris.GetCell(x, y + 20)];
                    if (tetrisRect.FillColor == Color.Black) {
                        tetrisRect.OutlineThickness = 1;
                    }
                    DrawCell(target, x + 5, y, pos);
                    if (tetrisRect.FillColor == Color.Black) {
                        tetrisRect.OutlineThickness = 0;
                    }
                }
            }
            if (moves != null) {
                Vector2f origScale = tetrisRect.Scale;
                Vector2f fifthScale = new Vector2f(0.20f, 0.20f);
                Vector2f twoFifthScale = fifthScale * 2;
                tetrisRect.Scale = fifthScale;
                foreach (TetriminoState m in moves) {
                    if (m.Equals(move)) {
                        tetrisRect.Scale = twoFifthScale;
                    }
                    DrawTetrimino(tetris.current, m.x + 5, m.y - 20, m.rot);
                    if (m.Equals(move)) {
                        tetrisRect.Scale = fifthScale;
                    }
                }
                tetrisRect.Scale = origScale;
            }
            //DrawTetrimino(tetris.current, move.x + 5, move.y - 20, move.rot);
            DrawTetrimino(tetris.current, tetris.pieceX + 5, tetris.pieceY - 20, tetris.pieceRotation);
            if (tetris.hold != null) {
                DrawTetrimino(tetris.hold, 1, 1, 0);
            }
            for (int x = 0; x < 5; x++) {
                DrawTetrimino(rng.GetPiece(x), 17, 1 + 4 * x, 0);
            }
        }
        public static void DrawCell(RenderTarget target, int x, int y, Vector2f pos) {
            tetrisRect.Position = new Vector2f(x * tetrisRect.Size.X, y * tetrisRect.Size.Y) + pos;
            target.Draw(tetrisRect);
        }
        private static Color ConvertColor(SysCol col) {
            return new Color(col.R, col.G, col.B, col.A);
        }
    }
}
