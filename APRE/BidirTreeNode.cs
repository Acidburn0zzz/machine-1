namespace SIL.APRE
{
	public class BidirTreeNode<TNode> : BidirListNode<TNode>, IBidirTreeNode<TNode> where TNode : BidirTreeNode<TNode>
	{
		private readonly BidirList<TNode> _children;
		private IBidirTree<TNode> _tree;

		public BidirTreeNode()
		{
			_children = new TreeBidirList((TNode) this);
		}

		public TNode Parent { get; private set; }

		public bool IsLeaf
		{
			get { return _children.Count == 0; }
		}

		public IBidirList<TNode> Children
		{
			get { return _children; }
		}

		public IBidirTree<TNode> Tree
		{
			get { return _tree; }
			
			internal set
			{
				_tree = value;
				foreach (TNode child in Children)
					child.Tree = value;
			}
		}

		protected internal override void Clear()
		{
			base.Clear();
			Parent = null;
		}

		protected internal override void Init(BidirList<TNode> list)
		{
			base.Init(list);
			TNode parent = ((TreeBidirList) list).Parent;
			Parent = parent;
			Tree = parent.Tree;
		}

		private class TreeBidirList : BidirList<TNode>
		{
			private readonly TNode _parent;

			public TreeBidirList(TNode parent)
			{
				_parent = parent;
			}

			public TNode Parent
			{
				get { return _parent; }
			}
		}
	}
}
