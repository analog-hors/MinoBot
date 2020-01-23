using System;
using System.Collections.Generic;
using System.Text;

namespace MinoBot.MonteCarlo
{
    public class Node<TState, TMove> where TState: State<TState, TMove>
    {
        public State<TState, TMove> state;
        public TMove move;
        public Node<TState, TMove> parent;
        public HashSet<Node<TState, TMove>> children;
        public float score;
        public int simulations = 1;
        public Node(State<TState, TMove> state) {
            this.state = state;
            children = new HashSet<Node<TState, TMove>>();
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
            simulations = 1;
        }
    }
    public interface State<TState, TMove> where TState: State<TState, TMove>
    {
        TState DoMove(TMove move);
        TState GetSelf();
        bool Finished();
    }
}
