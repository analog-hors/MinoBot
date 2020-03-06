using MinoBot.MonteCarlo;
using MinoTetris;
using System;
using System.Collections.Generic;

namespace MinoBot.Evaluators
{
    public struct StandardEvaluatorWeights {
        public float holes;
        public float holesSquared;
        public float holeDepths;
        public float holeDepthsSquared;
        public float moveHeight;
        public float moveHeightSquared;
        public float maxHeight;
        public float maxHeightSquared;
        public float wastedT;
        public float wells;
        public float wellsSquared;
        public float spikes;
        public float spikesSquared;
        public float pieceFit;
        public float pieceFitSquared;
        public float tSlotTDiff;
        public float clear0;
        public float clear1;
        public float clear2;
        public float clear3;
        public float clear4;
        public float tspin1;
        public float tspin2;
        public float tspin3;
        public float mini1;
        public float mini2;
        public float perfectClear;
    }
    public class StandardEvaluator {
        public static StandardEvaluator standard = new StandardEvaluator();
        public bool logging = false;
        public StandardEvaluatorWeights weights = new StandardEvaluatorWeights {
            holes = 0,
            holesSquared = -1,
            holeDepths = 0,
            holeDepthsSquared = -0.5f,
            moveHeight = 0,
            moveHeightSquared = -1,
            maxHeight = -1,
            maxHeightSquared = 0,
            wastedT = -100,
            wells = 0,
            wellsSquared = -1,
            spikes = 0,
            spikesSquared = -1,
            pieceFit = 0,
            pieceFitSquared = 100,
            tSlotTDiff = -25,
            clear0 = 0,
            clear1 = 1,
            clear2 = 4,
            clear3 = 9,
            clear4 = 2500,
            tspin1 = 250,
            tspin2 = 2500,
            tspin3 = 5000,
            mini1 = 100,
            mini2 = 200,
            perfectClear = 5000,
        };
        private static Pattern wellPattern = new Pattern(new Pattern.CellPattern[] {
            new Pattern.CellPattern(-1, 0, true),
            new Pattern.CellPattern(-1, 1, true),
            new Pattern.CellPattern(1, 0, true),
            new Pattern.CellPattern(1, 1, true),
            new Pattern.CellPattern(0, 0, false),
            new Pattern.CellPattern(0, 1, false)
        });
        private static Pattern spikePattern = new Pattern(new Pattern.CellPattern[] {
            new Pattern.CellPattern(-1, 0, false),
            new Pattern.CellPattern(-1, 1, false),
            new Pattern.CellPattern(1, 0, false),
            new Pattern.CellPattern(1, 1, false),
            new Pattern.CellPattern(0, 0, true),
            new Pattern.CellPattern(0, 1, true)
        });
        public (float, float) Evaluate(Node node) {
            TetriminoState move = node.move;
            TetrisState state = node.state;
            TetrisState parentState = node.parent.state;

            int availibleTs = 0;
            for (int i = 0; true; i++) {
                Tetrimino tetrimino = parentState.tetris.rng.GetPiece(i);
                if (tetrimino == null) {
                    break;
                }
                if (tetrimino == Tetrimino.T) {
                    availibleTs += 1;
                }
            }
            if (logging) {
                Console.WriteLine($"{availibleTs} availible Ts.");
            }

            int totalFilledEdgeTiles = 0;
            HashSet<(int, int)> checkedCells = new HashSet<(int, int)>();
            void TestEdge(int x, int y) {
                if (checkedCells.Add((x, y)) && parentState.tetris.GetCell(x, y) != CellType.EMPTY) {
                    totalFilledEdgeTiles += 1;
                }
            }
            for (int i = 0; i < 4; i++) {
                Pair<sbyte> block = state.tetrimino.states[move.rot, i];
                int x = block.x + move.x;
                int y = block.y + move.y;
                TestEdge(x + 1, y);
                TestEdge(x - 1, y);
                TestEdge(x, y + 1);
                TestEdge(x, y - 1);
            }
            int totalEdgeTiles = checkedCells.Count - 4;
            int holes = 0;
            int holeDepths = 0;
            int holeDepthsSquared = 0;
            //int buriedHoles = 0;
            bool isPC = true;
            int wells = 0;
            int spikes = 0;
            int tslots = 0;
            int[] heights = new int[10];
            for (int x = 0; x < 10; x++) {
                for (int y = 20; y < 40; y++) {
                    if (state.tetris.GetCell(x, y) == CellType.EMPTY) {
                        if (39 - y < heights[x]) {
                            int depth = heights[x] - 39 + y;
                            holeDepths += depth;
                            holeDepthsSquared += depth * depth;
                            holes += 1;//Math.Max(1, heights[x] - 39 + y / 2);
                        }
                    } else {
                        heights[x] = 39 - y;
                        isPC = false;
                    }
                    if (wellPattern.Test(state.tetris, x, y) == 0) {
                        wells += 1;
                    }
                    if (spikePattern.Test(state.tetris, x, y) == 0) {
                        spikes += 1;
                    }
                    if (state.tetris.GetCell(x, y) == CellType.EMPTY) {
                        int tCorners = 0;
                        if (state.tetris.GetCell(x - 1, y - 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (state.tetris.GetCell(x + 1, y - 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (state.tetris.GetCell(x - 1, y + 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (state.tetris.GetCell(x + 1, y + 1) != CellType.EMPTY) {
                            tCorners += 1;
                        }
                        if (tCorners > 2) {
                            int tHoles = 0;
                            if (state.tetris.GetCell(x - 1, y) == CellType.EMPTY) {
                                tHoles += 1;
                            }
                            if (state.tetris.GetCell(x + 1, y) == CellType.EMPTY) {
                                tHoles += 1;
                            }
                            if (state.tetris.GetCell(x, y - 1) != CellType.EMPTY) {
                                tHoles = 0;
                                //tHoles += 1;
                            }
                            if (state.tetris.GetCell(x, y + 1) == CellType.EMPTY) {
                                tHoles += 1;
                            }
                            if (tHoles > 2) {
                                if (logging) {
                                    Console.WriteLine($"tslot at {x}, {y}");
                                }
                                tslots += 1;
                            }
                        }
                    }
                }
            }

            if (isPC) {
                return (weights.perfectClear, 0);
            }

            int maxHeight = 0;
            int minHeight = 0;
            int totalHeight = 0;
            for (int i = 0; i < heights.Length; i++) {
                int height = heights[i];
                if (height > maxHeight) {
                    maxHeight = height;
                }
                if (height < minHeight) {
                    minHeight = height;
                }
                totalHeight += height;
            }

            float accumulated = 0;
            float transient = 0;

            accumulated += holes * weights.holes;
            accumulated += holes * holes * weights.holesSquared;
            accumulated += holeDepths * weights.holeDepths;
            accumulated += holeDepthsSquared * weights.holeDepthsSquared;
            if (move.y <= 35) {
                int moveHeight = 39 - move.y;
                accumulated += moveHeight * weights.moveHeight;
                accumulated += moveHeight * moveHeight * weights.moveHeightSquared;
            }
            accumulated += maxHeight * weights.maxHeight;
            accumulated += maxHeight * weights.maxHeightSquared;
            accumulated += state.tetris.tspin switch {
                TspinType.MINI => state.tetris.linesCleared switch {
                    1 => weights.mini1,
                    2 => weights.mini2,
                    _ => 0,
                },
                TspinType.FULL => state.tetris.linesCleared switch {
                    1 => weights.tspin1,
                    2 => weights.tspin2,
                    3 => weights.tspin3,
                    _ => 0
                },
                _ => state.tetris.linesCleared switch {
                    1 => weights.clear1,
                    2 => weights.clear2,
                    3 => weights.clear3,
                    4 => weights.clear4,
                    _ => weights.clear0
                }
            };
            if ((state.tetris.tspin == TspinType.NONE || state.tetris.linesCleared == 0) && state.tetrimino == Tetrimino.T) {
                if (logging) {
                    Console.WriteLine("punished for wasting T");
                }
                accumulated += weights.wastedT;
            }
            accumulated += wells > 1 ? (wells * wells * weights.wellsSquared + (wells * weights.wells)) : 0;
            accumulated += spikes * weights.spikes;
            accumulated += spikes * spikes * weights.spikesSquared;
            float pieceFit = totalFilledEdgeTiles / ((float) totalEdgeTiles);
            if (logging) {
                Console.WriteLine($"{pieceFit * 100}% fit.");
            }
            accumulated += pieceFit * weights.pieceFit;
            accumulated += pieceFit * pieceFit * weights.pieceFitSquared;
            accumulated += Math.Abs(tslots - Math.Max(1, availibleTs)) * weights.tSlotTDiff;
            return (accumulated, transient);
        }

        private class Pattern
        {
            private CellPattern[] pattern;
            public Pattern(CellPattern[] pattern) {
                this.pattern = pattern;
            }
            public int Test(Tetris tetris, int x, int y) {
                int diff = 0;
                foreach (CellPattern cell in pattern) {
                    if (tetris.GetCell(x + cell.x, y + cell.y) != CellType.EMPTY == cell.filled) {
                        diff += 1;
                    }
                }
                return diff;
            }
            public struct CellPattern
            {
                public readonly sbyte x;
                public readonly sbyte y;
                public readonly bool filled;
                public CellPattern(sbyte x, sbyte y, bool filled) {
                    this.x = x;
                    this.y = y;
                    this.filled = filled;
                }
            }
        }
    }
}
