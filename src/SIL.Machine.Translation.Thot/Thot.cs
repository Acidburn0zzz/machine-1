﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SIL.Machine.Translation.Thot
{
	internal static class Thot
	{
		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr decoder_open(string cfgFileName);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr decoder_openSession(IntPtr decoderHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void decoder_saveModels(IntPtr decoderHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr decoder_getSingleWordAlignmentModel(IntPtr decoderHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr decoder_getInverseSingleWordAlignmentModel(IntPtr decoderHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void decoder_close(IntPtr decoderHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int session_translate(IntPtr sessionHandle, IntPtr sentence, IntPtr translation, int capacity, out IntPtr data);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int session_translateInteractively(IntPtr sessionHandle, IntPtr sentence, IntPtr translation, int capacity, out IntPtr data);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int session_addStringToPrefix(IntPtr sessionHandle, IntPtr addition, IntPtr translation, int capacity, out IntPtr data);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int session_setPrefix(IntPtr sessionHandle, IntPtr prefix, IntPtr translation, int capacity, out IntPtr data);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void session_trainSentencePair(IntPtr sessionHandle, IntPtr sourceSentence, IntPtr targetSentence);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void session_close(IntPtr sessionHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int tdata_getPhraseCount(IntPtr dataHandle);
		
		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int tdata_getSourceSegmentation(IntPtr dataHandle, IntPtr sourceSegmentation, int capacity);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int tdata_getTargetSegmentCuts(IntPtr dataHandle, IntPtr targetSegmentCuts, int capacity);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern int tdata_getTargetUnknownWords(IntPtr dataHandle, IntPtr targetUnknownWords, int capacity);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void tdata_destroy(IntPtr dataHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr swAlignModel_create();

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr swAlignModel_open(string prefFileName);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void swAlignModel_addSentencePair(IntPtr swAlignModelHandle, IntPtr sourceSentence, IntPtr targetSentence);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void swAlignModel_train(IntPtr swAlignModelHandle, int numIters);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void swAlignModel_save(IntPtr swAlignModelHandle, string prefFileName);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern float swAlignModel_getTranslationProbability(IntPtr swAlignModelHandle, IntPtr sourceWord, IntPtr targetWord);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern float swAlignModel_getBestAlignment(IntPtr swAlignModelHandle, IntPtr sourceSentence, IntPtr targetSentence, IntPtr matrix, ref int iLen, ref int jLen);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void swAlignModel_close(IntPtr swAlignModelHandle);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool giza_symmetr1(string lhsFileName, string rhsFileName, string outputFileName, bool transpose);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool phraseModel_generate(string alignmentFileName, int maxPhraseLength, string tableFileName);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr langModel_open(string prefFileName);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern float langModel_getSentenceProbability(IntPtr lmHandle, IntPtr sentence);

		[DllImport("thot", CallingConvention = CallingConvention.Cdecl)]
		public static extern void langModel_close(IntPtr lmHandle);

		public static IntPtr ConvertStringsToNativeUtf8(IEnumerable<string> managedStrings)
		{
			return ConvertStringToNativeUtf8(string.Join(" ", managedStrings));
		}

		public static IntPtr ConvertStringToNativeUtf8(string managedString)
		{
			int len = Encoding.UTF8.GetByteCount(managedString);
			var buffer = new byte[len + 1];
			Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
			IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
			Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
			return nativeUtf8;
		}

		public static string ConvertNativeUtf8ToString(IntPtr nativeUtf8, int len)
		{
			var buffer = new byte[len];
			Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
			return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
		}
	}
}