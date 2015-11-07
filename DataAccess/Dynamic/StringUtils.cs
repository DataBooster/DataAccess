using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public static class StringUtils
	{
		private static readonly Random _RandomNumber;
		private static readonly object _RandomLock;
		private static readonly List<KeyValuePair<Encoding, byte[]>> _EncodingPreambles;
		private static readonly Encoding _DefaultReaderEncoding;

		static StringUtils()
		{
			_RandomNumber = new Random();
			_RandomLock = new object();

			_EncodingPreambles = new List<KeyValuePair<Encoding, byte[]>>();
			_EncodingPreambles.Add(CreateEncodingPreamble(new UTF32Encoding(true, true)));
			_EncodingPreambles.Add(CreateEncodingPreamble(Encoding.UTF32));
			_EncodingPreambles.Add(CreateEncodingPreamble(Encoding.UTF8));
			_EncodingPreambles.Add(CreateEncodingPreamble(Encoding.BigEndianUnicode));
			_EncodingPreambles.Add(CreateEncodingPreamble(Encoding.Unicode));

			_DefaultReaderEncoding = new UTF8Encoding();
		}

		public static string DeunderscoreFieldName(this string fieldName, bool camelCase = false)
		{
			if (string.IsNullOrEmpty(fieldName))
				return fieldName;

			if (fieldName.Length == 1)
				if (camelCase && char.IsUpper(fieldName[0]))
					return fieldName.ToLower(CultureInfo.InvariantCulture);
				else
					return fieldName;

			char[] pascalChars = new char[fieldName.Length];
			int cntUpper = 0, cntLower = 0, lenFragment = 0, lenPascal = 0;

			foreach (char c in fieldName)
			{
				if (char.IsUpper(c))
				{
					pascalChars[lenPascal] = (lenFragment == 0) ? c : char.ToLower(c, CultureInfo.InvariantCulture);
					lenPascal++;
					lenFragment++;
					cntUpper++;
				}
				else if (char.IsLower(c))
				{
					pascalChars[lenPascal] = (lenFragment == 0) ? char.ToUpper(c, CultureInfo.InvariantCulture) : c;
					lenPascal++;
					lenFragment++;
					cntLower++;
				}
				else if (char.IsPunctuation(c) || char.IsWhiteSpace(c))
				{
					lenFragment = 0;
				}
				else
				{
					pascalChars[lenPascal] = c;
					lenPascal++;
					lenFragment = 0;
				}
			}

			if (lenPascal == 0)
				return string.Empty;
			else
				if (camelCase)
				{
					if (char.IsUpper(pascalChars[0]))
						pascalChars[0] = char.ToLower(pascalChars[0], CultureInfo.InvariantCulture);

					return new string(pascalChars, 0, lenPascal);
				}
				else
					return (cntUpper > 0 && cntLower > 0) ? fieldName : new string(pascalChars, 0, lenPascal);
		}

		internal static string CompactFieldName(this string fieldName, bool toUpper = false, bool toLower = false)
		{
			if (string.IsNullOrEmpty(fieldName))
				return fieldName;

			char[] compactedChars = new char[fieldName.Length];
			int compactedLength = 0;

			foreach (char c in fieldName)
				if (char.IsPunctuation(c) || char.IsWhiteSpace(c))
					continue;
				else
				{
					if (toUpper && char.IsLower(c))
						compactedChars[compactedLength] = char.ToUpper(c, CultureInfo.InvariantCulture);
					else if (toLower && char.IsUpper(c))
						compactedChars[compactedLength] = char.ToLower(c, CultureInfo.InvariantCulture);
					else
						compactedChars[compactedLength] = c;

					compactedLength++;
				}

			return (compactedLength == 0) ? string.Empty : new string(compactedChars, 0, compactedLength);
		}

		private static bool[] GetRandomBools(int len)
		{
			byte[] rndBuffer = new byte[(len + 7) / 8];

			lock (_RandomLock)
			{
				_RandomNumber.NextBytes(rndBuffer);
			}

			bool[] bits = new bool[len];
			int rb;

			for (int i = 0; i < len; i++)
			{
				rb = (i + 8) / 8 - 1;
				bits[i] = (rndBuffer[rb] & 0x01) == 0x01;
				rndBuffer[rb] >>= 1;
			}

			return bits;
		}

		internal static string ShuffleCase(this string originalString, bool selfInverse = false)
		{
			if (string.IsNullOrWhiteSpace(originalString))
				return originalString;

			bool[] shufflingCases = GetRandomBools(originalString.Length);
			char[] shuffledChars = new char[originalString.Length];

			if (selfInverse)
			{
				char c;

				for (int i = 0; i < shuffledChars.Length; i++)
				{
					c = originalString[i];

					if (shufflingCases[i])
					{
						if (char.IsLower(c))
							shuffledChars[i] = char.ToUpper(c, CultureInfo.InvariantCulture);
						else if (char.IsUpper(c))
							shuffledChars[i] = char.ToLower(c, CultureInfo.InvariantCulture);
						else
							shuffledChars[i] = c;
					}
					else
						shuffledChars[i] = c;
				}
			}
			else
				for (int i = 0; i < shuffledChars.Length; i++)
					shuffledChars[i] = shufflingCases[i] ?
						char.ToUpper(originalString[i], CultureInfo.InvariantCulture) :
						char.ToLower(originalString[i], CultureInfo.InvariantCulture);

			return new string(shuffledChars);
		}

		internal static byte[] ToBytes(this string base64String)
		{
			if (base64String == null)
				return null;

			if (base64String.Length == 0)
				return new byte[0];

			try
			{
				return Convert.FromBase64String(base64String);
			}
			catch (FormatException)
			{
				return Encoding.UTF8.GetBytes(base64String);
			}
		}

		internal static string DecodeBytesToString(this byte[] encodedBytes)
		{
			if (encodedBytes == null)
				return null;

			if (encodedBytes.Length == 0)
				return string.Empty;

			return DetectEncoding(encodedBytes).GetString(encodedBytes);
		}

		private static KeyValuePair<Encoding, byte[]> CreateEncodingPreamble(Encoding encoding)
		{
			return new KeyValuePair<Encoding, byte[]>(encoding, encoding.GetPreamble());
		}

		private static bool Match(byte[] data, byte[] preamble)
		{
			if (data == null || preamble == null || preamble.Length == 0 || data.Length < preamble.Length)
				return false;

			for (long i = 0; i < preamble.Length; i++)
				if (data[i] != preamble[i])
					return false;

			return true;
		}

		private static Encoding DetectEncoding(byte[] data)
		{
			if (data == null || data.Length < 2)
				return _DefaultReaderEncoding;

			foreach (var encodingPreamble in _EncodingPreambles)
				if (Match(data, encodingPreamble.Value))
					return encodingPreamble.Key;

			return _DefaultReaderEncoding;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2015 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2015-03-09
//	Original Host:		http://dbParallel.codeplex.com
//	Primary Host:		http://DataBooster.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
