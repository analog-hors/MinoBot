using System;

namespace MinoTetris
{
    public class Tetris
    {
        private CellType[,] board;
        public TetrisRNGProvider rng { get; private set; }
        public Tetrimino current { get; private set; }
        public Tetrimino hold;// { get; private set; }
        public int pieceX;// { get; private set; }
        public int pieceY;// { get; private set; }
        public int pieceRotation;// { get; private set; }
        public bool blockOut { get; private set; }
        public int linesCleared { get; private set; }
        public int linesSent { get; private set; }
        public bool held;// { get; private set; }
        public Tetris(TetrisRNGProvider rng) {
            board = new CellType[10, 40];
            this.rng = rng;
            SetPiece(rng.NextPiece());
        }
        public Tetris(Tetris from, TetrisRNGProvider rng) {
            board = (CellType[,]) from.board.Clone();
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
            return TryMove(pieceRotation, pieceX - 1, pieceY);
        }
        public bool MoveRight() {
            return TryMove(pieceRotation, pieceX + 1, pieceY);
        }
        public bool TurnLeft() {
            return Rotate(pieceRotation > 0 ? pieceRotation - 1 : 3);
        }
        public bool TurnRight() {
            return Rotate(pieceRotation < 3 ? pieceRotation + 1 : 0);
        }
        public bool SoftDrop() {
            return TryMove(pieceRotation, pieceX, pieceY + 1);
        }
        public bool HardDrop() {
            while (TryMove(pieceRotation, pieceX, pieceY + 1)) { }
            for (int i = current.states.GetLength(1) - 1; i >= 0 ; i--) {
                Pair<sbyte> block = current.states[pieceRotation, i];
                SetCell(block.x + pieceX, block.y + pieceY, current.type);
            }
            CellType[,] newBoard = new CellType[10, 40];
            linesCleared = 0;
            for (int y = 39; y >= 0; y--) {
                bool rowFilled = true;
                for (int x = 0; x < 10; x++) {
                    if (board[x, y] == CellType.EMPTY) {
                        rowFilled = false;
                        break;
                    }
                }
                if (rowFilled) {
                    linesCleared += 1;
                } else {
                    int newY = y + linesCleared;
                    for (int x = 0; x < 10; x++) {
                        newBoard[x, newY] = board[x, y];
                    }
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
            SetPiece(hold == null ? rng.NextPiece() : hold);
            hold = temp;
            return true;
        }
        private bool Rotate(int rot) {
            int len = current.offsetTable.GetLength(1);
            for (int i = 0; i < len; i++) {
                Pair<sbyte> fromOffset = current.offsetTable[pieceRotation, i];
                Pair<sbyte> toOffset = current.offsetTable[rot, i];
                if (TryMove(rot, pieceX + fromOffset.x - toOffset.x, pieceY - (fromOffset.y - toOffset.y))) {
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
            TryMove(pieceRotation, pieceX, 20);
        }
        private bool TryMove(int rot, int x, int y) {
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
            return IsOutOfBounds(x, y) ? CellType.SOLID : board[x, y];
        }
        public void SetCell(int x, int y, CellType cell) {
            if (!IsOutOfBounds(x, y)) {
                board[x, y] = cell;
            }
        }
        public static bool IsOutOfBounds(int x, int y) {
            return x < 0 || x >= 10 || y < 0 || y >= 40;
        }
    }
}
