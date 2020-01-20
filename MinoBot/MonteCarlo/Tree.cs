using System;
using System.Collections.Generic;
using System.Text;

namespace MinoBot.MonteCarlo
{
    public class Tree<TState, TMove> where TState: State<TMove>
    {
        public Node<TState, TMove> root;
        // Scores a node, in which the highest scoring node is selected until a leaf node is found. 
        public Func<Node<TState, TMove>, float> selector;
        // Scores a node, the result of which will be used to backpropagate up the tree. 
        public Func<TState, float> evaluator;
        // Takes in a leaf node, expands it (add children), then returns a child node.
        public Func<Node<TState, TMove>, Node<TState, TMove>> expander;
        public Tree(TState state) {
            root = new Node<TState, TMove>(state);
        }
        public void Think() {
            Node<TState, TMove> node = root;
            while (!node.IsLeaf()) {
                node = NextNode(node);
            }
            node = expander(node);
            node.simulations += 1;
            float score = evaluator(node.state);
            while (true) {
                node.score += score;
                if (node.IsRoot()) break;
                node = node.parent;
            }
        }
        public List<TMove> GetMove() {
            List<TMove> moves = new List<TMove>();
            Node<TState, TMove> node = root;
            while (!node.IsLeaf()) {
                node = NextNode(node); // Skips root node: Root nodes don't have moves anyways.
                moves.Add(node.move);
            }
            return moves;
        }
        private Node<TState, TMove> NextNode(Node<TState, TMove> parent) {
            Node<TState, TMove> maxNode = null;
            float maxScore = 0;
            foreach (Node<TState, TMove> node in parent.children) {
                float score = selector(node);
                if (score > maxScore || maxNode == null) {
                    maxScore = score;
                    maxNode = node;
                }
            }
            return maxNode;
        }
    }
}
