using System;
using System.Collections.Generic;
using System.Text;

namespace MinoTetris
{
    public class Tetrimino
    {
        public static Pair<sbyte>[,] jlstzOffsetTable = {
            {
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0)
            }, {
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(1, 0),
                new Pair<sbyte>(1, -1),
                new Pair<sbyte>(0, 2),
                new Pair<sbyte>(1, 2)
            }, {
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0)
            }, {
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(-1, 0),
                new Pair<sbyte>(-1, -1),
                new Pair<sbyte>(0, 2),
                new Pair<sbyte>(-1, 2)
            }
        };
        public static Pair<sbyte>[,] iOffsetTable = {
            {
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(-1, 0),
                new Pair<sbyte>(2, 0),
                new Pair<sbyte>(-1, 0),
                new Pair<sbyte>(2, 0)
            }, {
                new Pair<sbyte>(-1, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 0),
                new Pair<sbyte>(0, 1),
                new Pair<sbyte>(0, -2)
            }, {
                new Pair<sbyte>(-1, 1),
                new Pair<sbyte>(1, 1),
                new Pair<sbyte>(-2, 1),
                new Pair<sbyte>(1, 0),
                new Pair<sbyte>(-2, 0)
            }, {
                new Pair<sbyte>(0, 1),
                new Pair<sbyte>(0, 1),
                new Pair<sbyte>(0, 1),
                new Pair<sbyte>(0, -1),
                new Pair<sbyte>(0, 2)
            }
        };
        public static Pair<sbyte>[,] oOffsetTable = {
            {
                new Pair<sbyte>(0, 0)
            }, {
                new Pair<sbyte>(0, -1)
            }, {
                new Pair<sbyte>(-1, -1)
            }, {
                new Pair<sbyte>(-1, 0)
            }
        };
        public Pair<sbyte>[,] states;
        public Pair<sbyte>[,] offsetTable;
        public CellType type;
        public static Tetrimino J = new Tetrimino() {
            states = new Pair<sbyte>[,] {
                {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(-1, -1)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(1, -1)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(1, 1)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(-1, 1)
                }
            },
            offsetTable = jlstzOffsetTable,
            type = CellType.J
        };
        public static Tetrimino L = new Tetrimino() {
            states = new Pair<sbyte>[,] {
                {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(1, -1)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(1, 1)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(-1, 1)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(-1, -1)
                }
            },
            offsetTable = jlstzOffsetTable,
            type = CellType.L
        };
        public static Tetrimino S = new Tetrimino() {
            states = new Pair<sbyte>[,] {
                {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(1, -1),
                    new Pair<sbyte>(-1, 0)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(1, 1),
                    new Pair<sbyte>(1, 0)
                }, {
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(-1, 1)
                }, {
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(-1, -1),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(0, 0)
                }
            },
            offsetTable = jlstzOffsetTable,
            type = CellType.S
        };
        public static Tetrimino T = new Tetrimino() {
            states = new Pair<sbyte>[,] {
                {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(-1, 0)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(0, 1)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(-1, 0)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(0, 1)
                }
            },
            offsetTable = jlstzOffsetTable,
            type = CellType.T
        };
        public static Tetrimino Z = new Tetrimino() {
            states = new Pair<sbyte>[,] {
                {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(-1, -1),
                    new Pair<sbyte>(1, 0)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(1, -1),
                    new Pair<sbyte>(1, 0)
                }, {
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(1, 1)
                }, {
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(-1, 1),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(0, 0)
                }
            },
            offsetTable = jlstzOffsetTable,
            type = CellType.Z
        };
        public static Tetrimino I = new Tetrimino() {
            states = new Pair<sbyte>[,] {
                {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(2, 0)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(0, 2)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(-2, 0)
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(0, -2)
                }
            },
            offsetTable = iOffsetTable,
            type = CellType.I
        };
        public static Tetrimino O = new Tetrimino() {
            states = new Pair<sbyte>[,] {
                {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(1, 0),
                    new Pair<sbyte>(1, -1),
                }, {
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(1, 1),
                    new Pair<sbyte>(1, 0),
                }, {
                    new Pair<sbyte>(-1, 1),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(0, 1),
                    new Pair<sbyte>(0, 0),
                }, {
                    new Pair<sbyte>(0, 0),
                    new Pair<sbyte>(0, -1),
                    new Pair<sbyte>(-1, 0),
                    new Pair<sbyte>(-1, -1),
                }
            },
            offsetTable = oOffsetTable,
            type = CellType.O
        };
    }
    //IMPORTANT The Rust cell_type output gets directly converted to this enum, update if either changes
    public enum CellType: byte 
    {
        EMPTY,
        GARBAGE,
        SOLID,
        J,
        L,
        S,
        T,
        Z,
        I,
        O,
        Count // Hack for getting the length of the enum
    }
    public struct Pair<T>
    {
        public T x { get; }
        public T y { get; }
        public Pair(T x, T y) {
            this.x = x;
            this.y = y;
        }
    }
}
