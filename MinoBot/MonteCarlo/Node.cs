using System;
using System.Collections.Generic;
using System.Text;

namespace MinoBot.MonteCarlo
{
    public class Node
    {
        public TetrisState state;
        public TetriminoState move;
        public Node parent;
        public List<Node> children;
        public float score;
        public int simulations = 0;
        public Node(TetrisState state) {
            this.state = state;
            children = new List<Node>();
        }
        public bool IsLeaf() {
            return children.Count == 0;
        }
        public bool IsRoot() {
            return parent == null;
        }
        public void Reset() {
            parent = null;
            children.Clear();
            score = 0;
            simulations = 0;
        }
    }
}
