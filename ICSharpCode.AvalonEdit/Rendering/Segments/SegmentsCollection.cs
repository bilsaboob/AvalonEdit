using System.Collections;
using System.Collections.Generic;
using RapidText.Document;

namespace ZenIDE.RSharp.Components.Editor
{
	public class SegmentsCollection : IEnumerable<ISegment>
	{
		private List<ISegment> _segments;

		public SegmentsCollection(string id, ITextSourceVersion version)
		{
			Id = id;
			TextVersion = version;
			_segments = new List<ISegment>();
		}

		public string Id { get; }

		public ITextSourceVersion TextVersion { get; set; }

		public int Count => _segments.Count;

		public void Add(ISegment segment)
		{
			_segments.Add(segment);
		}

		public IEnumerator<ISegment> GetEnumerator() => _segments.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}