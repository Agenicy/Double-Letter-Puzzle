//#define StopWhenFindResult
//#define Transposition
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

			Debug.Log(result);
			Debug.Log(tree);
		}
	}

	class Node
	{
		string value;
		string ID;

#if DependencyBased
		int dependencyIndex;
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

#if DependencyBased
		static Dictionary<string, List<string>> DependencyBasedTable = new Dictionary<string, List<string>>()
		{
			{ "a", new List<string>(){ "aa", "bb", "ee" } },
			{ "b", new List<string>(){ "aa", "bb", "cc" } },
			{ "c", new List<string>(){ "bb", "cc", "dd" } },
			{ "d", new List<string>(){ "cc", "dd", "ee" } },
			{ "e", new List<string>(){ "aa", "dd", "ee" } }
		};
#endif

		public List<Node> Children = new List<Node>();
		public bool isLeaf => Children.Count == 0;

		public Node(string input, Node parent = null, int index = 0
#if DependencyBased
			, int dependencyIndex = 0, string strThatChanged = null
#endif
			)
		{
			value = input;
			if (parent is null)
				ID = $"{index}";
			else
				ID = $"{parent.ID}-{index}";
			Debug.Log($"{ID} {value}");

#if Transposition
			Tree.Transposition[value] = this;
#endif

#if DependencyBased
			this.dependencyIndex = dependencyIndex;
			this.strThatChanged = strThatChanged;
			Debug.Log($"dependencyIndex = {dependencyIndex}");
#endif
		}

		public void Spand(out bool result)
		{
			result = false;

			Debug.Log($"{value} Spanding");
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

			for (int i = Math.Max(dependencyIndex - front, 0); i < Math.Min(value.Length, dependencyIndex + last); i++)
#else
			for (int i = 0; i < value.Length; i++)
#endif
			{
#if DependencyBased
				List<string> selectable = new List<string>();
				if (strThatChanged != null)
					selectable = DependencyBasedTable[strThatChanged];
				else
					foreach (var item in transitionTable.Keys)
					{
						selectable.Add(item);
					}
#else
				var selectable = transitionTable.Keys;
#endif

				foreach (var keyword in selectable)
				{
					if (i + keyword.Length <= value.Length)
						if (value.Substring(i, keyword.Length) == keyword)
						{
							foreach (var replacement in transitionTable[keyword])
							{
								++counter;
								string childValue = value.Substring(0, i) +
										replacement +
										value.Substring(i + keyword.Length);

#if Transposition
								if (Tree.Transposition.ContainsKey(childValue))
								{
									Debug.Log($"Pass {childValue}");
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
										, i, replacement
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

		public override string ToString()
		{
			return $"{ID} {value}";
		}
	}

	class Tree
	{
		public Node root;

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
				ret += $"{item} /";
			}
			return ret;
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
