using System;
using System.Collections.Generic;
using System.Text;

namespace MinoTetris
{
    public interface TetrisRNGProvider
    {
        Tetrimino NextPiece();
        Tetrimino GetPiece(int index);
        int NextGarbageHole();
    }
}
