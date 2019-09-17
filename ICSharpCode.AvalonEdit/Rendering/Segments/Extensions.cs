using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Rendering;

namespace ZenIDE.RSharp.Components.Editor
{
	public static class VisualLineConstructionContextExtensions
	{
		#region Segment builders
		public static List<ISegmentedDocumentLineBuilder> GetOrCreateSegmentBuilders(this VisualLineConstructionContext c, ISegmentLineBuilderFactory factory)
		{
			bool created = false;
			var builders = c.GetOrAdd("SegmentBuilders", () => {
				created = true;
				return factory?.CreateBuilders(c.TextVersion)?.ToList();
			});

			if (builders != null && created)
			{
				foreach (var builder in builders)
				{
					try
					{
						builder.Begin(c);
					}
					catch (Exception) { }
				}
			}

			return builders;
		}

		public static List<ISegmentedDocumentLineBuilder> GetSegmentBuilders(this VisualLineConstructionContext c)
		{
			if (c == null) return null;

			if (!c.TryGet("SegmentBuilders", out List<ISegmentedDocumentLineBuilder> builders))
				return null;

			return builders;
		}
		#endregion
	}
}