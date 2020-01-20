using System;
using System.Collections.Generic;
using System.Text;

namespace MinoTetris
{
    public class TetrisRNG : TetrisRNGProvider
    {
        private Random rng;
        private int garbageHole;
        private Tetrimino[] currentBag;
        private Tetrimino[] futureBag;
        private int bagIndex;
        public TetrisRNG(int seed) {
            rng = new Random(seed);
            garbageHole = rng.Next(0, 10);
            currentBag = new Tetrimino[]{
                Tetrimino.J,
                Tetrimino.L,
                Tetrimino.S,
                Tetrimino.T,
                Tetrimino.Z,
                Tetrimino.I,
                Tetrimino.O
            };
            futureBag = new Tetrimino[]{
                Tetrimino.J,
                Tetrimino.L,
                Tetrimino.S,
                Tetrimino.T,
                Tetrimino.Z,
                Tetrimino.I,
                Tetrimino.O
            };
            Shuffle(currentBag);
            Shuffle(futureBag);
            bagIndex = 0;
        }
        public int NextGarbageHole() {
            if (rng.Next(0, 10) < 3) {
                garbageHole = rng.Next(0, 10);
            }
            return garbageHole;
        }
        public Tetrimino NextPiece() {
            if (bagIndex == currentBag.Length) {
                Tetrimino[] temp = currentBag;
                currentBag = futureBag;
                futureBag = temp;
                bagIndex = 0;
                Shuffle(futureBag);
            }
            return currentBag[bagIndex++];
        }
        public Tetrimino GetPiece(int index) {
            index += bagIndex;
            return index < currentBag.Length
                ? currentBag[index]
                : futureBag[index - currentBag.Length];
        }
        private void Shuffle(Tetrimino[] bag) {
            int n = bag.Length;
            while (n > 1) {
                int k = rng.Next(n--);
                Tetrimino temp = bag[n];
                bag[n] = bag[k];
                bag[k] = temp;
            }
        }
    }
}
