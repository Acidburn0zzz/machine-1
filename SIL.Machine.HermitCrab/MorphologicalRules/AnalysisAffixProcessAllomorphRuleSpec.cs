﻿using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.Matching;
using SIL.Machine.Rules;

namespace SIL.Machine.HermitCrab.MorphologicalRules
{
	public class AnalysisAffixProcessAllomorphRuleSpec : AnalysisMorphologicalTransformRuleSpec
	{
		private readonly AffixProcessAllomorph _allomorph;

		public AnalysisAffixProcessAllomorphRuleSpec(AffixProcessAllomorph allomorph)
			: base(allomorph.Lhs, allomorph.Rhs)
		{
			_allomorph = allomorph;
			Pattern.Acceptable = match => _allomorph.Lhs.Any(part => match.GroupCaptures.Captured(part.Name));
		}

		public override ShapeNode ApplyRhs(PatternRule<Word, ShapeNode> rule, Match<Word, ShapeNode> match, out Word output)
		{
			output = match.Input.Clone();
			GenerateShape(_allomorph.Lhs, output.Shape, match);
			return null;
		}
	}
}