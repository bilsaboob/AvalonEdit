using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Rendering;
using RapidText;
using RapidText.Document;
using RapidText.Utils;

namespace ZenIDE.RSharp.Components.Editor
{
	public interface ISegmentLineBuilderFactory
	{
		IEnumerable<ISegmentedDocumentLineBuilder> CreateBuilders(ITextSourceVersion preferredTextVersion);
	}

	public interface ISegmentedDocumentLineBuilder
	{
		ITextSourceVersion AvailableVersion { get; }

		void Begin(VisualLineConstructionContext c);
		void End(VisualLineConstructionContext c);

		void SetLineSegments(DocumentLine line, SegmentsCollection segments);
		SegmentsCollection GetLineSegments(DocumentLine line);
		void ClearLineSegments(DocumentLine line);

		SegmentsCollection SegmentLine(TextDocument document, DocumentLine line, ITextSourceVersion version);
	}

	public interface IWithLineBuilder<TBuilder, TSegment>
	{
		TBuilder Builder { get; }
	}

	public static class MarkExtensions
	{
		public static TSegment Mark<TBuilder, TSegment>(this IWithLineBuilder<TBuilder, TSegment> builder, TextRange range)
			where TSegment : IUpdateableSegment, new()
			where TBuilder: SegmentedDocumentLineBuilder<TSegment>
		{
			return builder.Builder.Mark(range);
		}

		public static TSegment Mark<TSegment>(this SegmentedDocumentLineBuilder<TSegment> builder, TextRange range)
			where TSegment : IUpdateableSegment, new()
		{
			if (range.IsEmpty) return builder.NullSegment;
			return builder.Mark(range.StartOffset, range.Length);
		}

		public static TSegment Mark<TBuilder, TSegment>(this IWithLineBuilder<TBuilder, TSegment> builder, ISegment segment)
			where TSegment : IUpdateableSegment, new()
			where TBuilder : SegmentedDocumentLineBuilder<TSegment>
		{
			return builder.Builder.Mark(segment);
		}

		public static TSegment Mark<TSegment>(this SegmentedDocumentLineBuilder<TSegment> builder, ISegment segment)
			where TSegment : IUpdateableSegment, new()
		{
			if (segment == null) return builder.NullSegment;
			return builder.Mark(segment.Offset, segment.Length);
		}

		public static TSegment Mark<TBuilder, TSegment>(this IWithLineBuilder<TBuilder, TSegment> builder, int offset, int length)
			where TSegment : IUpdateableSegment, new()
			where TBuilder : SegmentedDocumentLineBuilder<TSegment>
		{
			return builder.Builder.Mark(offset, length);
		}

		public static TSegment Mark<TSegment>(this SegmentedDocumentLineBuilder<TSegment> builder, int offset, int length)
			where TSegment : IUpdateableSegment, new()
		{
			if (offset < 0 || length <= 0) return builder.NullSegment;
			var segment = builder.TryAddSegment(offset, length);
			if (segment == null) return builder.NullSegment;
			return segment;
		}
	}

	public abstract class SegmentedDocumentLineBuilder<TSegment> : ISegmentedDocumentLineBuilder
		where TSegment : IUpdateableSegment, new()
	{
		private static readonly TSegment _nullSegment = new TSegment();
		public TSegment NullSegment => _nullSegment;

		private bool _isSegmenting;
		private Type _thisType;

		protected SegmentedDocumentLineBuilder()
		{
			_thisType = GetType();
		}

		public ITextSourceVersion AvailableVersion { get; set; }

		protected TextDocument TextDocument { get; set; }
		protected ITextSourceVersion TextVersion { get; set; }
		protected DocumentLine Line { get; set; }
		protected SegmentsCollection Segments { get; set; }

		public ISegment Range { get; protected set; }
		public bool CheckRangeForSegments { get; set; }
		public bool IsOutsideRange { get; set; }
		protected bool IsInsideRange { get; set; }

		public virtual void Begin(VisualLineConstructionContext c) { }

		public virtual void End(VisualLineConstructionContext c) { }

		public SegmentsCollection GetLineSegments(DocumentLine line)
		{
			if (!line.TryGet($"Segments_{_thisType.Name}", out SegmentsCollection segments))
				return null;
			return segments;
		}

		public void SetLineSegments(DocumentLine line, SegmentsCollection segments)
		{
			line.Set($"Segments_{_thisType.Name}", segments);
		}

		public void ClearLineSegments(DocumentLine line)
		{
			line.Remove($"Segments_{_thisType.Name}");
		}

		protected SegmentsCollection CreateSegments()
		{
			return new SegmentsCollection($"Segments_{_thisType.Name}", AvailableVersion);
		}

		public virtual SegmentsCollection SegmentLine(TextDocument document, DocumentLine line, ITextSourceVersion textVersion)
		{
			if (_isSegmenting) throw new Exception("Cannot segment asynchronously");
			_isSegmenting = true;

			try
			{
				TextDocument = document;
				TextVersion = textVersion;
				Range = GetRange(line, textVersion);
				Line = line;
				Segments = CreateSegments();

				SegmentLine();

				return Segments;
			}
			finally
			{
				TextDocument = null;
				TextVersion = null;
				Line = null;
				Range = null;
				Segments = null;
				_isSegmenting = false;
			}
		}

		private ISegment GetRange(DocumentLine line, ITextSourceVersion version)
		{
			if (AvailableVersion == version) return line;

			var startOffset = line.Offset;
			var endOffset = line.EndOffset;
			version.UpdateRange(AvailableVersion, ref startOffset, ref endOffset);
			return new TextSegment() {
				StartOffset = startOffset,
				EndOffset = endOffset
			};
		}

		protected virtual void SegmentLine()
		{
			// segment the line
		}

		#region Segment helpers
		public TSegment TryAddSegment(int offset, int length)
		{
			var segment = default(TSegment);
			if (!AddSegment(offset, length, ref segment))
				return NullSegment;
			return segment;
		}

		public bool TryAddSegment(TSegment segment)
		{
			if (segment is INullSegment) return false;

			if (!AddSegment(segment.Offset, segment.EndOffset, ref segment))
				return false;

			return true;
		}

		protected bool AddSegment(int offset, int length, ref TSegment segment)
		{
			if (CheckRangeForSegments)
			{
				if (IsOutsideRange) return false;

				var isInRange = IsInRange(offset, length, out var isPastRange);
				if (!isInRange)
				{
					if (isPastRange)
						IsOutsideRange = true;

					return false;
				}
				else
				{
					IsInsideRange = true;
				}
			}

			// adjust the range
			var start = offset;
			if (Range != null)
				start = start.CoerceValue(Range.Offset, Range.EndOffset);

			var end = start + length;
			if (Range != null)
				end = end.CoerceValue(Range.Offset, Range.EndOffset);

			var len = end - start;
			if (len <= 0) return false;

			if (segment == null)
				segment = new TSegment();
			
			UpdateSegment(ref segment, start, end);

			Segments?.Add(segment);

			return true;
		}

		protected virtual void UpdateSegment(ref TSegment segment, int start, int end)
		{
			if (segment == null) return;

			// set the offsets
			segment.Offset = start;
			segment.EndOffset = end;

			// set the versions
			segment.Version = AvailableVersion;
			segment.SourceVersion = AvailableVersion;
		}
		#endregion

		#region Range helpers
		protected virtual bool CanContinueSegmenting(TextRange range)
		{
			if (range.IsEmpty) return true;

			if (CheckRangeForSegments)
			{
				if (IsInsideRange) return false;

				var isInRange = IsInRange(range.StartOffset, range.Length, out var isPastRange);
				if (!isInRange)
				{
					if (isPastRange)
					{
						IsOutsideRange = true;
						return false;
					}
				}
				else
				{
					IsInsideRange = true;
				}
			}

			return true;
		}

		protected virtual bool IsInRange(TextRange range)
		{
			if (range.IsEmpty) return false;
			return IsInRange(range.StartOffset, range.Length, out _);
		}

		protected virtual bool IsInRange(int offset, int length, out bool isPastRange)
		{
			isPastRange = false;
			var end = offset + length;

			if (offset < Range.Offset && end < Range.Offset) return false;
			if (offset > Range.EndOffset && end > Range.EndOffset)
			{
				isPastRange = true;
				return false;
			}

			return true;
		}
		#endregion

		#region Process helpers
		protected void Process(Action processAction)
		{
			Process(false, processAction);
		}
		protected void Process(bool checkSegmentsRange, Action processAction)
		{
			IsOutsideRange = false;
			IsInsideRange = false;
			CheckRangeForSegments = checkSegmentsRange;

			processAction?.Invoke();
		}

		protected void ProcessEachInRange<T>(IList<T> items, Func<T, bool> processItemAction)
			where T : IWithSegment
		{
			var range = Range;
			var rangeStartOffset = range.Offset;
			var rangeEndOffset = range.EndOffset;

			// iterate over the tokens and colorize those

			var i = items.FindFirstIndexInRange(0, items.Count, rangeStartOffset, rangeEndOffset);
			if (i < 0) return;

			for (; i < items.Count; ++i)
			{
				var item = items[i];
				if (item == null) continue;

				var segment = item?.Segment;
				if (segment.Offset < rangeStartOffset && segment.EndOffset < rangeStartOffset) continue;
				if (segment.Offset > rangeEndOffset && segment.EndOffset > rangeEndOffset)
				{
					IsOutsideRange = true;
					break;
				}

				if (!IsInsideRange)
				{
					IsInsideRange = true;
				}

				if (!processItemAction(item))
					break;
			}
		}

		protected void ProcessEachInRange<T>(IList<T> items, Func<T, TextRange> itemRangeFunc, Func<T, bool> processItemAction)
		{
			var range = Range;
			var rangeStartOffset = range.Offset;
			var rangeEndOffset = range.EndOffset;

			// iterate over the tokens and colorize those

			var i = items.FindFirstIndexInRange(0, items.Count, rangeStartOffset, rangeEndOffset, itemRangeFunc);
			if (i < 0) return;

			for (; i < items.Count; ++i)
			{
				var item = items[i];
				if (item == null) continue;

				var itemRange = itemRangeFunc(item);
				if (itemRange.StartOffset < rangeStartOffset && itemRange.EndOffset < rangeStartOffset) continue;
				if (itemRange.StartOffset > rangeEndOffset && itemRange.EndOffset > rangeEndOffset)
				{
					IsOutsideRange = true;
					break;
				}

				if (!IsInsideRange)
				{
					IsInsideRange = true;
				}

				if (!processItemAction(item))
					break;
			}
		}

		protected int FindFirstIndexInRange<T>(IList<T> items, Func<T, TextRange> itemRangeFunc)
		{
			var range = Range;
			var rangeStartOffset = range.Offset;
			var rangeEndOffset = range.EndOffset;
			return items.FindFirstIndexInRange(0, items.Count, rangeStartOffset, rangeEndOffset, itemRangeFunc);
		}

		protected int FindLastIndexInRange<T>(int startItemIndex, IList<T> items, Func<T, TextRange> itemRangeFunc)
		{
			var range = Range;
			var rangeStartOffset = range.Offset;
			var rangeEndOffset = range.EndOffset;

			var i = startItemIndex;
			for (; i < items.Count; ++i)
			{
				var itemRange = itemRangeFunc(items[i]);
				if (itemRange.StartOffset < rangeStartOffset && itemRange.EndOffset < rangeStartOffset) continue;
				if (itemRange.StartOffset > rangeEndOffset && itemRange.EndOffset > rangeEndOffset)
				{
					break;
				}
			}

			return i;
		}
		#endregion
	}
}