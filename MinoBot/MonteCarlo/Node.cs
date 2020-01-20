using System;
using System.Collections.Generic;
using System.Text;

namespace MinoBot.MonteCarlo
{
    public class Node<TState, TMove> where TState: State<TMove>
    {
        public TState state;
        public TMove move;
        public Node<TState, TMove> parent;
        public HashSet<Node<TState, TMove>> children;
        public float score;
        public int simulations;
        public Node(TState state) {
            this.state = state;
            children = new HashSet<Node<TState, TMove>>();
        }
        public bool IsLeaf() {
            return children.Count == 0;
        }
        public bool IsRoot() {
            return parent == null;
        }

    }
    public interface State<TMove>
    {
        State<TMove> DoMove(TMove move);
    }
}
