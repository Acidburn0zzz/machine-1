﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SIL.Extensions;
using SIL.Machine.NgramModeling;
using SIL.ObjectModel;

namespace SIL.Machine.Translation
{
	public class ThotSmtEngine : DisposableBase, ISmtEngine
	{
		public static void TrainModels(string cfgFileName, IEnumerable<IEnumerable<string>> sourceCorpus, IEnumerable<IEnumerable<string>> targetCorpus)
		{
			string cfgDir = Path.GetDirectoryName(cfgFileName);
			Debug.Assert(cfgDir != null);
			string tmPrefix = null, lmPrefix = null;
			foreach (string line in File.ReadAllLines(cfgFileName))
			{
				string l = line.Trim();
				if (l.StartsWith("-tm"))
				{
					tmPrefix = l.Substring(4).Trim();
					if (!Path.IsPathRooted(tmPrefix))
						tmPrefix = Path.Combine(cfgDir, tmPrefix);
				}
				else if (l.StartsWith("-lm"))
				{
					lmPrefix = l.Substring(4).Trim();
					if (!Path.IsPathRooted(lmPrefix))
						lmPrefix = Path.Combine(cfgDir, lmPrefix);
				}
			}

			if (string.IsNullOrEmpty(lmPrefix))
				throw new InvalidOperationException("The configuration file does not specify the language model parameter.");

			TrainLanguageModel(targetCorpus, lmPrefix, 3);
			TrainTranslationModel(sourceCorpus, targetCorpus, tmPrefix);
		}

		private static void TrainLanguageModel(IEnumerable<IEnumerable<string>> targetCorpus, string lmPrefix, int ngramSize)
		{
			string dir = Path.GetDirectoryName(lmPrefix);
			Debug.Assert(dir != null);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			WriteNgramCountsFile(targetCorpus, lmPrefix, ngramSize);
			WriteLanguageModelWeightsFile(lmPrefix, ngramSize);
			WriteWordPredictionFile(targetCorpus, lmPrefix);
		}

		private static void WriteNgramCountsFile(IEnumerable<IEnumerable<string>> targetCorpus, string lmPrefix, int ngramSize)
		{
			int wordCount = 0;
			var ngrams = new Dictionary<Ngram<string>, int>();
			foreach (IEnumerable<string> segment in targetCorpus)
			{
				List<string> words = segment.ToList();
				words.Insert(0, "<s>");
				words.Add("</s>");
				wordCount += words.Count;
				for (int n = 1; n <= ngramSize; n++)
				{
					for (int i = 0; i <= words.Count - n; i++)
					{
						var ngram = new Ngram<string>(Enumerable.Range(i, n).Select(j => words[j]));
						ngrams.UpdateValue(ngram, () => 0, c => c + 1);
					}
				}
			}

			using (var writer = new StreamWriter(lmPrefix))
			{
				foreach (KeyValuePair<Ngram<string>, int> kvp in ngrams.OrderBy(kvp => kvp.Key.Length).ThenBy(kvp => string.Join(" ", kvp.Key)))
				{
					writer.Write("{0} {1} {2}\n", string.Join(" ", kvp.Key), kvp.Key.Length == 1 ? wordCount : ngrams[kvp.Key.TakeAllExceptLast()], kvp.Value);
				}
			}
		}

		private static void WriteLanguageModelWeightsFile(string lmPrefix, int ngramSize)
		{
			File.WriteAllText(lmPrefix + ".weights", string.Format("{0} 3 10 {1}\n", ngramSize, string.Join(" ", Enumerable.Repeat("0.5", ngramSize * 3))));
		}

		private static void WriteWordPredictionFile(IEnumerable<IEnumerable<string>> targetCorpus, string lmPrefix)
		{
			var rand = new Random(31415);
			using (var writer = new StreamWriter(lmPrefix + ".wp"))
			{
				foreach (IEnumerable<string> segment in targetCorpus.Take(100000).OrderBy(i => rand.Next()))
					writer.Write("{0}\n", string.Join(" ", segment));
			}
		}

		private static void TrainTranslationModel(IEnumerable<IEnumerable<string>> sourceCorpus, IEnumerable<IEnumerable<string>> targetCorpus, string tmPrefix)
		{
			string dir = Path.GetDirectoryName(tmPrefix);
			Debug.Assert(dir != null);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			TrainSingleWordAlignmentModel(targetCorpus, sourceCorpus, tmPrefix + "_swm");
			TrainSingleWordAlignmentModel(sourceCorpus, targetCorpus, tmPrefix + "_invswm");
			Thot.giza_symmetr1(tmPrefix + "_swm.bestal", tmPrefix + "_invswm.bestal", tmPrefix + ".A3.final", true);
			Thot.phraseModel_generate(tmPrefix + ".A3.final", 7, tmPrefix + ".ttable");
			// TODO: keep the N-best source phrases for a particular target phrase in the phrase table
			File.WriteAllText(tmPrefix + ".lambda", "0.01");
			File.WriteAllText(tmPrefix + ".srcsegmlentable", "Uniform");
			File.WriteAllText(tmPrefix + ".trgsegmlentable", "Geometric");
		}

		private static void TrainSingleWordAlignmentModel(IEnumerable<IEnumerable<string>> sourceCorpus, IEnumerable<IEnumerable<string>> targetCorpus, string swmPrefix)
		{
			using (var swAlignModel = new ThotSingleWordAlignmentModel(swmPrefix, true))
			{
				foreach (Tuple<IEnumerable<string>, IEnumerable<string>> pair in sourceCorpus.Zip(targetCorpus))
					swAlignModel.AddSegmentPair(pair.Item1, pair.Item2);
				swAlignModel.Train(5);
				swAlignModel.Save();

				// TODO: prune lex table by a probability threshold

				using (var writer = new StreamWriter(swmPrefix + ".bestal"))
				{
					foreach (Tuple<IEnumerable<string>, IEnumerable<string>> pair in sourceCorpus.Zip(targetCorpus))
					{
						WordAlignmentMatrix waMatrix;
						double prob = swAlignModel.GetBestAlignment(pair.Item1.ToArray(), pair.Item2.ToArray(), out waMatrix);
						writer.Write("# Alignment probability= {0}\n", prob);
						writer.Write(waMatrix.ToGizaFormat(pair.Item1, pair.Item2));
					}
				}
			}
		}

		private readonly string _cfgFileName;
		private IntPtr _handle;
		private readonly HashSet<ThotSmtSession> _sessions;
		private readonly ISegmentAligner _segmentAligner;
		private readonly ThotSingleWordAlignmentModel _singleWordAlignmentModel;

		public ThotSmtEngine(string cfgFileName)
		{
			_cfgFileName = cfgFileName;
			_sessions = new HashSet<ThotSmtSession>();
			_handle = Thot.decoder_open(_cfgFileName);
			_singleWordAlignmentModel = new ThotSingleWordAlignmentModel(Thot.decoder_getSingleWordAlignmentModel(_handle));
			_segmentAligner = new FuzzyEditDistanceSegmentAligner(_singleWordAlignmentModel);
		}

		public void Train(IEnumerable<IEnumerable<string>> sourceCorpus, IEnumerable<IEnumerable<string>> targetCorpus)
		{
			lock (_sessions)
			{
				if (_sessions.Count > 0)
					throw new InvalidOperationException("The engine cannot be trained while there are active sessions open.");

				Thot.decoder_close(_handle);
				TrainModels(_cfgFileName, sourceCorpus, targetCorpus);
				_handle = Thot.decoder_open(_cfgFileName);
				_singleWordAlignmentModel.Handle = Thot.decoder_getSingleWordAlignmentModel(_handle);
			}
		}

		public ISmtSession StartSession()
		{
			CheckDisposed();

			var session = new ThotSmtSession(this);
			lock (_sessions)
				_sessions.Add(session);
			return session;
		}

		public void SaveModels()
		{
			Thot.decoder_saveModels(_handle);
		}

		public ISegmentAligner SegmentAligner
		{
			get { return _segmentAligner; }
		}

		internal IntPtr Handle
		{
			get { return _handle; }
		}

		internal void RemoveSession(ThotSmtSession session)
		{
			lock (_sessions)
				_sessions.Remove(session);
		}

		protected override void DisposeManagedResources()
		{
			lock (_sessions)
			{
				foreach (ThotSmtSession session in _sessions.ToArray())
					session.Dispose();
			}
		}

		protected override void DisposeUnmanagedResources()
		{
			Thot.decoder_close(_handle);
		}
	}
}