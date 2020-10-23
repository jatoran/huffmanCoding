using System;
using System.Collections.Generic;
using System.Linq;

namespace huffmanCoding {

    public enum NodePosition {
        left,
        right,
        center
    }

    public class TreeNode {
        public string title;
        public int priority;
        public TreeNode leftChild;
        public TreeNode rightChild;
        public string bitString = "";

        public TreeNode(string _title, int _priority, TreeNode _leftChild = null, TreeNode _rightChild = null) {
            title = _title;
            priority = _priority;
            leftChild = _leftChild;
            rightChild = _rightChild;
        }

       
        //PRINTING FUNCTIONS
        private void PrintValue(string value, NodePosition nodePostion) {
            switch (nodePostion) {
                case NodePosition.left:
                    PrintLeftValue(value);
                    break;
                case NodePosition.right:
                    PrintRightValue(value);
                    break;
                case NodePosition.center:
                    Console.WriteLine(value);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void PrintLeftValue(string value) {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("L:");
            Console.ForegroundColor = (value == "-") ? ConsoleColor.Red : ConsoleColor.Gray;
            Console.WriteLine(value);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void PrintRightValue(string value) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("R:");
            Console.ForegroundColor = (value == "-") ? ConsoleColor.Red : ConsoleColor.Gray;
            Console.WriteLine(value);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void PrintPretty(string indent, NodePosition nodePosition, bool last, bool empty) {

            Console.Write(indent);
            if (last) {
                Console.Write("└─");
                indent += "  ";
            }
            else {
                Console.Write("├─");
                indent += "| ";
            }

            var stringValue = empty ? "-" : title.ToString();
            PrintValue(stringValue, nodePosition);

            if (!empty && (this.leftChild != null || this.rightChild != null)) {
                if (this.leftChild != null)
                    this.leftChild.PrintPretty(indent, NodePosition.left, false, false);
                else
                    PrintPretty(indent, NodePosition.left, false, true);

                if (this.rightChild != null)
                    this.rightChild.PrintPretty(indent, NodePosition.right, true, false);
                else
                    PrintPretty(indent, NodePosition.right, true, true);
            }
        }
    }

    public class Heap{
        public List<TreeNode> elements = new List<TreeNode>();

        public TreeNode GetMin() {
            return this.elements.Count > 0 ? this.elements[0] : default;
        }

        private int GetParent(int index) {
            if (index <= 0) {
                return -1;
            }
            return (index - 1) / 2;
        }

        public void Add(TreeNode item) {
            elements.Add(item);
            this.HeapifyUp(elements.Count - 1);
        }

        public TreeNode PopMin() {
            if (elements.Count > 0) {
                TreeNode item = elements[0];
                elements[0] = elements[elements.Count - 1];
                elements.RemoveAt(elements.Count - 1);

                this.HeapifyDown(0);
                return item;
            }

            throw new InvalidOperationException("no element in heap loser - ps. learn to program");
        }

        public void HeapifyUp(int index) {
            var parent = this.GetParent(index);
            if (parent >= 0 && (elements[index].priority < elements[parent].priority)) {
                var temp = elements[index];
                elements[index] = elements[parent];
                elements[parent] = temp;

                this.HeapifyUp(parent);
            }
        }

        private void HeapifyDown(int index) {
            var smallest = index;

            var left = (2 * index) + 1;
            var right = (2 * index) + 2;

            if (left < this.elements.Count && elements[left].priority < elements[index].priority)
                smallest = left;

            if (right < this.elements.Count && elements[right].priority < elements[smallest].priority)
                smallest = right;

            if (smallest != index) {
                var temp = elements[index];
                elements[index] = elements[smallest];
                elements[smallest] = temp;

                this.HeapifyDown(smallest);
            }
        }

        public void Print() {
            this.GetMin().PrintPretty("", NodePosition.center, true, false);
        }
    }

    class Program {
        static void Main(string[] args) {
            string defaultInput = "aabbbccccddddeeeeeffffffggggggggggggggggggggggggggggggggggggggggggg";
            Dictionary<char, int> CharToCount = new Dictionary<char, int>();
            Dictionary<char, string> CharToHuffcode = new Dictionary<char, string>();

            //COUNT CHARACTERS - STORE IN "CharToCount"
            foreach (char c in defaultInput) {
                if (CharToCount.ContainsKey(c)) {
                    CharToCount[c]++;
                }
                else CharToCount.Add(c, 1);
            }

            //CONVERT "CharToCount" ELEMENTS TO TREENODES
            //ADD TREENODES TO MINHEAP(PRIORITY QUEUE)
            Heap heap = new Heap();
            foreach (KeyValuePair<char, int> k in CharToCount) {
                heap.Add(new TreeNode(k.Key.ToString(), k.Value));
            }

            //Print Counts per Char
            string test = "Counts: ";
            foreach (TreeNode t in heap.elements) {
                test += t.title + " " + t.priority + " | ";

            }
            Console.WriteLine(test);
            Console.WriteLine();

            //REDUCE MINHEAP(PRIORITY QUEUE) TO SINGLE NODE CONTAINING HUFFMAN TREE
            while (heap.elements.Count > 1) {
                var oldNode1 = heap.GetMin();
                heap.PopMin();

                var oldNode2 = heap.GetMin();
                heap.PopMin();

                heap.Add(new TreeNode(oldNode1.title + oldNode2.title, oldNode1.priority + oldNode2.priority, oldNode1, oldNode2));
            }

            heap.Print();
            Console.WriteLine();

            //POPULATE "CharToHuffcode" WITH CHAR AND HUFFMAN CODE VALUES
            void buildHuffmanCode(TreeNode currentNode, string huffman = "") {
                if (currentNode.leftChild == null && currentNode.rightChild == null) {
                    CharToHuffcode.Add(currentNode.title[0], huffman);
                    return;
                }
                if (currentNode.leftChild != null) 
                    buildHuffmanCode(currentNode.leftChild, huffman + "0");
                if (currentNode.rightChild != null) 
                    buildHuffmanCode(currentNode.rightChild, huffman + "1");
            }
            
            buildHuffmanCode(heap.elements[0]);

            //sort elements by length of code
            var sortedDict = from entry in CharToHuffcode orderby entry.Value.Length ascending select entry;

            //PRINT SORTED CHAR/HUFFMAN CODES
            foreach (KeyValuePair<char, string> k in sortedDict) {
                Console.WriteLine(k.Key + " " + k.Value);
            }

            //PRINT SIZE COMPARISON
            Console.WriteLine("Original size: " + defaultInput.Length * 8 + " bits");
            int encodedSize = 0;
            foreach(char c in defaultInput) {
                if (CharToHuffcode.ContainsKey(c)) {
                    encodedSize += CharToHuffcode[c].Length;
                }
            }
            Console.WriteLine("Encoded size: " + encodedSize + " bits");
        }
    }
}