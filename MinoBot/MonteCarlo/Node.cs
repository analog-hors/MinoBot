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
        /** Just for pooling purposes */
        public Node() {
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
        public static NodePool standard = new NodePool(2000);
        private readonly ConcurrentBag<Node> pool = new ConcurrentBag<Node>();
        public NodePool(int size) {
            #if POOLING
            while (size-- > 0) {
                pool.Add(new Node());
            }
            #endif
        }
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
