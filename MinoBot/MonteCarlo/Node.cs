using System;
using System.Collections.Concurrent;
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
        public int simulations;
        public int depth;
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
            state = null;
            parent = null;
            children.Clear();
            score = 0;
            simulations = 0;
        }
    }
    public class NodePool
    {
        public static NodePool standard = new NodePool();
        private ConcurrentBag<Node> pool = new ConcurrentBag<Node>();
        public Node Rent(TetrisState state) {
            if (pool.TryTake(out Node node)) {
                node.state = state;
                return node;
            }
            return new Node(state);
        }
        public void Return(Node node) {
            node.Reset();
            pool.Add(node);
        }
    }
}
