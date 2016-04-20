﻿using System.Linq;
using NUnit.Framework;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.HermitCrab;
using SIL.Machine.HermitCrab.MorphologicalRules;
using SIL.Machine.Matching;

namespace SIL.Machine.Translation.HermitCrab.Tests
{
	[TestFixture]
	public class HermitCrabTargetGeneratorTests : TranslationHermitCrabTestBase
	{
		[Test]
		public void GenerateWords_CanGenerate_ReturnsCorrectWord()
		{
			var any = FeatureStruct.New().Symbol(HCFeatureSystem.Segment).Value;

			var siPrefix = new AffixProcessRule
							{
								Name = "si_prefix",
								Gloss = "3SG",
								RequiredSyntacticFeatureStruct = FeatureStruct.New(Language.SyntacticFeatureSystem).Symbol("V").Value
							};
			Morphophonemic.MorphologicalRules.Add(siPrefix);
			siPrefix.Allomorphs.Add(new AffixProcessAllomorph
			                       	{
										Lhs = {Pattern<Word, ShapeNode>.New("1").Annotation(any).OneOrMore.Value},
										Rhs = {new InsertShape(Table3, "si+"), new CopyFromInput("1")}
			                       	});

			var edSuffix = new AffixProcessRule
							{
								Name = "ed_suffix",
								Gloss = "PAST",
								RequiredSyntacticFeatureStruct = FeatureStruct.New(Language.SyntacticFeatureSystem).Symbol("V").Value
							};
			Morphophonemic.MorphologicalRules.Add(edSuffix);
			edSuffix.Allomorphs.Add(new AffixProcessAllomorph
										{
											Lhs = {Pattern<Word, ShapeNode>.New("1").Annotation(any).OneOrMore.Value},
											Rhs = {new CopyFromInput("1"), new InsertShape(Table3, "+ɯd")}
										});

			var morpher = new Morpher(SpanFactory, TraceManager, Language);

			var targetGenerator = new HermitCrabTargetGenerator(GetMorphemeId, GetCategory, morpher);

			var analysis = new WordAnalysis(new[]
			{
				new MorphemeInfo("3SG", "V", "3SG", MorphemeType.Affix), 
				new MorphemeInfo("33", "V", "33", MorphemeType.Stem),
				new MorphemeInfo("PAST", "V", "PAST", MorphemeType.Affix)
			}, 1, "V");

			string[] words = targetGenerator.GenerateWords(analysis).ToArray();
			Assert.That(words, Is.EquivalentTo(new[] {"sisasɯd"}));
		}

		[Test]
		public void GenerateWords_CannotGenerate_ReturnsEmptyEnumerable()
		{
			var any = FeatureStruct.New().Symbol(HCFeatureSystem.Segment).Value;

			var edSuffix = new AffixProcessRule
							{
								Name = "ed_suffix",
								Gloss = "PAST",
								RequiredSyntacticFeatureStruct = FeatureStruct.New(Language.SyntacticFeatureSystem).Symbol("V").Value
							};
			Morphophonemic.MorphologicalRules.Add(edSuffix);
			edSuffix.Allomorphs.Add(new AffixProcessAllomorph
										{
											Lhs = {Pattern<Word, ShapeNode>.New("1").Annotation(any).OneOrMore.Value},
											Rhs = {new CopyFromInput("1"), new InsertShape(Table3, "+ɯd")}
										});

			var morpher = new Morpher(SpanFactory, TraceManager, Language);

			var targetGenerator = new HermitCrabTargetGenerator(GetMorphemeId, GetCategory, morpher);

			var analysis = new WordAnalysis(new[]
			{
				new MorphemeInfo("32", "V", "32", MorphemeType.Stem),
				new MorphemeInfo("3SG", "V", "3SG", MorphemeType.Affix)
			}, 0, "V");

			Assert.That(targetGenerator.GenerateWords(analysis), Is.Empty);
		}
	}
}