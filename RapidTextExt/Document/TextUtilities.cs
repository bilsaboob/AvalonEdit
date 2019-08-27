// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using System.Windows.Documents;

using RapidText;
using RapidText.Document;
using RapidText.Utils;
using RapidTextExt.Utils;

#if NREFACTORY
using ICSharpCode.NRefactory.Editor;
#endif

namespace RapidTextExt.Document
{	
	/// <summary>
	/// Static helper methods for working with text.
	/// </summary>
	public static class TextUtilitiesExt
	{	
		#region GetNextCaretPosition
		/// <summary>
		/// Gets the next caret position.
		/// </summary>
		/// <param name="textSource">The text source.</param>
		/// <param name="offset">The start offset inside the text source.</param>
		/// <param name="direction">The search direction (forwards or backwards).</param>
		/// <param name="mode">The mode for caret positioning.</param>
		/// <returns>The offset of the next caret position, or -1 if there is no further caret position
		/// in the text source.</returns>
		/// <remarks>
		/// This method is NOT equivalent to the actual caret movement when using VisualLine.GetNextCaretPosition.
		/// In real caret movement, there are additional caret stops at line starts and ends. This method
		/// treats linefeeds as simple whitespace.
		/// </remarks>
		public static int GetNextCaretPosition(ITextSource textSource, int offset, LogicalDirection direction, CaretPositioningMode mode)
		{
			if (textSource == null)
				throw new ArgumentNullException("textSource");
			switch (mode) {
				case CaretPositioningMode.Normal:
				case CaretPositioningMode.EveryCodepoint:
				case CaretPositioningMode.WordBorder:
				case CaretPositioningMode.WordBorderOrSymbol:
				case CaretPositioningMode.WordStart:
				case CaretPositioningMode.WordStartOrSymbol:
					break; // OK
				default:
					throw new ArgumentException("Unsupported CaretPositioningMode: " + mode, "mode");
			}
			if (direction != LogicalDirection.Backward
			    && direction != LogicalDirection.Forward)
			{
				throw new ArgumentException("Invalid LogicalDirection: " + direction, "direction");
			}
			int textLength = textSource.TextLength;
			if (textLength <= 0) {
				// empty document? has a normal caret position at 0, though no word borders
				if (IsNormal(mode)) {
					if (offset > 0 && direction == LogicalDirection.Backward) return 0;
					if (offset < 0 && direction == LogicalDirection.Forward) return 0;
				}
				return -1;
			}
			while (true) {
				int nextPos = (direction == LogicalDirection.Backward) ? offset - 1 : offset + 1;
				
				// return -1 if there is no further caret position in the text source
				// we also need this to handle offset values outside the valid range
				if (nextPos < 0 || nextPos > textLength)
					return -1;
				
				// check if we've run against the textSource borders.
				// a 'textSource' usually isn't the whole document, but a single VisualLineElement.
				if (nextPos == 0) {
					// at the document start, there's only a word border
					// if the first character is not whitespace
					if (IsNormal(mode) || !char.IsWhiteSpace(textSource.GetCharAt(0)))
						return nextPos;
				} else if (nextPos == textLength) {
					// at the document end, there's never a word start
					if (mode != CaretPositioningMode.WordStart && mode != CaretPositioningMode.WordStartOrSymbol) {
						// at the document end, there's only a word border
						// if the last character is not whitespace
						if (IsNormal(mode) || !char.IsWhiteSpace(textSource.GetCharAt(textLength - 1)))
							return nextPos;
					}
				} else {
					char charBefore = textSource.GetCharAt(nextPos - 1);
					char charAfter = textSource.GetCharAt(nextPos);
					// Don't stop in the middle of a surrogate pair
					if (!char.IsSurrogatePair(charBefore, charAfter)) {
						CharacterClass classBefore = TextUtilities.GetCharacterClass(charBefore);
						CharacterClass classAfter = TextUtilities.GetCharacterClass(charAfter);
						// get correct class for characters outside BMP:
						if (char.IsLowSurrogate(charBefore) && nextPos >= 2) {
							classBefore = TextUtilities.GetCharacterClass(textSource.GetCharAt(nextPos - 2), charBefore);
						}
						if (char.IsHighSurrogate(charAfter) && nextPos + 1 < textLength) {
							classAfter = TextUtilities.GetCharacterClass(charAfter, textSource.GetCharAt(nextPos + 1));
						}
						if (StopBetweenCharacters(mode, classBefore, classAfter)) {
							return nextPos;
						}
					}
				}
				// we'll have to continue searching...
				offset = nextPos;
			}
		}
		
		static bool IsNormal(CaretPositioningMode mode)
		{
			return mode == CaretPositioningMode.Normal || mode == CaretPositioningMode.EveryCodepoint;
		}
		
		static bool StopBetweenCharacters(CaretPositioningMode mode, CharacterClass charBefore, CharacterClass charAfter)
		{
			if (mode == CaretPositioningMode.EveryCodepoint)
				return true;
			// Don't stop in the middle of a grapheme
			if (charAfter == CharacterClass.CombiningMark)
				return false;
			// Stop after every grapheme in normal mode
			if (mode == CaretPositioningMode.Normal)
				return true;
			if (charBefore == charAfter) {
				if (charBefore == CharacterClass.Other &&
				    (mode == CaretPositioningMode.WordBorderOrSymbol || mode == CaretPositioningMode.WordStartOrSymbol))
				{
					// With the "OrSymbol" modes, there's a word border and start between any two unknown characters
					return true;
				}
			} else {
				// this looks like a possible border
				
				// if we're looking for word starts, check that this is a word start (and not a word end)
				// if we're just checking for word borders, accept unconditionally
				if (!((mode == CaretPositioningMode.WordStart || mode == CaretPositioningMode.WordStartOrSymbol)
				      && (charAfter == CharacterClass.Whitespace || charAfter == CharacterClass.LineTerminator)))
				{
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}
