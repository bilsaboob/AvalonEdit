// Copyright (c) 2010-2013 AlphaSierraPapa for the SharpDevelop Team
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace RapidText.Document
{
	#if !NREFACTORY
	/// <summary>
	/// A line/column position.
	/// Text editor lines/columns are counted started from one.
	/// </summary>
	/// <remarks>
	/// The document provides the methods <see cref="IDocument.GetLocation"/> and
	/// <see cref="IDocument.GetOffset(TextLocation)"/> to convert between offsets and TextLocations.
	/// </remarks>
	[Serializable]
	[TypeConverter(typeof(TextLocationConverter))]
	public struct TextLocation : IComparable<TextLocation>, IEquatable<TextLocation>
	{
		/// <summary>
		/// Represents no text location (0, 0).
		/// </summary>
		public static readonly TextLocation Empty = new TextLocation(0, 0);
		
		/// <summary>
		/// Creates a TextLocation instance.
		/// </summary>
		public TextLocation(int line, int column)
		{
			this.line = line;
			this.column = column;
		}
		
		readonly int column, line;
		
		/// <summary>
		/// Gets the line number.
		/// </summary>
		public int Line {
			get { return line; }
		}
		
		/// <summary>
		/// Gets the column number.
		/// </summary>
		public int Column {
			get { return column; }
		}
		
		/// <summary>
		/// Gets whether the TextLocation instance is empty.
		/// </summary>
		public bool IsEmpty {
			get {
				return column <= 0 && line <= 0;
			}
		}
		
		/// <summary>
		/// Gets a string representation for debugging purposes.
		/// </summary>
		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "(Line {1}, Col {0})", this.column, this.line);
		}
		
		/// <summary>
		/// Gets a hash code.
		/// </summary>
		public override int GetHashCode()
		{
			return unchecked (191 * column.GetHashCode() ^ line.GetHashCode());
		}
		
		/// <summary>
		/// Equality test.
		/// </summary>
		public override bool Equals(object obj)
		{
			if (!(obj is TextLocation)) return false;
			return (TextLocation)obj == this;
		}
		
		/// <summary>
		/// Equality test.
		/// </summary>
		public bool Equals(TextLocation other)
		{
			return this == other;
		}
		
		/// <summary>
		/// Equality test.
		/// </summary>
		public static bool operator ==(TextLocation left, TextLocation right)
		{
			return left.column == right.column && left.line == right.line;
		}
		
		/// <summary>
		/// Inequality test.
		/// </summary>
		public static bool operator !=(TextLocation left, TextLocation right)
		{
			return left.column != right.column || left.line != right.line;
		}
		
		/// <summary>
		/// Compares two text locations.
		/// </summary>
		public static bool operator <(TextLocation left, TextLocation right)
		{
			if (left.line < right.line)
				return true;
			else if (left.line == right.line)
				return left.column < right.column;
			else
				return false;
		}
		
		/// <summary>
		/// Compares two text locations.
		/// </summary>
		public static bool operator >(TextLocation left, TextLocation right)
		{
			if (left.line > right.line)
				return true;
			else if (left.line == right.line)
				return left.column > right.column;
			else
				return false;
		}
		
		/// <summary>
		/// Compares two text locations.
		/// </summary>
		public static bool operator <=(TextLocation left, TextLocation right)
		{
			return !(left > right);
		}
		
		/// <summary>
		/// Compares two text locations.
		/// </summary>
		public static bool operator >=(TextLocation left, TextLocation right)
		{
			return !(left < right);
		}
		
		/// <summary>
		/// Compares two text locations.
		/// </summary>
		public int CompareTo(TextLocation other)
		{
			if (this == other)
				return 0;
			if (this < other)
				return -1;
			else
				return 1;
		}
	}
	
	/// <summary>
	/// Converts strings of the form '0+[;,]0+' to a <see cref="TextLocation"/>.
	/// </summary>
	public class TextLocationConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
		}
		
		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(TextLocation) || base.CanConvertTo(context, destinationType);
		}
		
		/// <inheritdoc/>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string) {
				string[] parts = ((string)value).Split(';', ',');
				if (parts.Length == 2) {
					return new TextLocation(int.Parse(parts[0], culture), int.Parse(parts[1], culture));
				}
			}
			return base.ConvertFrom(context, culture, value);
		}
		
		/// <inheritdoc/>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is TextLocation && destinationType == typeof(string)) {
				var loc = (TextLocation)value;
				return loc.Line.ToString(culture) + ";" + loc.Column.ToString(culture);
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
	
	/// <summary>
	/// An (Offset,Length)-pair.
	/// </summary>
	public interface ISegment
	{
		/// <summary>
		/// Gets the start offset of the segment.
		/// </summary>
		int Offset { get; }
		
		/// <summary>
		/// Gets the length of the segment.
		/// </summary>
		/// <remarks>For line segments (IDocumentLine), the length does not include the line delimeter.</remarks>
		int Length { get; }
		
		/// <summary>
		/// Gets the end offset of the segment.
		/// </summary>
		/// <remarks>EndOffset = Offset + Length;</remarks>
		int EndOffset { get; }
	}

	public interface IWithSegment
	{
		ISegment Segment { get; }
	}

	public interface ISegmentWithVersion : ISegment
	{
		ITextSourceVersion Version { get; set; }
	}

	public interface IUpdateableSegment : ISegmentWithVersion
	{
		ITextSourceVersion SourceVersion { get; set; }
		ITextSourceVersion UpdatedVersion { get; set; }
		
		new int Offset { get; set; }
		new int Length { get; set; }
		new int EndOffset { get; set; }

		bool Update(TextDocument document, ITextSourceVersion newVersion);
	}

	/// <summary>
	/// Extension methods for <see cref="ISegment"/>.
	/// </summary>
	public static class ISegmentExtensions
	{
		/// <summary>
		/// Gets whether <paramref name="segment"/> fully contains the specified segment.
		/// </summary>
		/// <remarks>
		/// Use <c>segment.Contains(offset, 0)</c> to detect whether a segment (end inclusive) contains offset;
		/// use <c>segment.Contains(offset, 1)</c> to detect whether a segment (end exclusive) contains offset.
		/// </remarks>
		public static bool Contains (this ISegment segment, int offset, int length)
		{
			return segment.Offset <= offset && offset + length <= segment.EndOffset;
		}
		
		/// <summary>
		/// Gets whether <paramref name="thisSegment"/> fully contains the specified segment.
		/// </summary>
		public static bool Contains (this ISegment thisSegment, ISegment segment)
		{
			return segment != null && thisSegment.Offset <= segment.Offset && segment.EndOffset <= thisSegment.EndOffset;
		}

		public static bool Contains(this ISegment thisSegment, int offset)
		{
			return thisSegment != null && offset >= thisSegment.Offset && offset <= thisSegment.EndOffset;
		}

		/// <summary>
		/// Finds the first segment within the [start, end] text range searching the segments list over the segments [segmentStartIndex, segmentEndIndex] range
		/// </summary>
		public static int FindFirstIndexInRange<T>(this IList<T> segments, int segmentStartIndex, int segmentEndIndex, int start, int end)
			where T : IWithSegment
		{
			int low = segmentStartIndex;
			var len = segmentEndIndex - 1;
			int hi = len;

			T item = default;
			byte type = 0;
			ISegment segment = null;

			var index = -1;
			var isMatch = false;

			while (low <= hi)
			{
				index = low + (hi - low >> 1);
				int comparison;

				while (true)
				{
					segment = segments[index]?.Segment;
					if (segment == null || segment is INullSegment)
					{
						if (low == hi) break;
						index++;
						continue;
					}

					break;
				}

				if (segment == null) break;

				if (segment.Offset < start && segment.EndOffset < start)
				{
					comparison = -1;
				}
				else if (segment.Offset > end && segment.EndOffset > end)
				{
					comparison = 1;
				}
				else
				{
					isMatch = true;
					break;
				}

				if (comparison < 0)
					low = index + 1;
				else
					hi = index - 1;
			}

			if (!isMatch) return -1;

			for (var i = index - 1; i >= 0; --i)
			{
				segment = segments[i]?.Segment;
				if (segment == null) continue;

				if (segment.Offset < start && segment.EndOffset < start)
				{
					break;
				}

				if (segment.Offset > end && segment.EndOffset > end)
				{
					break;
				}

				// we have an earlier match...
				index = i;
			}

			return index;
		}

		/// <summary>
		/// Finds the first segment within the [start, end] text range searching the segments list over the segments [segmentStartIndex, segmentEndIndex] range
		/// </summary>
		public static int FindFirstIndexInRange<T>(this IList<T> segments, int segmentStartIndex, int segmentEndIndex, int start, int end, Func<T, TextRange> itemRangeFunc)
		{
			int low = segmentStartIndex;
			var len = segmentEndIndex - 1;
			int hi = len;

			T item = default;
			byte type = 0;
			var itemRange = TextRange.Empty;

			var index = -1;
			var isMatch = false;

			while (low <= hi)
			{
				index = low + (hi - low >> 1);
				int comparison;

				while (true)
				{
					itemRange = itemRangeFunc(segments[index]);
					if (itemRange.IsEmpty)
					{
						if (low == hi) break;
						index++;
						continue;
					}
				
					break;
				}
				
				if(itemRange.IsEmpty) break;
				
				if (itemRange.StartOffset < start && itemRange.EndOffset < start)
				{
					comparison = -1;
				}
				else if (itemRange.StartOffset > end && itemRange.EndOffset > end)
				{
					comparison = 1;
				}
				else
				{
					isMatch = true;
					break;
				}

				if (comparison < 0)
					low = index + 1;
				else
					hi = index - 1;
			}

			if (!isMatch) return -1;

			for (var i = index - 1; i >= 0; --i)
			{
				itemRange = itemRangeFunc(segments[i]);
				if (itemRange.IsEmpty) continue;
				
				if (itemRange.StartOffset < start && itemRange.EndOffset < start)
				{
					break;
				}

				if (itemRange.StartOffset > end && itemRange.EndOffset > end)
				{
					break;
				}

				// we have an earlier match...
				index = i;
			}

			return index;
		}
	}


	#endif
}
