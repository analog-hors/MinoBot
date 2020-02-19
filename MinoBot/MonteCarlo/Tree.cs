//#define POOLING
using System;

namespace MinoBot.MonteCarlo
{
    public class Tree
    {
        public Node root;
        // Scores a node, in which the highest scoring node is selected until a leaf node is found. 
        public Func<Node, float> selector;
        // Scores a node, the result of which will be used to backpropagate up the tree. 
        public Func<Node, TetriminoState, (float, float)> evaluator;
        // Takes in a leaf node and expands it (add children)
        public Action<Node> expander;
        public Tree(TetrisState state) {
            Reset(state);
        }
        private Node SelectNode(Node parent) {
            if (parent.IsLeaf()) return parent;
            Node maxNode = null;
            float maxScore = 0;
            foreach (Node node in parent.children) {
                if (node.state.Finished()) continue;
                float score = selector(node);
                if (score > maxScore || maxNode == null) {
                    maxScore = score;
                    maxNode = node;
                }
            }
            return maxNode == null || maxNode.IsLeaf() ? maxNode : SelectNode(maxNode);
        }
        public void Think() {
            Node node = SelectNode(root);
            if (node == null) return;
            expander(node);
            if (node.IsLeaf()) {
                node.state.Finished(true);
                return;
            }
            foreach (Node c in node.children) {
                Node n = c;
                (float accumulated, float transient) = evaluator(n, n.move);
                n.score = transient;
                while (true) {
                    n.score += accumulated;
                    n.simulations += 1;
                    if (n.IsRoot()) break;
                    n = n.parent;
                }
            }
        }
        #if POOLING
        void ReturnNode(Node node) {
            foreach (Node child in node.children) {
                ReturnNode(child);
            }
            NodePool.standard.Return(node);
        }
        #endif
        public void Reset(TetrisState state) {
            if (root == null) {
                root = new Node(state);
            } else {
                #if POOLING
                foreach (Node child in root.children) {
                    ReturnNode(child);
                }
                #endif
                root.Reset();
                root.state = state;
            }
        }
        public Node GetMove() {
            Node maxNode = null;
            int maxSims = -1;
            foreach (Node node in root.children) {
                if (node.state.tetris.blockOut) continue;
                if (node.simulations > maxSims) {
                    #if POOLING
                    if (maxNode != null) {
                        ReturnNode(maxNode);
                    }
                    #endif
                    maxSims = node.simulations;
                    maxNode = node;
                }
            }
            root = maxNode;
            if (root != null) {
                root.parent = null;
            }
            return root;
        }
    }
}
