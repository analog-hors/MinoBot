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
            node = expander(node);
            float score = evaluator(node.state, node.move);
            while (true) {
                node.score += score;
                node.simulations += 1;
                if (node.IsRoot()) break;
                node = node.parent;
            }
        }
        public void Reset(TetrisState state) {
            root = new Node(state);
        }
        public Node GetMove() {
            Node maxNode = null;
            int maxSims = -1;
            foreach (Node node in root.children) {
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
