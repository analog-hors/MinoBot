using MinoTetris;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MinoBot
{
    public class Pathfinder
    {
        private MoveNode[,,] field = new MoveNode[10, 40, 4];
        public HashSet<TetriminoState> FindAllMoves(Tetris tetris) {
            HashSet<TetriminoState> moves = new HashSet<TetriminoState>();
            Array.Clear(field, 0, field.Length);
            int len = (int) Move.Count;
            Queue<TetriminoState> children = new Queue<TetriminoState>();
            void ExpandNode(int x, int y, int r) {
                MoveNode parent = field[x, y, r];
                tetris.pieceX = x;
                tetris.pieceY = y;
                tetris.pieceRotation = r;
                DoMove(tetris, Move.SONIC_DROP);
                moves.Add(new TetriminoState(tetris.pieceX, tetris.pieceY, tetris.pieceRotation));
                for (int i = 0; i < len; i++) {
                    Move move = (Move) i;
                    tetris.pieceX = x;
                    tetris.pieceY = y;
                    tetris.pieceRotation = r;
                    if (DoMove(tetris, move)) {
                        MoveNode prev = field[tetris.pieceX, tetris.pieceY, tetris.pieceRotation];
                        if (prev == null || prev.rank > parent.rank + MoveNode.GetRank(move)) {
                            MoveNode child = new MoveNode(move, parent);
                            field[tetris.pieceX, tetris.pieceY, tetris.pieceRotation] = child;
                            children.Enqueue(new TetriminoState(tetris.pieceX, tetris.pieceY, tetris.pieceRotation));
                        }
                    }
                }
                tetris.SetPiece(tetris.current);
            }
            tetris.SetPiece(tetris.current);
            field[tetris.pieceX, tetris.pieceY, tetris.pieceRotation] = new MoveNode(Move.Count, null);
            ExpandNode(tetris.pieceX, tetris.pieceY, tetris.pieceRotation);
            while (children.Count != 0) {
                TetriminoState child = children.Dequeue();
                ExpandNode(child.x, child.y, child.rot);
            }
            return moves;
        }
        public List<Move> GetPath(int x, int y, int rot) {
            MoveNode node = field[x, y, rot];
            return node == null ? null : node.GetMoves();
        }
        public static bool DoMove(Tetris tetris, Move move) {
            bool ret = false;
            switch (move) {
                case Move.LEFT: return tetris.MoveLeft();
                case Move.RIGHT: return tetris.MoveRight();
                case Move.DAS_LEFT:
                    while (tetris.MoveLeft()) {
                        ret = true;
                    }
                    break;
                case Move.DAS_RIGHT:
                    while (tetris.MoveRight()) {
                        ret = true;
                    }
                    break;
                case Move.ROT_LEFT: return tetris.TurnLeft();
                case Move.ROT_RIGHT: return tetris.TurnRight();
                case Move.SONIC_DROP:
                    while (tetris.SoftDrop()) {
                        ret = true;
                    }
                    break;
                case Move.SOFT_DROP: return tetris.SoftDrop();
            }
            return ret;
        }
        private class MoveNode
        {
            public int rank;
            public Move move;
            public MoveNode parent;
            public MoveNode(Move move, MoveNode parent) {
                this.move = move;
                this.parent = parent;
                rank = parent == null ? 0 : parent.rank + GetRank(move);
            }
            public List<Move> GetMoves() {
                List<Move> moves = new List<Move>();
                MoveNode node = this;
                while (node.parent != null) {
                    moves.Add(node.move);
                    node = node.parent;
                }
                moves.Reverse();
                return moves;
            }
            public static int GetRank(Move move) {
                switch (move) {
                    //Rotations rated higher because they can move more than one step at a time through wallkicking
                    case Move.ROT_LEFT:
                    case Move.ROT_RIGHT:
                        return 1;
                    //Ranked better than their DAS equivalents
                    case Move.LEFT:
                    case Move.RIGHT:
                    case Move.SOFT_DROP:
                        return 2;
                    //Slow
                    case Move.DAS_LEFT:
                    case Move.DAS_RIGHT:
                        return 5;
                    //Slowest
                    case Move.SONIC_DROP:
                        return 10;
                    default:
                        return 0;
                }
            }
        }
    }
    public struct TetriminoState : IEquatable<object>
    {
        public int x { get; }
        public int y { get; }
        public int rot { get; }
        public TetriminoState(int x, int y, int rot) {
            this.x = x;
            this.y = y;
            this.rot = rot;
        }
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            TetriminoState other = (TetriminoState) obj;
            return other.x == x && other.y == y && other.rot == rot;
        }
        public override int GetHashCode() {
            return HashCode.Combine(x, y, rot);
        }
    }
    
    public enum Move
    {
        ROT_LEFT,
        ROT_RIGHT,
        LEFT,  
        RIGHT,
        SOFT_DROP,
        DAS_LEFT, 
        DAS_RIGHT,
        SONIC_DROP,
        Count // Hack for getting length of enum
    }
}
