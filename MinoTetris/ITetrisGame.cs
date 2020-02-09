namespace MinoTetris
{
    public interface ITetrisGame
    {
        bool blockOut { get; }
        Tetrimino current { get; }
        bool held { get; set; }
        Tetrimino hold { get; set; }
        int linesCleared { get; }
        int linesSent { get; }
        int pieceRotation { get; set; }
        int pieceX { get; set; }
        int pieceY { get; set; }
        TetrisRNGProvider rng { get; }

        CellType GetCell(int x, int y);
        bool HardDrop();
        bool Hold();
        bool MoveLeft();
        bool MoveRight();
        bool PieceFits(int rot, int x, int y);
        void SetCell(int x, int y, CellType cell);
        void SetPiece(Tetrimino piece);
        bool SoftDrop();
        bool TurnLeft();
        bool TurnRight();
    }
}