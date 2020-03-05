using MinoBot.Evaluators;
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
        private static readonly float sqrt2 = (float) Math.Sqrt(2);
        public Tetrimino[] queue;
        public bool holdAllowed = true;
        public int maxDepth = 0;
        public TetrisBot(Tetris tetris) {
            pathfinder = new Pathfinder();
            tree = new Tree(NewTetrisState(tetris)) {
                expander = NodeExpander,
                selector = UCTSelector,
                evaluator = StandardEvaluator.standard.Evaluate
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
            queue = new Tetrimino[5];
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
