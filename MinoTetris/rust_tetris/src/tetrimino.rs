extern crate num;
use num::FromPrimitive;

pub struct Pair<T> {
    pub x: T,
    pub y: T
}
enum_from_primitive! {
    #[repr(u8)]
    #[derive(Copy, Clone, PartialEq)]
    pub enum CellType {
        EMPTY,
        GARBAGE,
        SOLID,
        J,
        L,
        S,
        T,
        Z,
        I,
        O
    }
}
pub struct Tetrimino {
    pub states: [[Pair<i32>; 4]; 4],
    pub cell_type: CellType,
    pub offset_table: &'static [Vec<Pair<i32>>; 4]
}
lazy_static! {
    static ref JLSTZ_OFFSET_TABLE: [Vec<Pair<i32>>; 4] = [
        vec![
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 }
        ], vec![
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 1, y: 0 },
            Pair::<i32> { x: 1, y: -1 },
            Pair::<i32> { x: 0, y: 2 },
            Pair::<i32> { x: 1, y: 2 }
        ], vec![
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 }
        ], vec![
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: -1, y: 0 },
            Pair::<i32> { x: -1, y: -1 },
            Pair::<i32> { x: 0, y: 2 },
            Pair::<i32> { x: -1, y: 2 }
        ]
    ];
    static ref O_OFFSET_TABLE: [Vec<Pair<i32>>; 4] = [
        vec![
            Pair::<i32> { x: 0, y: 0 },
        ], vec![
            Pair::<i32> { x: 0, y: -1 },
        ], vec![
            Pair::<i32> { x: -1, y: -1 },
        ], vec![
            Pair::<i32> { x: -1, y: 0 },
        ]
    ];
    static ref I_OFFSET_TABLE: [Vec<Pair<i32>>; 4] = [
        vec![
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: -1, y: 0 },
            Pair::<i32> { x: 2, y: 0 },
            Pair::<i32> { x: -1, y: 0 },
            Pair::<i32> { x: 2, y: 0 }
        ], vec![
            Pair::<i32> { x: -1, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 0 },
            Pair::<i32> { x: 0, y: 1 },
            Pair::<i32> { x: 0, y: -2 }
        ], vec![
            Pair::<i32> { x: -1, y: 1 },
            Pair::<i32> { x: 1, y: 1 },
            Pair::<i32> { x: -2, y: 1 },
            Pair::<i32> { x: 1, y: 0 },
            Pair::<i32> { x: -2, y: 0 }
        ], vec![
            Pair::<i32> { x: 0, y: 1 },
            Pair::<i32> { x: 0, y: 1 },
            Pair::<i32> { x: 0, y: 1 },
            Pair::<i32> { x: 0, y: -1 },
            Pair::<i32> { x: 0, y: 2 }
        ]
    ];
    pub static ref J: Tetrimino = Tetrimino {
        states: [
            [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: -1, y: -1 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 1, y: -1 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: 1, y: 1 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: -1, y: 1 }
            ]
        ],
        offset_table: &JLSTZ_OFFSET_TABLE,
        cell_type: CellType::J
    };
    pub static ref L: Tetrimino = Tetrimino {
        states: [
            [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: 1, y: -1 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 1, y: 1 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: -1, y: 1 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: -1, y: -1 }
            ]
        ],
        offset_table: &JLSTZ_OFFSET_TABLE,
        cell_type: CellType::L
    };
    pub static ref S: Tetrimino = Tetrimino {
        states: [
            [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 1, y: -1 },
                Pair::<i32> { x: -1, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 1, y: 1 },
                Pair::<i32> { x: 1, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: -1, y: 1 }
            ], [
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: -1, y: -1 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 0, y: 0 }
            ]
        ],
        offset_table: &JLSTZ_OFFSET_TABLE,
        cell_type: CellType::S
    };
    pub static ref T: Tetrimino = Tetrimino {
        states: [
            [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: -1, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: 0, y: 1 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: -1, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 0, y: 1 }
            ]
        ],
        offset_table: &JLSTZ_OFFSET_TABLE,
        cell_type: CellType::T
    };
    pub static ref Z: Tetrimino = Tetrimino {
        states: [
            [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: -1, y: -1 },
                Pair::<i32> { x: 1, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 1, y: -1 },
                Pair::<i32> { x: 1, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 1, y: 1 }
            ], [
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: -1, y: 1 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 0, y: 0 }
            ]
        ],
        offset_table: &JLSTZ_OFFSET_TABLE,
        cell_type: CellType::Z
    };
    pub static ref I: Tetrimino = Tetrimino {
        states: [
            [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: 2, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 0, y: 2 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: -2, y: 0 }
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 0, y: -2 }
            ]
        ],
        offset_table: &I_OFFSET_TABLE,
        cell_type: CellType::I
    };
    pub static ref O: Tetrimino = Tetrimino {
        states: [
            [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: 1, y: 0 },
                Pair::<i32> { x: 1, y: -1 },
            ], [
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 1, y: 1 },
                Pair::<i32> { x: 1, y: 0 },
            ], [
                Pair::<i32> { x: -1, y: 1 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: 0, y: 1 },
                Pair::<i32> { x: 0, y: 0 },
            ], [
                Pair::<i32> { x: 0, y: 0 },
                Pair::<i32> { x: 0, y: -1 },
                Pair::<i32> { x: -1, y: 0 },
                Pair::<i32> { x: -1, y: -1 },
            ]
        ],
        offset_table: &O_OFFSET_TABLE,
        cell_type: CellType::O
    };
}
pub fn byte_to_tetrimino(t: u8) -> &'static Tetrimino {
    match CellType::from_u8(t).unwrap() {
        CellType::J => &J,
        CellType::L => &L,
        CellType::S => &S,
        CellType::T => &T,
        CellType::Z => &Z,
        CellType::I => &I,
        CellType::O => &O,
        _ => unreachable!()
    }
}