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
        // Takes in a leaf node, expands it (add children), then returns a child node.
        public Func<Node, Node> expander;
        public Tree(TetrisState state) {
            Reset(state);
        }
        public void Think() {
            Node SelectNode(Node parent) {
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
                if (maxNode == null) {
                    parent.state.Finished(true);
                    return parent.parent == null ? null : SelectNode(parent.parent);
                }
                return maxNode.IsLeaf() ? maxNode : SelectNode(maxNode);
            }
            Node node = root.IsLeaf() ? root : SelectNode(root);
            if (node == null) return;
            if (expander(node) == null) {
                node.state.Finished(true);
                return;
            }
            //node = child;
            foreach (Node child in node.children) {
                float score = evaluator(child.state, child.move);
                Node n = child;
                while (true) {
                    /*
                    if (node.simulations == 0 || node.score < score) {
                        node.score = score;
                    } else {
                        score = node.score;
                    }
                    */
                    n.score += score;
                    n.simulations += 1;
                    if (n.IsRoot()) break;
                    n = n.parent;
                }
            }
        }
        public void Reset(TetrisState state) {
            void ReturnNode(Node node) {
                foreach (Node child in node.children) {
                    ReturnNode(child);
                }
                NodePool.standard.Return(node);
            }
            if (root == null) {
                root = new Node(state);
            } else {
                foreach (Node child in root.children) {
                    ReturnNode(child);
                }
                root.Reset();
                root.state = state;
            }
        }
        public Node GetMove() {
            void ReturnNode(Node node) {
                foreach (Node child in node.children) {
                    ReturnNode(child);
                }
                NodePool.standard.Return(node);
            }
            Node maxNode = null;
            int maxSims = -1;
            foreach (Node node in root.children) {
                if (node.state.Finished()) continue;
                if (node.simulations > maxSims) {
                    if (maxNode != null) {
                        ReturnNode(maxNode);
                    }
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
