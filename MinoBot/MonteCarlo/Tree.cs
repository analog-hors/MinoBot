using System;
using System.Collections.Generic;
using System.Text;

namespace MinoBot.MonteCarlo
{
    public class Tree<TState, TMove> where TState : State<TState, TMove>
    {
        public Node<TState, TMove> root;
        // Scores a node, in which the highest scoring node is selected until a leaf node is found. 
        public Func<Node<TState, TMove>, float> selector;
        // Scores a node, the result of which will be used to backpropagate up the tree. 
        public Func<State<TState, TMove>, TMove, float> evaluator;
        // Takes in a leaf node, expands it (add children), then returns a child node.
        public Func<Node<TState, TMove>, Node<TState, TMove>> expander;
        public Tree(State<TState, TMove> state) {
            Reset(state);
        }
        public void Think() {
            Node<TState, TMove> SelectNode(Node<TState, TMove> parent) {
                if (parent.IsLeaf()) return parent;
                Node<TState, TMove> maxNode = null;
                float maxScore = 0;
                foreach (Node<TState, TMove> node in parent.children) {
                    if (node.state.Finished()) continue;
                    float score = selector(node);
                    if (score > maxScore || maxNode == null) {
                        maxScore = score;
                        maxNode = node;
                    }
                }
                return maxNode == null || maxNode.IsLeaf() ? maxNode : SelectNode(maxNode);
            }
            Node<TState, TMove> node = SelectNode(root);
            if (node == null) return;
            node = expander(node);
            float score = evaluator(node.state, node.move);
            while (true) {
                node.score += score;
                node.simulations += 1;
                if (node.IsRoot()) break;
                node = node.parent;
            }
        }
        public void Reset(State<TState, TMove> state) {
            root = new Node<TState, TMove>(state);
        }
        public Node<TState, TMove> GetMove() {
            Node<TState, TMove> maxNode = null;
            int maxSims = 0;
            foreach (Node<TState, TMove> node in root.children) {
                if (node.state.Finished()) continue;
                if (node.simulations > maxSims) {
                    maxSims = node.simulations;
                    maxNode = node;
                }
            }
            root = maxNode;
            root.parent = null;
            return root;
        }
    }
}
