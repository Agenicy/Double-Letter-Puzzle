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
			tree.root.Spand(out result);

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
		string value;
		string ID;
		public static int nodeCount = 0;

#if DependencyBased
		int dependencyIndexMin;
		int dependencyIndexMax;
		string strThatChanged;
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
			, int dependencyIndexMin = 0, int dependencyIndexMax = 0, string strThatChanged = null
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
			Debug.Log($"(dependencyIndex = {dependencyIndexMin}, {dependencyIndexMax})");
#endif
		}

		public void Spand(out bool result)
		{
			Tree.SpreadingTimes++;

			result = false;

			Debug.Log($"[Spanding] {value}");
			if (value.Length == 1)
			{
				result = true;
#if StopWhenFindResult
				return;
#endif
			}

			bool canMerge = false;
			int counter = 0;

#if DependencyBased

			int front = 2; // 往前最長可能受影響的距離
			int last = 2;  // 往後最長可能受影響的距離

			int threatMin, threatMax;
			if (dependencyIndexMax - dependencyIndexMin <= 2)
			{
				threatMin = 0;
				threatMax = value.Length;
			}
			else
			{
				threatMin = Math.Max(dependencyIndexMin - front, 0);
				threatMax = Math.Min(value.Length, dependencyIndexMax + last);
			}

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
										, i, i + keyword.Length, replacement
#endif
									));
							}
							canMerge = true;
						}
				}
			}

			if (canMerge)
			{
				foreach (var child in Children)
				{
					bool res;
					child.Spand(out res);
					if (res)
					{
						result = res || result;
#if StopWhenFindResult
						return;
#endif
					}
				}
			}
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
	}

	class Debug
	{
		public static void Log(object obj)
		{
			Console.WriteLine(obj.ToString());
		}
	}
}
