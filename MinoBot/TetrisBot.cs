using MinoBot.MonteCarlo;
using MinoTetris;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MinoBot
{
    public class TetrisBot
    {
        public Tree tree;
        private Pathfinder pathfinder;
        private static float sqrt2 = (float)Math.Sqrt(2);
        private Random random;
        public Tetrimino[] queue;
        public bool holdAllowed = true;
        public int maxDepth = 0;
        public TetrisBot(Tetris tetris, Random random) {
            this.random = random;
            pathfinder = new Pathfinder();
            tree = new Tree(NewTetrisState(tetris)) {
                expander = NodeExpander,
                selector = UCTSelector,
                evaluator = MinoBotEvaluator.standard.Evaluate
            };
        }
        public void Reset(Tetris tetris) {
            tree.Reset(NewTetrisState(tetris));
            maxDepth = 0;
        }
        public void Update(Tetris tetris) {
            UpdateQueue(tetris);
            int diff = 0 - tree.root.state.tetRng.index;
            void ResetAll(Node node) {
                node.state.tetRng.index += diff;
                node.depth -= 1;
                foreach (Node child in node.children) {
                    ResetAll(child);
                }
            }
            ResetAll(tree.root);
            maxDepth = 0;
        }
        public void Think() {
            tree.Think();
        }
        public Node GetMove() {
            return tree.GetMove();
        }
        private TetrisState NewTetrisState(Tetris tetris) {
            UpdateQueue(tetris);
            CustomTetrisRNG rng = new CustomTetrisRNG(this);
            return new TetrisState(new Tetris(tetris, rng), rng);
        }
        private void UpdateQueue(Tetris tetris) {
            queue = new Tetrimino[6];
            for (int i = 0; i < queue.Length; i++) {
                queue[i] = tetris.rng.GetPiece(i);
            }
        }
        private static float UCTSelector(Node node) {
            return (node.simulations == 0 ? 0 : (node.score / node.simulations)) + (sqrt2 * 1 * (float)Math.Sqrt(Math.Log(node.parent.simulations) / node.simulations));
        }
        private void NodeExpander(Node node) {
            void CreateChildren() {
                HashSet<TetriminoState> moves = pathfinder.FindAllMoves(node.state.tetris, 1, 1, 1);
                foreach (TetriminoState move in moves) {
                    TetrisState childState = node.state.DoMove(move, pathfinder.field[move.x, move.y, move.rot], node.state.tetris.held);
                    if (!childState.tetris.blockOut) {
                        #if POOLING
                        Node child = NodePool.standard.Rent(childState);
                        #else
                        Node child = new Node(childState);
                        #endif
                        child.move = move;
                        child.parent = node;
                        node.children.Add(child);
                    }
                }
            }
            CreateChildren();
            if (holdAllowed) {
                bool reverse = node.state.tetris.hold == null;
                node.state.tetris.Hold();
                CreateChildren();
                node.state.tetris.held = false;
                node.state.tetris.Hold();
                if (reverse) {
                    node.state.tetRng.index -= 1;
                    node.state.tetris.hold = null;
                }
            }
        }
    }
    public class MinoBotEvaluator {
        public static MinoBotEvaluator standard = new MinoBotEvaluator();
        public bool logging = false;
        private static Pattern wellPattern = new Pattern(new Pattern.CellPattern[] {
            new Pattern.CellPattern(-1, 0, true),
            new Pattern.CellPattern(-1, 1, true),
            new Pattern.CellPattern(1, 0, true),
            new Pattern.CellPattern(1, 1, true),
            new Pattern.CellPattern(0, 0, false),
            new Pattern.CellPattern(0, 1, false)
        });
        private static Pattern spikePattern = new Pattern(new Pattern.CellPattern[] {
            new Pattern.CellPattern(-1, 0, false),
            new Pattern.CellPattern(-1, 1, false),
            new Pattern.CellPattern(1, 0, false),
            new Pattern.CellPattern(1, 1, false),
            new Pattern.CellPattern(0, 0, true),
            new Pattern.CellPattern(0, 1, true)
        });
        public (float, float) Evaluate(Node node) {
            TetriminoState move = node.move;
            TetrisState state = node.state;
            TetrisState parentState = node.parent.state;
            float accumulated = 0;
            float transient = 0;

            int availibleTs = 0;
            for (int i = 0; true; i++) {
                Tetrimino tetrimino = parentState.tetris.rng.GetPiece(i);
                if (tetrimino == null) {
                    break;
                }
                if (tetrimino == Tetrimino.T) {
                    availibleTs += 1;
                }
            }
            if (logging) {
                Console.WriteLine($"{availibleTs} availible Ts.");
            }

            int totalEdgeTiles = 0;
            int totalFilledEdgeTiles = 0;
            void TestEdge(int x, int y) {
                for (int i = 0; i < 4; i++) {
                    Pair<sbyte> block = state.tetrimino.states[move.rot, i];
                    if (x == block.x + move.x && y == block.y + move.y) {
                        return;
                    }
                }
                totalEdgeTiles += 1;
                if (parentState.tetris.GetCell(x, y) != CellType.EMPTY) {
                    totalFilledEdgeTiles += 1;
                }
            }
            for (int i = 0; i < 4; i++) {
                Pair<sbyte> block = state.tetrimino.states[move.rot, i];
                int x = block.x + move.x;
                int y = block.y + move.y;
                TestEdge(x + 1, y);
                TestEdge(x - 1, y);
                TestEdge(x, y + 1);
                TestEdge(x, y - 1);
            }

            int holes = 0;
            int buriedHoles = 0;
            bool isPC = true;
            int wells = 0;
            int spikes = 0;
            int tslots = 0;
            int[] heights = new int[10];
            for (int x = 0; x < 10; x++) {
                for (int y = 20; y < 40; y++) {
                    if (state.tetris.GetCell(x, y) == CellType.EMPTY) {
                        if (39 - y < heights[x]) {
                            holes += 1;//Math.Max(1, heights[x] - 39 + y / 2);
                        }
                    } else {
                        heights[x] = 39 - y;
                        isPC = false;
                    }
                    if (wellPattern.Test(state.tetris, x, y) == 0) {
                        wells += 1;
                    }
                    if (spikePattern.Test(state.tetris, x, y) == 0) {
                        spikes += 1;
                    }
                    if (state.tetris.GetCell(x, y) == CellType.EMPTY) {
                        int tCorners = 0;
                        if (state.tetris.GetCell(x - 1, y - 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (state.tetris.GetCell(x + 1, y - 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (state.tetris.GetCell(x - 1, y + 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (state.tetris.GetCell(x + 1, y + 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (tCorners > 2) {
                            int tHoles = 0;
                            if (state.tetris.GetCell(x - 1, y) == CellType.EMPTY) {
                                tHoles += 1;
                            }
                            if (state.tetris.GetCell(x + 1, y) == CellType.EMPTY) {
                                tHoles += 1;
                            }
                            if (state.tetris.GetCell(x, y - 1) != CellType.EMPTY) {
                                tHoles = 0;
                                //tHoles += 1;
                            }
                            if (state.tetris.GetCell(x, y + 1) == CellType.EMPTY) {
                                tHoles += 1;
                            }
                            if (tHoles > 2) {
                                if (logging) {
                                    Console.WriteLine($"tslot at {x}, {y}");
                                }
                                tslots += 1;
                            }
                        }
                    }
                }
            }

            if (isPC) {
                return (5000, 0);
            }

            int maxHeight = 0;
            int minHeight = 0;
            int totalHeight = 0;
            for (int i = 0; i < heights.Length; i++) {
                int height = heights[i];
                if (height > maxHeight) {
                    maxHeight = height;
                }
                if (height < minHeight) {
                    minHeight = height;
                }
                totalHeight += height;
            }
            
            int holePenalty = holes + buriedHoles;
            accumulated += holePenalty * holePenalty * -1f;
            if (move.y <= 35) {
                int moveHeight = 39 - move.y;
                accumulated += moveHeight * moveHeight * -1;
            }
            accumulated += maxHeight * -1;
            accumulated += state.tetris.tspin switch {
                TspinType.MINI => state.tetris.linesCleared switch {
                    1 => 100,
                    2 => 200,
                    _ => 0,
                },
                TspinType.FULL => state.tetris.linesCleared switch {
                    1 => 250,
                    2 => 2500,
                    3 => 5000,
                    _ => 0
                },
                _ => state.tetris.linesCleared switch {
                    1 => 1,
                    2 => 4,
                    3 => 9,
                    4 => 16,
                    _ => 0
                }
            };
            if ((state.tetris.tspin == TspinType.NONE || state.tetris.linesCleared == 0) && state.tetrimino == Tetrimino.T) {
                if (logging) {
                    Console.WriteLine("punished for wasting T");
                }
                accumulated += -100;
            }
            accumulated += wells > 1 ? (wells * wells * -1) : 0;
            accumulated += spikes * spikes * -1;
            float pieceFit = totalFilledEdgeTiles / (float) totalEdgeTiles;
            accumulated += pieceFit * pieceFit * 100;
            accumulated += Math.Abs(tslots - Math.Max(1, availibleTs)) * -25;
            return (accumulated, transient);
        }

        private class Pattern
        {
            private CellPattern[] pattern;
            public Pattern(CellPattern[] pattern) {
                this.pattern = pattern;
            }
            public int Test(Tetris tetris, int x, int y) {
                int diff = 0;
                foreach (CellPattern cell in pattern) {
                    if (tetris.GetCell(x + cell.x, y + cell.y) != CellType.EMPTY == cell.filled) {
                        diff += 1;
                    }
                }
                return diff;
            }
            public struct CellPattern
            {
                public readonly sbyte x;
                public readonly sbyte y;
                public readonly bool filled;
                public CellPattern(sbyte x, sbyte y, bool filled) {
                    this.x = x;
                    this.y = y;
                    this.filled = filled;
                }
            }
        }
    }
    public class TetrisState
    {
        public Tetris tetris;
        public CustomTetrisRNG tetRng;
        public bool usesHeld;
        public Tetrimino tetrimino;
        private bool setFinished;
        public TetrisState(Tetris tetris, CustomTetrisRNG tetRng) {
            this.tetris = tetris;
            this.tetRng = tetRng;
            tetrimino = tetris.current;
        }
        public bool Finished() {
            return setFinished || tetris.blockOut || tetRng.index + 1 >= tetRng.bot.queue.Length;
        }
        public void Finished(bool finished) {
            setFinished = finished;
        }
        public TetrisState DoMove(TetriminoState move, Pathfinder.MoveNode moveNode, bool hold) {
            CustomTetrisRNG childRng = new CustomTetrisRNG(tetRng);
            Tetris child = new Tetris(tetris, childRng);
            if (hold) {
                child.Hold();
            }
            Tetrimino tetrimino = child.current;
            child.pieceX = move.x;
            child.pieceY = move.y;
            child.pieceRotation = move.rot;
            child.HardDrop();
            child.tspin = moveNode.tspin;
            return new TetrisState(child, childRng) {
                usesHeld = hold,
                tetrimino = tetrimino
            };
        }
    }
    public class CustomTetrisRNG : TetrisRNGProvider
    {
        public TetrisBot bot;
        public int index;
        public CustomTetrisRNG(TetrisBot bot) {
            this.bot = bot;
            index = 0;
        }
        public CustomTetrisRNG(CustomTetrisRNG from) {
            bot = from.bot;
            index = from.index;
        }
        public Tetrimino NextPiece() {
            return index < bot.queue.Length ? bot.queue[index++] : null;
        }
        public int NextGarbageHole() {
            throw new NotImplementedException();
        }
        public Tetrimino GetPiece(int index) {
            return index < bot.queue.Length ? bot.queue[index] : null;
        }
    }
}
