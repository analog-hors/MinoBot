using MinoTetris;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MinoBot
{
    public class Pathfinder
    {
        public MoveNode[,,] field = new MoveNode[10, 40, 4];
        public HashSet<TetriminoState> FindAllMoves(Tetris tetris, int shiftDelay, int rotateDelay, int softDropDelay) {
            TetriminoStateComparer.standard.tetrimino = tetris.current;
            HashSet<TetriminoState> moves = new HashSet<TetriminoState>(TetriminoStateComparer.standard);
            Array.Clear(field, 0, field.Length);
            int len = (int) Move.Count;
            Queue<TetriminoState> children = new Queue<TetriminoState>();
            void ExpandNode(int x, int y, byte r) {
                MoveNode parent = field[x, y, r];
                for (int i = 0; i < len; i++) {
                    Move move = (Move) i;
                    tetris.pieceX = x;
                    tetris.pieceY = y;
                    tetris.pieceRotation = r;
                    bool moveSuccess = DoMove(tetris, move);
                    TetriminoState childPos = new TetriminoState(tetris.pieceX, tetris.pieceY, tetris.pieceRotation);
                    if (moveSuccess) {
                        MoveNode prev = field[tetris.pieceX, tetris.pieceY, tetris.pieceRotation];
                        bool isMoreImportant = !prev.valid;
                        int distanceTravelled = Math.Abs(x - tetris.pieceX) + Math.Abs(y - tetris.pieceY);
                        if (!isMoreImportant) {
                            int prevDist = prev.totalDistanceTravelled;
                            if (prev.move == Move.SONIC_DROP) {
                                prevDist -= prev.distanceTravelled;
                            }
                            int newDist = parent.totalDistanceTravelled;
                            if (move != Move.SONIC_DROP) {
                                newDist += distanceTravelled;
                            }
                            isMoreImportant = prevDist > newDist;
                        }
                        if (isMoreImportant) {
                            MoveNode child = new MoveNode {
                                move = move,
                                parent = new TetriminoState(x, y, r),
                                distanceTravelled = distanceTravelled,
                                totalDistanceTravelled = parent.totalDistanceTravelled + distanceTravelled,
                                valid = true,
                                tspin = tetris.elibigleForTspin
                            };
                            field[tetris.pieceX, tetris.pieceY, tetris.pieceRotation] = child;
                            children.Enqueue(childPos);
                        }
                    }
                    if (move == Move.SONIC_DROP) {
                        moves.Add(childPos);
                    }
                }
                tetris.SetPiece(tetris.current);
            }
            tetris.SetPiece(tetris.current);
            field[tetris.pieceX, tetris.pieceY, tetris.pieceRotation] = new MoveNode {
                root = true,
                valid = true
            };
            ExpandNode(tetris.pieceX, tetris.pieceY, tetris.pieceRotation);
            while (children.TryDequeue(out TetriminoState child)) {
                ExpandNode(child.x, child.y, child.rot);
            }
            return moves;
        }
        public List<Move> GetPath(int x, int y, int rot) {
            MoveNode node = field[x, y, rot];
            if (!node.valid) {
                return null;
            }
            List<Move> moves = new List<Move>();
            bool skipping = true;
            while (!node.root) {
                if (node.move != Move.SONIC_DROP) {
                    skipping = false;
                }
                if (!skipping) {
                    moves.Add(node.move);
                }
                node = field[node.parent.x, node.parent.y, node.parent.rot];
            }
            moves.Reverse();
            return moves;
        }
        public static bool DoMove(Tetris tetris, Move move) {
            bool ret = false;
            switch (move) {
                case Move.LEFT: return tetris.MoveLeft();
                case Move.RIGHT: return tetris.MoveRight();
                case Move.ROT_LEFT: return tetris.TurnLeft();
                case Move.ROT_RIGHT: return tetris.TurnRight();
                case Move.SONIC_DROP:
                    while (tetris.SoftDrop()) {
                        ret = true;
                    }
                    break;
            }
            return ret;
        }
        public struct MoveNode
        {
            public int totalDistanceTravelled; //Total distance it took to get here while allowing further moves
            public int distanceTravelled; //Distance for just the immediate move
            public Move move;
            public TetriminoState parent;
            public bool root;
            public bool valid;
            public TspinType tspin;
        }
        private class TetriminoStateComparer : IEqualityComparer<TetriminoState>
        {
            public static TetriminoStateComparer standard = new TetriminoStateComparer(null);
            public Tetrimino tetrimino;
            public TetriminoStateComparer(Tetrimino tetrimino) {
                this.tetrimino = tetrimino;
            }
            public bool Equals([AllowNull] TetriminoState x, [AllowNull] TetriminoState y) {
                for (int i = 0; i < 4; i++) {
                    Pair<int> xCell = GetCellPosition(x, i);
                    Pair<int> yCell = GetCellPosition(y, i);
                    if (xCell.x != yCell.x || xCell.y != yCell.y) {
                        return false;
                    }
                }
                return true;
            }
            private Pair<int> GetCellPosition(TetriminoState move, int i) {
                Pair<sbyte> cell = tetrimino.states[move.rot, i];
                return new Pair<int>(cell.x + move.x, cell.y + move.y);
            }
            public int GetHashCode([DisallowNull] TetriminoState move) {
                uint hashcode = 0;
                for (int i = 0; i < 4; i++) {
                    Pair<int> cell = GetCellPosition(move, i);
                    hashcode ^= (uint)(((byte)(cell.x ^ cell.y)) >> (i * 8));
                }
                return unchecked((int) hashcode);
            }
        }
    }
    public struct TetriminoState : IEquatable<object>
    {
        public int x { get; }
        public int y { get; }
        public byte rot { get; }
        public TetriminoState(int x, int y, byte rot) {
            this.x = x;
            this.y = y;
            this.rot = rot;
        }
        public override bool Equals(object obj) {
            TetriminoState other = (TetriminoState) obj;
            return other.x == x && other.y == y && other.rot == rot;
        }
        public override int GetHashCode() {
            return x ^ (y << 8) ^ (rot << 16);
        }
    }
    
    public enum Move
    {
        ROT_LEFT,
        ROT_RIGHT,
        LEFT,  
        RIGHT,
        SONIC_DROP,
        Count // Hack for getting length of enum
    }
}
