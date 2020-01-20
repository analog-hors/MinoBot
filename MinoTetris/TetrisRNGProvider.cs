using System;
using System.Collections.Generic;
using System.Text;

namespace MinoTetris
{
    public interface TetrisRNGProvider
    {
        Tetrimino NextPiece();
        int NextGarbageHole();
    }
}
