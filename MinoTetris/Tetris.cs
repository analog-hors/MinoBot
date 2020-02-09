using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MinoTetris
{
    public class Tetris : ITetrisGame
    {
        private const string DLL_PATH = "rust_tetris.dll";
        public bool blockOut { get; private set; }
        public Tetrimino current { get; private set; }
        public int linesCleared { get; private set; }
        public int linesSent => throw new NotImplementedException();
        public TetrisRNGProvider rng { get; private set; }
        public bool held { get; set; }
        public Tetrimino hold { get; set; }
        private int _pieceRotation;
        public int pieceRotation {
            get {
                return _pieceRotation;
            }
            set {
                _pieceRotation = value;
                set_piece_state(boardPtr, pieceX, pieceY, pieceRotation);
            }
        }
        private int _pieceX;
        public int pieceX {
            get {
                return _pieceX;
            }
            set {
                _pieceX = value;
                set_piece_state(boardPtr, pieceX, pieceY, pieceRotation);
            }
        }
        private int _pieceY;
        public int pieceY {
            get {
                return _pieceY;
            }
            set {
                _pieceY = value;
                set_piece_state(boardPtr, pieceX, pieceY, pieceRotation);
            }
        }

        private IntPtr boardPtr;
        [DllImport(DLL_PATH)]
        private static extern void set_piece_state(IntPtr board_ptr, Int32 x, Int32 y, Int32 rot);

        [DllImport(DLL_PATH)]
        private static extern IntPtr create(byte t);
        public Tetris(TetrisRNGProvider rng) {
            this.rng = rng;
            current = rng.NextPiece();
            boardPtr = create((byte) current.type);
        }
        [DllImport(DLL_PATH)]
        private static extern IntPtr clone(IntPtr board_ptr);
        public Tetris(Tetris from, TetrisRNGProvider rng) {
            this.rng = rng;
            boardPtr = clone(from.boardPtr);
            current = from.current;
            hold = from.hold;
            pieceX = from.pieceX;
            pieceY = from.pieceY;
            pieceRotation = from.pieceRotation;
            blockOut = from.blockOut;
            held = from.held;
        }

        [DllImport(DLL_PATH)]
        private static extern byte get_cell(IntPtr board_ptr, Int32 x, Int32 y);
        public CellType GetCell(int x, int y) {
            return (CellType) get_cell(boardPtr, x, y);
        }

        [DllImport(DLL_PATH)]
        private static extern void set_cell(IntPtr board_ptr, Int32 x, Int32 y, byte cell);
        public void SetCell(int x, int y, CellType cell) {
            set_cell(boardPtr, x, y, (byte) cell);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HardDropResult
        {
            public bool block_out;
            public Int32 lines_cleared;
        }
        [DllImport(DLL_PATH)]
        private static extern HardDropResult hard_drop(IntPtr board_ptr, byte next);
        public bool HardDrop() {
            current = rng.NextPiece();
            HardDropResult result = hard_drop(boardPtr, (byte) current.type);
            blockOut = result.block_out;
            linesCleared = result.lines_cleared;
            held = false;
            return !blockOut;
        }

        [DllImport(DLL_PATH)]
        private static extern bool hold_piece(IntPtr board_ptr, byte next);
        public bool Hold() {
            held = hold_piece(boardPtr, (byte) rng.GetPiece(0).type);
            if (held) {
                if (hold == null) {
                    hold = current;
                    current = rng.NextPiece();
                } else {
                    Tetrimino t = hold;
                    hold = current;
                    current = t;
                }
            }
            return held;
        }

        [DllImport(DLL_PATH)]
        private static extern PieceState move_left(IntPtr board_ptr);
        public bool MoveLeft() {
            return SetPieceState(move_left(boardPtr));
        }

        [DllImport(DLL_PATH)]
        private static extern PieceState move_right(IntPtr board_ptr);
        public bool MoveRight() {
            return SetPieceState(move_right(boardPtr));
        }

        [DllImport(DLL_PATH)]
        private static extern bool piece_fits(IntPtr board_ptr, Int32 rot, Int32 x, Int32 y);
        public bool PieceFits(int rot, int x, int y) {
            return piece_fits(boardPtr, rot, x, y);
        }

        [DllImport(DLL_PATH)]
        private static extern PieceState set_piece(IntPtr board_ptr, byte piece);
        public void SetPiece(Tetrimino piece) {
            current = piece;
            SetPieceState(set_piece(boardPtr, (byte) piece.type));
        }

        [DllImport(DLL_PATH)]
        private static extern PieceState soft_drop(IntPtr board_ptr);
        public bool SoftDrop() {
            return SetPieceState(soft_drop(boardPtr));
        }

        [DllImport(DLL_PATH)]
        private static extern PieceState turn_left(IntPtr board_ptr);
        public bool TurnLeft() {
            return SetPieceState(turn_left(boardPtr));
        }

        [DllImport(DLL_PATH)]
        private static extern PieceState turn_right(IntPtr board_ptr);
        public bool TurnRight() {
            return SetPieceState(turn_right(boardPtr));
        }

        [DllImport(DLL_PATH)]
        private static extern void destroy(IntPtr board_ptr);
        ~Tetris() {
            destroy(boardPtr);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PieceState
        {
            public Int32 x;
            public Int32 y;
            public Int32 r;
        }
        private bool SetPieceState(PieceState state) {
            bool ret = pieceX != (pieceX = state.x);
            ret = pieceY != (pieceY = state.y) || ret;
            ret = pieceRotation != (pieceRotation = state.r) || ret;
            return ret;
        }
    }
}
