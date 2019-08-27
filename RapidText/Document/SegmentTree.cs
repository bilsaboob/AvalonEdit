using System;
using System.Collections.Generic;
using System.Text;

namespace RapidText.Document
{
	/// <summary>
	/// Interface to allow TextSegments to access the TextSegmentCollection - we cannot use a direct reference
	/// because TextSegmentCollection is generic.
	/// </summary>
	public interface ISegmentTree
	{
		void Add(TextSegment s);
		void Remove(TextSegment s);
		void UpdateAugmentedData(TextSegment s);
	}
}
