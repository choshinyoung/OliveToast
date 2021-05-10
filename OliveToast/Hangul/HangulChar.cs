using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPark.Hangul
{
	/// <summary>
	/// 한글 문자 클래스: 현대 한글 문자에 대해 다양한 속성과 메서드를 제공하는 클래스입니다.
	/// </summary>
	public class HangulChar
	{
		// 한글에 대한 유니코드 범위 [44032, 55215]
		private const int BeginCode = 0xAC00;
		private const int EndCode = 0xD7AF;

		// 문자 구성: 초성, 중성, 종성
		private static readonly char[] Onset = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ',
			'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
		private static readonly char[] Nucleus = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ',
			'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
		private static readonly char[] Coda = { (char)0x00, 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ',
			'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };

		// 문자 획수: 초성, 중성, 종성
		private static readonly int[] OnsetStrokes = { 1, 2, 1, 2, 4, 3, 3, 4, 8, 2, 4, 1, 2, 4, 3, 2, 3, 4, 3 };
		private static readonly int[] NucleusStrokes =
			{ 2, 3, 3, 4, 2, 3, 3, 4, 2, 4, 5, 3, 3, 2, 4, 5, 3, 3, 1, 2, 1 };
		private static readonly int[] CodaStrokes =
			{ 0, 1, 2, 3, 1, 3, 4, 2, 3, 4, 6, 7, 5, 6, 7, 6, 3, 4, 6, 2, 4, 1, 2, 3, 2, 3, 4, 3 };

		/// <summary>
		/// 문자로부터 한글 문자 클래스의 인스턴스를 생성합니다.
		/// </summary>
		/// <param name="character">한글 문자(char)</param>
		public HangulChar(char character)
		{
			this.CurrentCharacter = character;
		}

		/// <summary>
		/// 유니코드로부터 한글 문자 클래스의 인스턴스를 생성합니다.
		/// </summary>
		/// <param name="unicode">한글 문자(char)에 해당하는 유니코드</param>
		public HangulChar(int unicode)
		{
			this.CurrentCharacter = Convert.ToChar(unicode);
		}

		/// <summary>
		/// 현재 인스턴스의 문자입니다.
		/// </summary>
		public char CurrentCharacter { get; private set; }

		/// <summary>
		/// 현재 인스턴스 문자의 유니코드입니다.
		/// </summary>
		public int CurrentUnicode => (int)CurrentCharacter;

		/// <summary>
		/// 인자의 음소 배열을 한글 음절로 합성합니다. 
		/// 반환값은 합성의 성공(가능) 여부를 나타냅니다. 
		/// </summary>
		/// <param name="phonemes">초성, 중성(, 종성) 순의 한글 음소 배열</param>
		/// <param name="syllable">합성된 한글 음절</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"><paramref name="phonemes"/>이 null로 주어질 경우</exception>
		public static bool TryJoinToSyllable(char[] phonemes, out char syllable)
		{
			if (phonemes == null) { throw new ArgumentNullException(); }

			bool isSuccess = false;
			// 초성, 중성(, 종성)으로 이루어진 문자 배열인지 확인
			if (phonemes.Length >= 2 && phonemes.Length <= 3)
			{
				bool check = Onset.Contains(phonemes[0]) && Nucleus.Contains(phonemes[1]);
				isSuccess = phonemes.Length == 3 ? check && Coda.Contains(phonemes[2]) : check;
			}

			syllable = (char)0x00;
			// 한글 음절로의 합성이 불가능한 경우
			if (!isSuccess)
			{
				return false;
			}
			// 한글 음절로의 합성이 가능한 경우
			int onsetIndex = Array.IndexOf(Onset, phonemes[0]);
			int nucleusIndex = Array.IndexOf(Nucleus, phonemes[1]);
			int codaIndex = phonemes.Length == 3 ? Array.IndexOf(Coda, phonemes[2]) : 0;
			int newCode = BeginCode + (onsetIndex * 588) + (nucleusIndex * 28) + codaIndex;
			if (newCode < BeginCode || newCode > EndCode)
			{
				isSuccess = false;
			}
			else
			{
				syllable = Convert.ToChar(newCode);
			}

			return isSuccess;
		}

		/// <summary>
		/// 인자의 음소 배열을 한글 음절로 합성합니다. 
		/// 합성이 불가능할 경우 예외를 발생시킵니다.
		/// </summary>
		/// <param name="phonemes">초성, 중성(, 종성) 순의 한글 음소 배열</param>
		/// <exception cref="InvalidOperationException">인수의 배열이 합성 가능한 초성, 중성(, 종성)이 아닐 경우</exception>
		/// <returns></returns>
		public static char JoinToSyllable(char[] phonemes)
		{
			bool isSuccess = TryJoinToSyllable(phonemes, out char syllable);

			if (!isSuccess)
			{
				throw new InvalidOperationException("인수의 배열은 합성 가능한 한글 초성, 중성(, 종성)이 아닙니다.");
			}

			return syllable;
		}

		/// <summary>
		/// 검색문자를 대상문자에 대해 (초성) 비교 후 일치 여부를 반환합니다. 
		/// 검색문자에 초성이 주어질 경우 초성 일치, 그렇지 않은 경우 문자 완전 일치 여부를 반환합니다.
		/// </summary>
		/// <param name="searchChar">(초성) 비교할 문자</param>
		/// <param name="targetChar">비교 대상 문자</param>
		/// <returns></returns>
		public static bool IsOnsetMatch(char searchChar, char targetChar)
		{
			// 1. 검색문자가 초성인 경우 대응하는 대상문자도 초성을 비교
			// 2. 그렇지 않은 경우 대응하는 대상문자와 완전 일치 여부 비교
			HangulChar shc = new HangulChar(searchChar);
			HangulChar thc = new HangulChar(targetChar);
			if (shc.IsOnset() && thc.TrySplitSyllable(out char[] phonemes))
			{
				targetChar = phonemes[0];
			}

			return searchChar == targetChar ? true : false;
		}

		/// <summary>
		/// 검색문자를 현재 인스턴스의 문자에 대해 (초성) 비교 후 일치 여부를 반환합니다.
		/// 검색문자에 초성이 주어질 경우 초성 일치, 그렇지 않은 경우 문자 완전 일치 여부를 반환합니다.
		/// </summary>
		/// <param name="searchChar">(초성) 비교할 문자</param>
		/// <returns></returns>
		public bool IsOnsetMatch(char searchChar)
		{
			return HangulChar.IsOnsetMatch(searchChar, this.CurrentCharacter);
		}

		/// <summary>
		/// 초성으로 사용될 수 있는지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsOnset() => Onset.Contains(CurrentCharacter);

		/// <summary>
		/// 중성으로 사용될 수 있는지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsNucleus() => Nucleus.Contains(CurrentCharacter);

		/// <summary>
		/// 종성으로 사용될 수 있는지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsCoda() => Coda.Contains(CurrentCharacter);

		/// <summary>
		/// 자음인지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsConsonant() => Onset.Contains(CurrentCharacter) || Coda.Contains(CurrentCharacter);

		/// <summary>
		/// 모음인지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsVowel() => Nucleus.Contains(CurrentCharacter);

		/// <summary>
		/// 음소(낱소리)인지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsPhoneme() => IsConsonant() || IsVowel();

		/// <summary>
		/// 음소(낱소리)가 아닌 완전한 한글 음절인지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsSyllable() => BeginCode <= (int)CurrentCharacter && (int)CurrentCharacter <= EndCode;

		/// <summary>
		/// 한글 문자인지 판단합니다.
		/// </summary>
		/// <returns></returns>
		public bool IsKoreanCharacter() => IsPhoneme() || IsSyllable();

		/// <summary>
		/// 한글 문자인지 판단합니다. (== IsKoreanCharacter())
		/// </summary>
		/// <returns></returns>
		public bool IsHangul() => IsKoreanCharacter();

		/// <summary>
		/// 한글 음절을 초성, 중성, 종성 순으로 분리합니다. 
		/// 반환 결과는 분리의 성공(가능)여부를 나타냅니다.
		/// </summary>
		/// <param name="phonemes">초성, 중성, 종성 순으로 분리된 배열</param>
		/// <returns></returns>
		public bool TrySplitSyllable(out char[] phonemes)
		{
			// 분리 가능한 한글이 아닌 경우
			if (!IsSyllable())
			{
				phonemes = new char[] { (char)0x00, (char)0x00, (char)0x00 };
				return false;
			}

			int foo = (int)CurrentCharacter - BeginCode;
			int onsetIndex = (int)foo / 588;
			int nucleusIndex = (int)(foo - onsetIndex * 588) / 28;
			int codaIndex = foo - onsetIndex * 588 - 28 * nucleusIndex;

			phonemes = new char[] { Onset[onsetIndex], Nucleus[nucleusIndex], Coda[codaIndex] };
			return true;
		}

		/// <summary>
		/// 한글 음절을 초성, 중성, 종성 순으로 분리합니다.
		/// 분리가 불가능한 경우 예외를 발생시킵니다.
		/// </summary>
		/// <exception cref="InvalidOperationException">인스턴스의 문자가 분리 가능한 한글이 아닌 경우</exception>
		/// <returns></returns>
		public char[] SplitSyllable()
		{
			bool isSuccess = TrySplitSyllable(out char[] phonemes);

			if (!isSuccess)
			{
				throw new InvalidOperationException("인스턴스의 문자가 분리 가능한 한글이 아닙니다.");
			}

			return phonemes;
		}

		/// <summary>
		/// 한글 획수를 반환합니다. 한글이 아닌 문자의 경우 0을 반환합니다.
		/// </summary>
		/// <returns></returns>
		public int CountStrokes()
		{
			// 한글이 아닌 경우 0을 반환
			if (!IsHangul())
			{
				return 0;
			}

			int count;
			// 한글 - 음소인 경우
			if (IsPhoneme())
			{
				if (IsOnset())
				{
					count = OnsetStrokes[Array.IndexOf(Onset, CurrentCharacter)];
				}
				else if (IsNucleus())
				{
					count = NucleusStrokes[Array.IndexOf(Nucleus, CurrentCharacter)];
				}
				else
				{
					count = CodaStrokes[Array.IndexOf(Coda, CurrentCharacter)];
				}
			}
			// 한글 - 분리 가능한 음절인 경우
			else
			{
				_ = TrySplitSyllable(out char[] phonemes);
				count = OnsetStrokes[Array.IndexOf(Onset, phonemes[0])];
				count += NucleusStrokes[Array.IndexOf(Nucleus, phonemes[1])];
				count += CodaStrokes[Array.IndexOf(Coda, phonemes[2])];
			}

			return count;

		}
	}
}
