#define StopWhenFindResult
#define Transposition
#define DependencyBased

using System;
using System.Collections.Generic;



namespace Double_Letter_Puzzle
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Enter input: ");
			string quest = Console.ReadLine();

			Tree tree = new Tree(quest);
			bool result;

#if DependencyBased
			result = tree.Dependency(0) == quest.Length;
#else
			tree.root.Spand(out result);
#endif

			Debug.Log($"Result: {result}");
			Debug.Log($"Tree Spands {Tree.SpreadingTimes} times.");
			Debug.Log($"Total generated {Node.nodeCount} nodes.");
			Debug.Log("The tree graph is like:");
			Debug.Log(tree);
			Debug.Log(tree.TGF());
		}
	}

	class Node
	{
		public string value;
		string ID;
		public static int nodeCount = 0;

#if DependencyBased
		public int dependencyIndexMin;
		public int dependencyIndexMax;
		public string strThatChanged;
		public int indexCount;
#endif

		static Dictionary<string, List<string>> transitionTable = new Dictionary<string, List<string>>()
		{
			{ "aa", new List<string>(){ "b", "e" } },
			{ "bb", new List<string>(){ "a", "c" } },
			{ "cc", new List<string>(){ "b", "d" } },
			{ "dd", new List<string>(){ "c", "e" } },
			{ "ee", new List<string>(){ "a", "d" } }
		};

		public List<Node> Children = new List<Node>();
		public bool isLeaf => Children.Count == 0;

		public Node(string input, Node parent = null, int index = 0
#if DependencyBased
			, int dependencyIndexMin = 0, int dependencyIndexMax = short.MaxValue, string strThatChanged = null, int indexCount = 0
#endif
			)
		{
			nodeCount++;

			value = input;
			if (parent is null)
				ID = $"{index}";
			else
				ID = $"{parent.ID}-{index}";
			Debug.Log($"[{ID}]: {value}");

#if Transposition
			Tree.Transposition[value] = this;
#endif

#if DependencyBased
			this.dependencyIndexMin = dependencyIndexMin;
			this.dependencyIndexMax = dependencyIndexMax;
			this.strThatChanged = strThatChanged;
			this.indexCount = indexCount;
			Debug.Log($"(dependencyIndex = {dependencyIndexMin}, {indexCount})");

			if (!Tree.DependencyBasedTable.ContainsKey(indexCount))
				Tree.DependencyBasedTable[indexCount] = new List<Node>();
			Tree.DependencyBasedTable[indexCount].Add(this);
#endif
		}

		public int Spand(out bool result)
		{
			Tree.SpreadingTimes++;

			result = false;

			Debug.Log($"[Spanding] {value}");
			if (value.Length == 1)
			{
				result = true;
#if StopWhenFindResult
				return -1;
#endif
			}

			bool canMerge = false;
			int counter = 0;

#if DependencyBased

			const int front = 2; // 往前最長可能受影響的距離
			const int last = 2;  // 往後最長可能受影響的距離

			int threatMin, threatMax;

			threatMin = Math.Max(dependencyIndexMin - front, 0);
			threatMax = Math.Min(value.Length, dependencyIndexMax + last);


			for (int i = threatMin; i < threatMax; i++)
#else
			for (int i = 0; i < value.Length; i++)
#endif
			{
				foreach (var keyword in transitionTable.Keys)
				{
					if (i + keyword.Length <= value.Length)
						if (value.Substring(i, keyword.Length) == keyword)
						{
							foreach (var replacement in transitionTable[keyword])
							{

#if DependencyBased
								if (strThatChanged != null)
									if (!(replacement == strThatChanged || keyword.Contains(strThatChanged)))
										continue;
#endif
								++counter;
								string childValue = value.Substring(0, i) +
										replacement +
										value.Substring(i + keyword.Length);

#if Transposition
								if (Tree.Transposition.ContainsKey(childValue))
								{
									Debug.Log($"[Pass] {childValue}");
									continue;
								}
#endif

								Children.Add(
									new Node(
										childValue
										,
										this,
										counter
#if DependencyBased
										, i, i + keyword.Length, replacement, i + 1
#endif
									));
							}
							canMerge = true;
						}
				}
			}


#if DependencyBased
#else

			if (canMerge)
			{
				for (int i = 0; i < Children.Count; i++)
				{
					var child = Children[i];
					bool res;
					int index = child.Spand(out res);
					if (res)
					{
						result = res || result;
#if StopWhenFindResult
						return -1;
#endif
					}
				}
			}
#endif


			return -1;
		}


		public List<Node> ToList(ref List<Node> list)
		{
			if (isLeaf)
				list.Add(this);
			else
			{
				foreach (var child in Children)
				{
					child.ToList(ref list);
				}
			}
			return list;
		}

		public void TGF(string parentID, ref string node, ref string path)
		{
			node += $"{ID} {value}\n";

			if (parentID != "")
				path += $"{parentID} {ID}\n";

			if (!isLeaf)
			{
				foreach (var child in Children)
				{
					child.TGF(ID, ref node, ref path);
				}
			}
		}

		public override string ToString()
		{
			return $"{ID} {value}";
		}
	}

	class Tree
	{
		public Node root;
		public static int SpreadingTimes = 0;

#if Transposition
		public static Dictionary<string, Node> Transposition = new Dictionary<string, Node>();
#endif

#if DependencyBased
		public static Dictionary<int, List<Node>> DependencyBasedTable = new Dictionary<int, List<Node>>();
#endif

		public Tree(string input)
		{
			root = new Node(input);
		}

		public override string ToString()
		{
			List<Node> output = new List<Node>();
			root.ToList(ref output);
			string ret = "";
			foreach (var item in output)
			{
				ret += $"{item}\n";
			}
			return ret;
		}

		public string TGF()
		{
			string nodes = "";
			string paths = "";

			root.TGF("", ref nodes, ref paths);

			return nodes + "#\n" + paths;
		}



#if DependencyBased
		/// <summary>
		/// 在指定點開始展開1層
		/// </summary>
		/// <param name="index">指定點</param>
		/// <returns></returns>
		public int Dependency(int index)
		{
			Node parent;
			if (DependencyBasedTable.Count == 0)
				parent = root;
			else
			{
				parent = DependencyBasedTable[index][0];
				DependencyBasedTable[index].RemoveAt(0); // deque
			}

			bool end = false;
			int next = parent.Spand(out end);

			if (end)
				return next;

			foreach (var child in parent.Children)
				Combination(child);

			return 0;
		}

		public int Combination(Node parent)
		{
			int last = parent.indexCount;

			int index = 0;
			Queue<Node> queue = new Queue<Node>();
			if(DependencyBasedTable.ContainsKey(last))
			foreach (Node merge in DependencyBasedTable[last])
			{
				if (merge.strThatChanged == parent.strThatChanged)
				{
					Node newNode = new Node(
							parent.value.Substring(0, parent.indexCount) + merge.value.Substring(parent.indexCount),
							parent,
							index++,
							parent.dependencyIndexMin,
							merge.dependencyIndexMax,
							parent.strThatChanged,
							parent.indexCount + 1
					);

					parent.Children.Add(newNode);
					merge.Children.Add(newNode);
					queue.Enqueue(newNode);

					newNode.Spand(out bool _);
				}
			}
			return 0;
		}
#endif

	}

	class Debug
	{
		public static void Log(object obj)
		{
			Console.WriteLine(obj.ToString());
		}
	}
}
