using System;
using System.Collections;

namespace MinoTetris
{
    public class Tetris
    {
        private ushort[] board;
        public TetrisRNGProvider rng { get; private set; }
        public Tetrimino current { get; private set; }
        public Tetrimino hold;// { get; private set; }
        public int pieceX;// { get; private set; }
        public int pieceY;// { get; private set; }
        public byte pieceRotation;// { get; private set; }
        public bool blockOut { get; private set; }
        public int linesCleared { get; private set; }
        public int linesSent { get; private set; }
        public bool held;// { get; private set; }
        public TspinType tspin { get; private set; }
        private TspinType elibigleForTspin;
        public Tetris(TetrisRNGProvider rng) {
            board = new ushort[40];
            this.rng = rng;
            SetPiece(rng.NextPiece());
        }
        public Tetris(Tetris from, TetrisRNGProvider rng) {
            board = (ushort[]) from.board.Clone();
            this.rng = rng;
            current = from.current;
            hold = from.hold;
            pieceX = from.pieceX;
            pieceY = from.pieceY;
            pieceRotation = from.pieceRotation;
            blockOut = from.blockOut;
            linesSent = from.linesSent;
            held = from.held;
        }
        public bool MoveLeft() {
            elibigleForTspin = TspinType.NONE;
            return TryMove(pieceRotation, pieceX - 1, pieceY);
        }
        public bool MoveRight() {
            elibigleForTspin = TspinType.NONE;
            return TryMove(pieceRotation, pieceX + 1, pieceY);
        }
        public bool TurnLeft() {
            return Rotate(pieceRotation > 0 ? pieceRotation - 1 : 3);
        }
        public bool TurnRight() {
            return Rotate(pieceRotation < 3 ? pieceRotation + 1 : 0);
        }
        public bool SoftDrop() {
            elibigleForTspin = TspinType.NONE;
            return TryMove(pieceRotation, pieceX, pieceY + 1);
        }
        public bool HardDrop() {
            tspin = elibigleForTspin;
            elibigleForTspin = TspinType.NONE;
            while (TryMove(pieceRotation, pieceX, pieceY + 1)) { }
            for (int i = current.states.GetLength(1) - 1; i >= 0 ; i--) {
                Pair<sbyte> block = current.states[pieceRotation, i];
                SetCell(block.x + pieceX, block.y + pieceY, current.type);
            }
            ushort[] newBoard = new ushort[40];
            linesCleared = 0;
            for (int y = 39; y >= 0; y--) {
                bool rowFilled = board[y] == 0b0000001111111111;
                if (rowFilled) {
                    linesCleared += 1;
                } else {
                    newBoard[y + linesCleared] = board[y];
                }
            }
            board = newBoard;
            SetPiece(rng.NextPiece());
            if (!PieceFits(pieceRotation, pieceX, pieceY)) {
                blockOut = true;
            }
            held = false;
            return !blockOut;
        }
        public bool Hold() {
            if (held) {
                return false;
            }
            held = true;
            Tetrimino temp = current;
            SetPiece(hold ?? rng.NextPiece());
            hold = temp;
            return true;
        }
        private bool Rotate(int rot) {
            elibigleForTspin = TspinType.NONE;
            int len = current.offsetTable.GetLength(1);
            for (int i = 0; i < len; i++) {
                Pair<sbyte> fromOffset = current.offsetTable[pieceRotation, i];
                Pair<sbyte> toOffset = current.offsetTable[rot, i];
                int xOffset = fromOffset.x - toOffset.x;
                int yOffset = fromOffset.y - toOffset.y;
                if (TryMove((byte) rot, pieceX + xOffset, pieceY - yOffset)) {
                    if (current == Tetrimino.T) {
                        int filledCorners = 0;
                        bool topLeft = GetCell(pieceX - 1, pieceY - 1) != CellType.EMPTY;
                        if (topLeft) {
                            filledCorners += 1;
                        }
                        bool topRight = GetCell(pieceX + 1, pieceY - 1) != CellType.EMPTY;
                        if (topRight) {
                            filledCorners += 1;
                        }
                        bool bottomLeft = GetCell(pieceX - 1, pieceY + 1) != CellType.EMPTY;
                        if (bottomLeft) {
                            filledCorners += 1;
                        }
                        bool bottomRight = GetCell(pieceX + 1, pieceY + 1) != CellType.EMPTY;
                        if (bottomRight) {
                            filledCorners += 1;
                        }
                        if (filledCorners > 2) {
                            bool isFull = pieceRotation switch {
                                0 => topLeft && topRight,
                                1 => topRight && bottomRight,
                                2 => bottomLeft && bottomRight,
                                _ => bottomLeft && topLeft
                            };
                            if (isFull || (yOffset == -2 && (xOffset == 1 || xOffset == -1))) {
                                elibigleForTspin = TspinType.FULL;
                            } else {
                                elibigleForTspin = TspinType.MINI;
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public void SetPiece(Tetrimino piece) {
            current = piece;
            pieceX = 4;
            pieceY = 19;
            pieceRotation = 0;
            elibigleForTspin = TspinType.NONE;
            TryMove(pieceRotation, pieceX, 20);
        }
        private bool TryMove(byte rot, int x, int y) {
            if (PieceFits(rot, x, y)) {
                pieceX = x;
                pieceY = y;
                pieceRotation = rot;
                return true;
            }
            return false;
        }
        public bool PieceFits(int rot, int x, int y) {
            for (int i = 0; i < 4; i++) {
                Pair<sbyte> block = current.states[rot, i];
                if (GetCell(block.x + x, block.y + y) != CellType.EMPTY) {
                    return false;
                }
            }
            return true;
        }
        public CellType GetCell(int x, int y) {
            return IsOutOfBounds(x, y) || (board[y] & (1 << x)) != 0 ? CellType.GARBAGE : CellType.EMPTY;
        }
        public void SetCell(int x, int y, CellType cell) {
            if (!IsOutOfBounds(x, y)) {
                if (cell == CellType.EMPTY) {
                    board[y] &= (ushort) ~(1 << x);
                } else {
                    board[y] |= (ushort) (1 << x);
                }
            }
        }
        public static bool IsOutOfBounds(int x, int y) {
            return x < 0 || x >= 10 || y < 0 || y >= 40;
        }
    }
    public enum TspinType {
        NONE,
        MINI,
        FULL
    }
}
