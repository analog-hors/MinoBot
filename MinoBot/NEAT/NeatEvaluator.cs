using MinoBot.MonteCarlo;
using MinoTetris;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinoBot.NEAT
{
    public class NeatEvaluator
    {
        private IBlackBox brain;
        public NeatEvaluator(IBlackBox brain) {
            this.brain = brain;
        }
        public float Evaluate(TetrisState state, TetriminoState move) {
            brain.ResetState();
            for (int x = 0; x < 10; x++) {
                for (int y = 0; y < 20; x++) {
                    brain.InputSignalArray[y * 10 + x] =
                        state.tetris.GetCell(x, y) == CellType.EMPTY
                            ? 0
                            : 1;
                }
            }
            brain.Activate();
            return (float) brain.OutputSignalArray[0];
        }
    }
}
