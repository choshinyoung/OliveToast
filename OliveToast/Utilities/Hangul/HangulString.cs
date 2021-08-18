using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HPark.Hangul
{
	/// <summary>
	/// 한글 문자열 클래스: (한글을 포함한) 문자열에 대해 한글 처리에 관련된 다양한 속성과 메서드를 제공하는 클래스입니다. 
	/// </summary>
	public class HangulString
	{
		/// <summary>
		/// 문자열로부터 한글 문자열 클래스의 인스턴스를 생성합니다.
		/// </summary>
		/// <param name="aString">인스턴스를 생성할 문자열</param>
		public HangulString(string aString) => CurrentString = aString;

		/// <summary>
		/// 현재 인스턴스의 문자열입니다.
		/// </summary>
		public string CurrentString { get; private set; }

		/// <summary>
		/// 문자열을 HangulChar 클래스 인스턴스의 배열로 변환하여 반환합니다.
		/// </summary>
		/// <param name="aString"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">입력 인자가 null인 경우</exception>
		public static HangulChar[] ToHangulCharArray(string aString)
		{
			if (aString is null)
			{
				throw new ArgumentNullException();
			}
			return aString.ToCharArray().Select(c => new HangulChar(c)).ToArray();
		}

		/// <summary>
		/// 문자열을 한글 문자열 부분과 나머지 문자열 부분으로 구분하여 반환합니다.
		/// </summary>
		/// <param name="aString"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">입력 인자가 null인 경우</exception>
		public static string[] SeparateString(string aString)
		{
			if (aString is null)
			{
				throw new ArgumentNullException();
			}

			string[] result = new string[2];
			StringBuilder sbKorean = new StringBuilder();
			StringBuilder sbOthers = new StringBuilder();
			foreach (char c in aString)
			{
				HangulChar hc = new HangulChar(c);
				if (hc.IsKoreanCharacter())
				{
					sbKorean.Append(c);
				}
				else
				{
					sbOthers.Append(c);
				}
			}
			result[0] = sbKorean.ToString();
			result[1] = sbOthers.ToString();
			return result;
		}

		/// <summary>
		/// 문자열이 모두 한글로만 이루어져 있는지의 여부를 반환합니다.
		/// </summary>
		/// <param name="aString"></param>
		/// <returns></returns>
		public static bool IsAllHangul(string aString)
		{
			string checkPoint = HangulString.SeparateString(aString)[1];
			if (checkPoint.Length > 0)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// 문자열에 대해 한글 음절을 초성, 중성, 종성으로 분리하여 반환합니다.
		/// </summary>
		/// <param name="aString"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"><paramref name="aString"/>이 null인 경우</exception>
		public static string SplitToPhonemes(string aString)
		{
			if (aString == null)
			{
				throw new ArgumentNullException();
			}

			StringBuilder sb = new StringBuilder();
			foreach (char c in aString)
			{
				HangulChar hc = new HangulChar(c);
				// 한글 음절로 판명되어 분리가 가능한 경우
				if (hc.TrySplitSyllable(out char[] phonemes))
				{
					foreach (char pc in phonemes)
					{
						// 초성-중성으로만 이루어져 있는 경우 종성은 반환 문자열에 포함하지 않음
						if (pc != (char)0x00)
						{
							sb.Append(pc);
						}
					}
				}
				// 한글 음절이 아닌 경우
				else
				{
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// 문자열 내의 초성, 중성, 종성 음소를 합성하여 반환합니다.
		/// 의도하지 않은 반환 결과를 피하기 위해 일반적으로 사용하는 방식의 
		/// (완전한 한글 음절이 분리된) 자음/모음으로 구성된 문자열을 인수로 사용할 것을 권장합니다.
		/// </summary>
		/// <param name="aString"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"><paramref name="aString"/>이 null인 경우</exception>
		public static string JoinPhonemes(string aString)
		{
			if (aString == null) { throw new ArgumentNullException(); }
			StringBuilder sb = new StringBuilder();
			int curIdx = 0;
			int lastIdx = aString.Length - 1;
			// 마지막 인덱스 이후의 값 판단에 대한 예외를 피하기 위해 대상 문자열에 10개의 공백 임시 추가
			string newString = aString + new string(' ', 10);

			while (curIdx <= lastIdx)
			{
				// 현재 인덱스의 문자
				HangulChar hc = new HangulChar(newString[curIdx]);
				// 현재 인덱스의 문자가 음소가 아닌 경우 현재 문자를 더하고 다음 인덱스로 이동
				if (!hc.IsPhoneme())
				{
					sb.Append(hc.CurrentCharacter);
					curIdx++;
					continue;
				}

				// 현재 인덱스의 문자가 음소인 경우에 대해
				// (1) 현재 인덱스로부터 2글자 합성이 가능하고
				// (2)-A 이후 첫 글자가 한글이 아니거나
				// (2)-B 이후 두 글자(초성-중성)가 합성이 가능한 경우
				// 현재 인덱스로부터 2글자를 합성하고, 인덱스를 2칸 전진
				bool isCur2SpanOk = HangulChar.TryJoinToSyllable(
					new char[] { newString[curIdx], newString[curIdx + 1] },
					out char cur2SpanSyllable);
				bool isCur2Next1Ok = !(new HangulChar(newString[curIdx + 2])).IsHangul();
				bool isCur2Next2Ok = HangulChar.TryJoinToSyllable(
					new char[] { newString[curIdx + 2], newString[curIdx + 3] },
					out char _);
				// (3) 현재 인덱스로부터 3글자 합성이 가능하고
				// (4)-A 이후 첫 글자가 한글이 아니거나
				// (4)-B 이후 두 글자(초성-중성)가 합성이 가능한 경우
				// 현재 인덱스로부터 3글자를 합성하고, 인덱스를 3칸 전진
				bool isCur3SpanOk = HangulChar.TryJoinToSyllable(
					new char[] { newString[curIdx], newString[curIdx + 1], newString[curIdx + 2] },
					out char cur3SpanSyllable);
				bool isCur3Next1Ok = !(new HangulChar(newString[curIdx + 3])).IsHangul();
				bool isCur3Next2Ok = HangulChar.TryJoinToSyllable(
					new char[] { newString[curIdx + 3], newString[curIdx + 4] },
					out char _);

				// 상기 논리에 의한 판단부
				// 2글자 기준 판단
				if (isCur2SpanOk && (isCur2Next1Ok || isCur2Next2Ok))
				{
					sb.Append(cur2SpanSyllable);
					curIdx += 2;
				}
				// 3글자 기준 판단
				else if (isCur3SpanOk && (isCur3Next1Ok || isCur3Next2Ok))
				{
					sb.Append(cur3SpanSyllable);
					curIdx += 3;
				}
				else
				{
					sb.Append(hc.CurrentCharacter);
					curIdx++;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// 문자열에 대해 초성 검색을 실시합니다. 반환값은 초성 검색에 대한 결과의 존재여부입니다. 
		/// 검색 문자열에 초성이 주어질 경우 초성 일치, 그렇지 않은 경우 문자 완전 일치 여부를 반환합니다.
		/// </summary>
		/// <param name="searchString">검색할 문자열</param>
		/// <param name="targetString">대상 문자열</param>
		/// <param name="indices">(초성)일치가 발견된 대상 문자열 내 인덱스 배열</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"><paramref name="searchString"/> 또는 <paramref name="targetString"/>이 null인 경우</exception>
		public static bool GetOnsetMatches(string searchString, string targetString, out int[] indices)
		{
			if (searchString == null || targetString == null)
			{
				throw new ArgumentNullException();
			}

			List<int> idxs = new List<int>();

			// 검색 문자열의 길이가 대상 문자열의 길이보다 긴 경우
			if (searchString.Length > targetString.Length)
			{
				indices = idxs.ToArray();
				return false;
			}

			// 대상 문자열 내의 인덱스를 증가시켜 가며
			for (int curIdx = 0; curIdx < targetString.Length - searchString.Length + 1; curIdx++)
			{
				// 해당 인덱스로부터 검색 문자열의 길이만큼 초성일치 검색 수행
				// 초성 일치가 될 경우 해당 인덱스 저장
				bool isMatch = true;
				for (int chkIdx = curIdx; chkIdx < curIdx + searchString.Length; chkIdx++)
				{
					if (!HangulChar.IsOnsetMatch(searchString[chkIdx - curIdx], targetString[chkIdx]))
					{
						isMatch = false;
						break;
					}
				}
				if (isMatch)
				{
					idxs.Add(curIdx);
				}
			}
			indices = idxs.ToArray();
			if (indices.Length > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 현재 인스턴스의 문자열을 HangulChar 클래스 인스턴스의 배열로 변환하여 반환합니다.
		/// </summary>
		/// <returns></returns>
		public HangulChar[] ToHangulCharArray() => HangulString.ToHangulCharArray(CurrentString);

		/// <summary>
		/// 현재 인스턴스의 문자열을 한글 문자열 부분과 나머지 문자열 부분으로 구분하여 반환합니다.
		/// </summary>
		/// <returns></returns>
		public string[] SeparateString() => HangulString.SeparateString(CurrentString);

		/// <summary>
		/// 현재 인스턴스의 문자열이 모두 한글로만 이루어져 있는지의 여부를 반환합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsAllHangul() => HangulString.IsAllHangul(CurrentString);

		/// <summary>
		/// 현재 인스턴스의 문자열에 대해 한글 음절을 초성, 중성, 종성으로 분리하여 반환합니다.
		/// </summary>
		/// <returns></returns>
		public string SplitToPhonemes() => HangulString.SplitToPhonemes(CurrentString);

		/// <summary>
		/// 인스턴스 문자열 내의 초성, 중성, 종성 음소를 합성하여 반환합니다.
		/// 의도하지 않은 반환 결과를 피하기 위해 일반적으로 사용하는 방식의 
		/// (완전한 한글 음절이 분리된) 자음/모음으로 구성된 문자열을 인수로 사용할 것을 권장합니다.
		/// </summary>
		/// <returns></returns>
		public string JoinPhonemes() => HangulString.JoinPhonemes(CurrentString);

		/// <summary>
		/// 인스턴스 문자열의 길이(글자수)를 반환합니다.
		/// </summary>
		/// <remarks>
		/// 윈도우 시스템에서는 개행문자가 2글자(2바이트)로 취급됨에 유의해야 합니다.
		/// </remarks>
		/// <param name="ignoreWhiteSpcaes">공백문자(whitespace) 무시 여부</param>
		/// <returns></returns>
		public int GetStringLength(bool ignoreWhiteSpcaes = false)
		{
			string inputString = ignoreWhiteSpcaes ? RemoveWhiteSpaces(CurrentString) : CurrentString;
			return inputString.Length;
		}

		/// <summary>
		/// 현재 인코딩에 대해 인스턴스 문자열의 길이(바이트)를 반환합니다.
		/// </summary>
		/// <remarks>
		/// 윈도우 시스템에서는 개행문자가 2글자(2바이트)로 취급됨에 유의해야 합니다.
		/// </remarks>
		/// <param name="ignoreWhiteSpcaes">공백문자(whitespace) 무시 여부</param>
		/// <returns></returns>
		public int GetStringByteLength(bool ignoreWhiteSpcaes = false)
		{
			string inputString = ignoreWhiteSpcaes ? RemoveWhiteSpaces(CurrentString) : CurrentString;
			return Encoding.Default.GetByteCount(inputString);
		}

		/// <summary>
		/// 인수의 문자열에서 공백문자(whitespace)를 제거한 문자열을 반환합니다.
		/// </summary>
		/// <param name="aString"></param>
		/// <returns></returns>
		private static string RemoveWhiteSpaces(string aString) =>
			new string(aString.Where(c => !char.IsWhiteSpace(c)).Select(c => c).ToArray());
	}
}
