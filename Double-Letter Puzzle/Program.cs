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
			tree.root.Spand();

			Debug.Log(tree);
		}
	}

	class Node
	{
		string value;
		string ID;

		static Dictionary<string, List<string>> transitionTable = new Dictionary<string, List<string>>()
		{
			{ "aa", new List<string>(){ "b", "e" } },
			{ "bb", new List<string>(){ "a", "c" } },
			{ "cc", new List<string>(){ "b", "d" } },
			{ "dd", new List<string>(){ "c", "e" } },
			{ "ee", new List<string>(){ "d", "a" } }
		};

		public List<Node> Children = new List<Node>();
		public bool isLeaf => Children.Count == 0;

		public Node(string input, Node parent = null, int index = 0)
		{
			value = input;
			if (parent is null)
				ID = $"{index}";
			else
				ID = $"{parent.ID}-{index}";
			Debug.Log($"{ID} {value}");
		}

		public void Spand()
		{
			Debug.Log($"{value} Spanding");

			bool canMerge = false;
			for (int i = 0; i < value.Length; i++)
			{
				foreach (var keyword in transitionTable.Keys)
				{
					int counter = 0;
					if (i + keyword.Length <= value.Length)
						if (value.Substring(i, keyword.Length) == keyword)
						{
							foreach (var replacement in transitionTable[keyword])
							{
								Children.Add(
									new Node(
										value.Substring(0, i) +
										replacement +
										value.Substring(i + keyword.Length),
										this,
										counter++
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
					child.Spand();
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

		public Dictionary<string, Node> Transposition = new Dictionary<string, Node>();

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
