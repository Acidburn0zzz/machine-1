﻿using System.Collections.Generic;
using System.Linq;

namespace SIL.APRE.Transduction
{
	public enum RuleOrder
	{
		Linear,
		Permutation,
		Combination
	}

	public abstract class RuleCascadeBase<TOffset> : IRule<TOffset>
	{
		private readonly List<IRule<TOffset>> _rules;
		private readonly RuleOrder _ruleOrder;

		protected RuleCascadeBase(RuleOrder ruleOrder)
			: this(ruleOrder, Enumerable.Empty<IRule<TOffset>>())
		{
		}

		protected RuleCascadeBase(RuleOrder ruleOrder, IEnumerable<IRule<TOffset>> rules)
		{
			_ruleOrder = ruleOrder;
			_rules = new List<IRule<TOffset>>(rules);
		}

		public RuleOrder RuleOrder
		{
			get { return _ruleOrder; }
		}

		public abstract bool IsApplicable(IBidirList<Annotation<TOffset>> input);

		protected void AddRuleInternal(IRule<TOffset> rule)
		{
			_rules.Add(rule);
		}

		public virtual bool Apply(IBidirList<Annotation<TOffset>> input)
		{
			return ApplyRules(input, 0);
		}

		private bool ApplyRules(IBidirList<Annotation<TOffset>> input, int ruleIndex)
		{
			bool applied = false;
			for (int i = ruleIndex; i < _rules.Count; i++)
			{
				if (_rules[i].Apply(input))
				{
					switch (_ruleOrder)
					{
						case RuleOrder.Permutation:
							ApplyRules(input, i);
							break;

						case RuleOrder.Combination:
							ApplyRules(input, 0);
							break;

					}
					applied = true;
				}
			}
			return applied;
		}
	}
}
