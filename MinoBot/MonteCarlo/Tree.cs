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
        public Func<TetrisState, TetriminoState, float> evaluator;
        // Takes in a leaf node and expands it (add children)
        public Action<Node> expander;
        public Tree(TetrisState state) {
            Reset(state);
        }
        public void Think() {
            Node SelectNode(Node parent) {
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
            Node node = SelectNode(root);
            if (node == null) return;
            expander(node);
            if (node.IsLeaf()) {
                node.state.Finished(true);
                return;
            }
            foreach (Node c in node.children) {
                Node n = c;
                float score = evaluator(n.state, n.move);
                while (true) {
                    n.score += score;
                    n.simulations += 1;
                    if (n.IsRoot()) break;
                    n = n.parent;
                }
            }
        }
        public void Reset(TetrisState state) {
            #if POOLING
            void ReturnNode(Node node) {
                foreach (Node child in node.children) {
                    ReturnNode(child);
                }
                NodePool.standard.Return(node);
            }
            #endif
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
            #if POOLING
            void ReturnNode(Node node) {
                foreach (Node child in node.children) {
                    ReturnNode(child);
                }
                NodePool.standard.Return(node);
            }
            #endif
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
