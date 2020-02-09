#[macro_use]
extern crate enum_primitive;
#[macro_use]
extern crate lazy_static;
mod tetrimino;
use tetrimino::{ CellType, Tetrimino };
use num::FromPrimitive;

const START_PIECE_X: i32 = 4;
const START_PIECE_Y: i32 = 19;
const START_PIECE_ROT: i32 = 0;

pub struct HardDropResult {
    pub block_out: bool,
    pub lines_cleared: i32
}

pub struct PieceState {
    pub x: i32,
    pub y: i32,
    pub r: i32
}

pub struct Board {
    pub grid: [[CellType; 40]; 10],
    pub current: &'static Tetrimino,
    pub hold: Option<&'static Tetrimino>,
    pub piece_x: i32,
    pub piece_y: i32,
    pub piece_rot: i32,
    pub held: bool
}

impl Board {
    pub fn new(start_piece: &'static Tetrimino) -> Board {
        let mut board = Board {
            grid: [[CellType::EMPTY; 40]; 10],
            current: start_piece,
            hold: None,
            piece_x: 0,
            piece_y: 0,
            piece_rot: 0,
            held: false
        };
        board.set_piece(start_piece);
        board
    }
    pub fn clone(source: &Board) -> Board {
        Board {
            grid: source.grid.clone(),
            current: source.current,
            hold: source.hold.clone(),
            piece_x: source.piece_x,
            piece_y: source.piece_y,
            piece_rot: source.piece_rot,
            held: source.held
        }
    }
    pub fn is_out_of_bounds(x: i32, y: i32) -> bool {
        x < 0 || x >= 10 || y < 0 || y >= 40
    }
    pub fn hold_piece(&mut self, next: &'static Tetrimino) -> bool {
        if self.held {
            false
        } else {
            self.held = true;
            let temp = self.current;
            self.set_piece(match self.hold {
                Some(hold) => hold,
                None => next
            });
            self.hold.replace(temp);
            true
        }
    }
    pub fn get_cell(&self, x: i32, y: i32) -> CellType {
        if Self::is_out_of_bounds(x, y) {
            CellType::SOLID
        } else {
            self.grid[x as usize][y as usize]
        }
    }
    pub fn set_cell(&mut self, x: i32, y: i32, cell: CellType) {
        if !Self::is_out_of_bounds(x, y) {
            self.grid[x as usize][y as usize] = cell;
        }
    }
    pub fn hard_drop(&mut self, next: &'static Tetrimino) -> HardDropResult {
        while self.try_move(self.piece_x, self.piece_y + 1, self.piece_rot) { }
        for block in &self.current.states[self.piece_rot as usize] {
            self.set_cell(self.piece_x + block.x, self.piece_y +  block.y, self.current.cell_type);
        }
        let mut new_board = [[CellType::EMPTY; 40]; 10];
        let mut lines_cleared: i32 = 0;
        for y in (0..40).rev() {
            let mut row_filled = true;
            for x in 0..10 {
                if self.grid[x][y] == CellType::EMPTY {
                    row_filled = false;
                    break;
                }
            }
            if row_filled {
                lines_cleared += 1;
            } else {
                let new_y = y + (lines_cleared as usize);
                for x in 0..10 {
                    new_board[x][new_y] = self.grid[x][y];
                }
            }
        }
        self.grid = new_board;
        self.set_piece(next);
        self.held = false;
        HardDropResult {
            block_out: !self.piece_fits(self.piece_x, self.piece_y, self.piece_rot),
            lines_cleared: lines_cleared
        }
    }
    pub fn set_piece(&mut self, piece: &'static Tetrimino){
        self.current = piece;
        self.piece_x = START_PIECE_X;
        self.piece_y = START_PIECE_Y;
        self.piece_rot = START_PIECE_ROT;
        self.try_move(self.piece_x, self.piece_y + 1, self.piece_rot);
    }
    pub fn piece_fits(&self, x: i32, y: i32, rot: i32) -> bool {
        for block in &self.current.states[rot as usize] {
            if self.get_cell(x + block.x, y +  block.y) != CellType::EMPTY {
                return false;
            }
        }
        true
    }
    pub fn move_left(&mut self) -> bool {
        self.try_move(self.piece_x - 1, self.piece_y, self.piece_rot)
    }
    pub fn move_right(&mut self) -> bool {
        self.try_move(self.piece_x + 1, self.piece_y, self.piece_rot)
    }
    pub fn soft_drop(&mut self) -> bool {
        self.try_move(self.piece_x, self.piece_y + 1, self.piece_rot)
    }
    pub fn turn_left(&mut self) -> bool {
        self.rotate(if self.piece_rot > 0 { self.piece_rot - 1 } else { 3 })
    }
    pub fn turn_right(&mut self) -> bool {
        self.rotate(if self.piece_rot < 3 { self.piece_rot + 1 } else { 0 })
    }
    fn rotate(&mut self, rot: i32) -> bool {
        let len = self.current.offset_table[0].len();
        for i in 0..len {
            let from_offset = &self.current.offset_table[self.piece_rot as usize][i];
            let to_offset = &self.current.offset_table[rot as usize][i];
            let x = self.piece_x + from_offset.x - to_offset.x;
            let y = self.piece_y - (from_offset.y - to_offset.y);
            if self.try_move(x, y, rot) {
                return true;
            }
        }
        false
    }
    fn try_move(&mut self, x: i32, y: i32, rot: i32) -> bool {
        if self.piece_fits(x, y, rot) {
            self.piece_x = x;
            self.piece_y = y;
            self.piece_rot = rot;
            true
        } else {
            false
        }
    }
}
#[no_mangle]
pub extern fn create(t: u8) -> *mut Board {
    unsafe {
        std::mem::transmute(Box::new(Board::new(
            tetrimino::byte_to_tetrimino(t)
        )))
    }
}
#[no_mangle]
pub extern fn clone(board_ptr: *mut Board) -> *mut Board {
    let board = unsafe { &mut *board_ptr };
    unsafe {
        std::mem::transmute(Box::new(Board::clone(
            board
        )))
    }
}
#[no_mangle]
pub extern fn destroy(board_ptr: *mut Board) {
    let _board: Box<Board> = unsafe { std::mem::transmute(board_ptr) };
    // Drop
}
#[no_mangle]
pub extern fn get_cell(board_ptr: *mut Board, x: i32, y: i32) -> u8 {
    let board = unsafe { &mut *board_ptr };
    board.get_cell(x, y) as u8
}
#[no_mangle]
pub extern fn set_cell(board_ptr: *mut Board, x: i32, y: i32, cell: u8) {
    let board = unsafe { &mut *board_ptr };
    board.set_cell(x, y, CellType::from_u8(cell).unwrap());
}
#[no_mangle]
pub extern fn hard_drop(board_ptr: *mut Board, next: u8) -> HardDropResult {
    let board = unsafe { &mut *board_ptr };
    board.hard_drop(tetrimino::byte_to_tetrimino(next))
}
#[no_mangle]
pub extern fn piece_fits(board_ptr: *mut Board, rot: i32, x: i32, y: i32) -> bool {
    let board = unsafe { &mut *board_ptr };
    board.piece_fits(x, y, rot)
}
#[no_mangle]
pub extern fn move_left(board_ptr: *mut Board) -> PieceState {
    let board = unsafe { &mut *board_ptr };
    board.move_left();
    PieceState {
        x: board.piece_x,
        y: board.piece_y,
        r: board.piece_rot
    }
}
#[no_mangle]
pub extern fn move_right(board_ptr: *mut Board) -> PieceState {
    let board = unsafe { &mut *board_ptr };
    board.move_right();
    PieceState {
        x: board.piece_x,
        y: board.piece_y,
        r: board.piece_rot
    }
}
#[no_mangle]
pub extern fn turn_left(board_ptr: *mut Board) -> PieceState {
    let board = unsafe { &mut *board_ptr };
    board.turn_left();
    PieceState {
        x: board.piece_x,
        y: board.piece_y,
        r: board.piece_rot
    }
}
#[no_mangle]
pub extern fn turn_right(board_ptr: *mut Board) -> PieceState {
    let board = unsafe { &mut *board_ptr };
    board.turn_right();
    PieceState {
        x: board.piece_x,
        y: board.piece_y,
        r: board.piece_rot
    }
}
#[no_mangle]
pub extern fn soft_drop(board_ptr: *mut Board) -> PieceState {
    let board = unsafe { &mut *board_ptr };
    board.soft_drop();
    PieceState {
        x: board.piece_x,
        y: board.piece_y,
        r: board.piece_rot
    }
}
#[no_mangle]
pub extern fn hold_piece(board_ptr: *mut Board, next: u8) -> bool {
    let board = unsafe { &mut *board_ptr };
    board.hold_piece(tetrimino::byte_to_tetrimino(next))
}
#[no_mangle]
pub extern fn set_piece(board_ptr: *mut Board, piece: u8) -> PieceState {
    let board = unsafe { &mut *board_ptr };
    board.set_piece(tetrimino::byte_to_tetrimino(piece));
    PieceState {
        x: board.piece_x,
        y: board.piece_y,
        r: board.piece_rot
    }
}
#[no_mangle]
pub extern fn set_piece_state(board_ptr: *mut Board, x: i32, y: i32, rot: i32) {
    let mut board = unsafe { &mut *board_ptr };
    board.piece_x = x;
    board.piece_y = y;
    board.piece_rot = rot;
}
