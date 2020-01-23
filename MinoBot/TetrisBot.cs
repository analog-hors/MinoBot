using MinoBot.MonteCarlo;
using MinoTetris;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MinoBot
{
    public class TetrisBot
    {
        private Tree<TetrisState, TetriminoState> tree;
        private Pathfinder pathfinder;
        private static float sqrt2 = (float)Math.Sqrt(2);
        private Random random;
        public Tetrimino[] queue;
        public TetrisBot(Tetris tetris, Random random) {
            this.random = random;
            pathfinder = new Pathfinder();
            tree = new Tree<TetrisState, TetriminoState>(NewTetrisState(tetris)) {
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
            int diff = 0 - tree.root.state.GetSelf().tetRng.index;
            void ResetAll(Node<TetrisState, TetriminoState> node) {
                node.state.GetSelf().tetRng.index += diff;
                foreach (Node<TetrisState, TetriminoState> child in node.children) {
                    ResetAll(child);
                }
            }
            ResetAll(tree.root);
        }
        public void Think() {
            tree.Think();
        }
        public Node<TetrisState, TetriminoState> GetMove(Tetris tetris) {
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
        private static float UCTSelector<TState, TMove>(Node<TState, TMove> node) where TState : State<TState, TMove> {
            return node.score / node.simulations + (sqrt2 * 2 * (float)Math.Sqrt(Math.Log(node.parent.simulations) / node.simulations));
        }
        private Node<TetrisState, TetriminoState> NodeExpander(Node<TetrisState, TetriminoState> node) {
            HashSet<TetriminoState> moves = pathfinder.FindAllMoves(node.state.GetSelf().tetris, 1, 1, 1);
            foreach (TetriminoState move in moves) {
                Node<TetrisState, TetriminoState> child = new Node<TetrisState, TetriminoState>(node.state.DoMove(move)) {
                    move = move,
                    parent = node
                };
                if (!child.state.GetSelf().tetris.blockOut) {
                    node.children.Add(child);
                }
            }
            int i = random.Next(1, node.children.Count);
            foreach (Node<TetrisState, TetriminoState> child in node.children) {
                if (--i == 0) {
                    return child;
                }
            }
            return null;
        }
    }
    public class MinoBotEvaluator {
        public static MinoBotEvaluator standard = new MinoBotEvaluator();
        public float Evaluate(State<TetrisState, TetriminoState> state, TetriminoState move) {
            TetrisState tState = state.GetSelf();
            int holes = 0;
            for (int x = 0; x < 10; x++) {
                for (int y = 1; y < 40; y++) {
                    if (tState.tetris.GetCell(x, y) == CellType.EMPTY && tState.tetris.GetCell(x, y - 1) != CellType.EMPTY) {
                        holes += 1;
                    }
                }
            }
            int[] heights = new int[10];
            for (int x = 0; x < 10; x++) {
                for (int y = 0; y < 40; y++) {
                    if (tState.tetris.GetCell(x, y) != CellType.EMPTY) {
                        heights[x] = 39 - y;
                    }
                }
            }
            //A well is defined as a dip down two or more tiles 
            //A spike is defined as a dip up two or more tiles 
            int wellsAndSpikes = 0;
            int maxHeight = 0;
            int minHeight = 0;
            int totalHeight = 0;
            for (int i = 0; i < heights.Length; i++) {
                int height = heights[i];
                int diffPrev = i == 0 ? 40 : heights[i - 1] - heights[i];
                int diffNext = i + 1 == heights.Length ? 40 : heights[i] - heights[i + 1];
                if (diffPrev <= 2 || diffNext <= -2) {
                    wellsAndSpikes += 1;
                }
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
            //transientScore += maxHeight * -10;
            transientScore += totalHeight * totalHeight * -0.1f;
            int diff = maxHeight - minHeight;
            if (diff < 3) {
                diff = 0;
            }
            //transientScore += (diff * diff * -1f);
            transientScore += tState.tetris.linesCleared * tState.tetris.linesCleared;
            //transientScore += wellsAndSpikes > 1 ? ((wellsAndSpikes + 1) * (wellsAndSpikes + 1) * -0.1f) : 0;// One well is fine.
            return tState.accumulatedScore * 1 + transientScore * 1;
        }
    }
    public class TetrisState : State<TetrisState, TetriminoState>
    {
        public Tetris tetris;
        public CustomTetrisRNG tetRng;
        public float accumulatedScore;
        public TetrisState(Tetris tetris, CustomTetrisRNG tetRng) {
            this.tetris = tetris;
            this.tetRng = tetRng;
        }
        public bool Finished() {
            return tetris.blockOut || tetRng.index + 1 >= tetRng.bot.queue.Length;
        }
        public TetrisState DoMove(TetriminoState move) {
            CustomTetrisRNG childRng = new CustomTetrisRNG(tetRng);
            Tetris child = new Tetris(tetris, childRng);
            child.pieceX = move.x;
            child.pieceY = move.y;
            child.pieceRotation = move.rot;
            child.HardDrop();
            return new TetrisState(child, childRng) {
                accumulatedScore = accumulatedScore
            };
        }
        public TetrisState GetSelf() {
            return this;
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
