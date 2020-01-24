using MinoBot.MonteCarlo;
using MinoTetris;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MinoBot
{
    public class TetrisBot
    {
        private Tree tree;
        private Pathfinder pathfinder;
        private static float sqrt2 = (float)Math.Sqrt(2);
        private Random random;
        public Tetrimino[] queue;
        public bool holdAllowed = true;
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
        }
        public void Update(Tetris tetris) {
            UpdateQueue(tetris);
            int diff = 0 - tree.root.state.tetRng.index;
            void ResetAll(Node node) {
                node.state.tetRng.index += diff;
                foreach (Node child in node.children) {
                    ResetAll(child);
                }
            }
            ResetAll(tree.root);
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
            return (node.simulations == 0 ? 0 : (node.score / node.simulations)) + (sqrt2 * 2 * (float)Math.Sqrt(Math.Log(node.parent.simulations) / node.simulations));
        }
        private Node NodeExpander(Node node) {
            void CreateChildren(Tetris tetris) {
                HashSet<TetriminoState> moves = pathfinder.FindAllMoves(tetris, 1, 1, 1);
                foreach (TetriminoState move in moves) {
                    Node child = new Node(node.state.DoMove(move, tetris.held)) {
                        move = move,
                        parent = node
                    };
                    if (!child.state.tetris.blockOut) {
                        node.children.Add(child);
                    }
                }
            }
            CreateChildren(node.state.tetris);
            if (holdAllowed) {
                Tetris held = new Tetris(node.state.tetris, new CustomTetrisRNG(node.state.tetRng));
                held.Hold();
                CreateChildren(held);
            }
            int i = random.Next(1, node.children.Count);
            foreach (Node child in node.children) {
                if (--i == 0) {
                    return child;
                }
            }
            return null;
        }
    }
    public class MinoBotEvaluator {
        public static MinoBotEvaluator standard = new MinoBotEvaluator();
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
        public float Evaluate(TetrisState state, TetriminoState move) {
            TetrisState tState = state;
            int holes = 0;
            int buriedHoles = 0;
            for (int x = 0; x < 10; x++) {
                for (int y = 1; y < 40; y++) {
                    if (tState.tetris.GetCell(x, y) == CellType.EMPTY) {
                        int newY = y;
                        while (tState.tetris.GetCell(x, --newY) != CellType.EMPTY) {
                            if (newY == y - 1) {
                                holes += 1;
                            } else {
                                buriedHoles += 1;
                            }
                        }
                        
                    }
                }
            }
            //A well is defined as a dip down two or more tiles 
            int wells = 0;
            //A spike is defined as a dip up two or more tiles 
            int spikes = 0;
            int[] heights = new int[10];
            for (int x = 0; x < 10; x++) {
                for (int y = 0; y < 40; y++) {
                    if (tState.tetris.GetCell(x, y) != CellType.EMPTY) {
                        heights[x] = 39 - y;
                    }
                    if (wellPattern.Test(tState.tetris, x, y)) {
                        wells += 1;
                    }
                    if (spikePattern.Test(tState.tetris, x, y)) {
                        spikes += 1;
                    }
                }
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
            //tState.accumulatedScore += (maxHeight * -10f);
            float transientScore = 0;
            transientScore += holes * holes * -1;
            transientScore += buriedHoles * buriedHoles * -0.5f;
            //transientScore += maxHeight * -10;
            transientScore += totalHeight * totalHeight * -0.1f;
            if (move.y <= 30) {
                transientScore += -100;
            }
            if (tState.tetris.blockOut) {
                transientScore += -1000;
            }
            int diff = maxHeight - minHeight;
            if (diff < 3) {
                diff = 0;
            }
            //transientScore += diff * diff * -1f;
            transientScore += tState.tetris.linesCleared * tState.tetris.linesCleared * tState.tetris.linesCleared;
            transientScore += wells > 1 ? (wells * wells * -1) : 0;
            transientScore += spikes * spikes * -1;
            return tState.accumulatedScore * 1 + transientScore * 1;
        }
        private class Pattern
        {
            private CellPattern[] pattern;
            public Pattern(CellPattern[] pattern) {
                this.pattern = pattern;
            }
            public bool Test(Tetris tetris, int x, int y) {
                foreach (CellPattern cell in pattern) {
                    if (tetris.GetCell(x + cell.x, y + cell.y) != CellType.EMPTY == cell.filled) {
                        return false;
                    }
                }
                return true;
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
        public float accumulatedScore;
        public bool usesHeld;
        public TetrisState(Tetris tetris, CustomTetrisRNG tetRng) {
            this.tetris = tetris;
            this.tetRng = tetRng;
        }
        public bool Finished() {
            return tetris.blockOut || tetRng.index + 1 >= tetRng.bot.queue.Length;
        }
        public TetrisState DoMove(TetriminoState move, bool hold) {
            CustomTetrisRNG childRng = new CustomTetrisRNG(tetRng);
            Tetris child = new Tetris(tetris, childRng);
            if (hold) {
                child.Hold();
            }
            child.pieceX = move.x;
            child.pieceY = move.y;
            child.pieceRotation = move.rot;
            child.HardDrop();
            return new TetrisState(child, childRng) {
                accumulatedScore = accumulatedScore,
                usesHeld = hold
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
            throw new NotImplementedException();
        }
    }
}
