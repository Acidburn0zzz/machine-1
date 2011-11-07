﻿namespace SIL.HermitCrab
{
	/// <summary>
	/// This interface represents an output format for HC objects.
	/// </summary>
	public interface IOutput
	{
		/// <summary>
		/// Morphes the specified word and writes the results to the underlying stream.
		/// </summary>
		/// <param name="morpher">The morpher.</param>
		/// <param name="word">The word.</param>
		/// <param name="prettyPrint">if set to <c>true</c> the results will be formatted for human readability.</param>
		/// <param name="printTraceInputs">if set to <c>true</c> the inputs to rules in the trace will be written out.</param>
		void MorphAndLookupWord(Morpher morpher, string word, bool prettyPrint, bool printTraceInputs);

		/// <summary>
		/// Writes the specified word.
		/// </summary>
		/// <param name="word">The word.</param>
		/// <param name="prettyPrint">if set to <c>true</c> the results will be formatted for human readability.</param>
		void Write(Word word, bool prettyPrint);

		/// <summary>
		/// Writes the specified trace.
		/// </summary>
		/// <param name="trace">The trace.</param>
		/// <param name="prettyPrint">if set to <c>true</c> the results will be formatted for human readability.</param>
		/// <param name="printTraceInputs">if set to <c>true</c> the inputs to rules in the trace will be written out.</param>
		void Write(Trace trace, bool prettyPrint, bool printTraceInputs);

		/// <summary>
		/// Writes the specified load exception.
		/// </summary>
		/// <param name="le">The load exception.</param>
		void Write(LoadException le);

		/// <summary>
		/// Writes the specified morph exception.
		/// </summary>
		/// <param name="me">The morph exception.</param>
		void Write(MorphException me);

		/// <summary>
		/// Flushes the current buffer to the underlying stream.
		/// </summary>
		void Flush();


		/// <summary>
		/// Closes the underlying stream.
		/// </summary>
		void Close();
	}
}
